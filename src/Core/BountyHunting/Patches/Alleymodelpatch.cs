using System;
using System.Collections.Generic;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch on DefaultAlleyModel.GetTroopsOfAIOwnedAlley — the method
    /// AlleyCampaignBehavior calls to decide who actually spawns as agents in an
    /// AI-owned alley. Vanilla only ever generates anonymous thug troops from this
    /// method; the alley's owner Hero is never included as a combatant (only
    /// player-owned alleys track their leader as a fightable agent, via a separate
    /// mechanism this method doesn't touch). For an alley bounty, we need the
    /// bounty hero to actually show up and be fightable, so this patch substitutes
    /// a custom roster — the hero plus this bounty's guard troops — whenever the
    /// alley in question is currently claimed by an active alley bounty.
    /// </summary>
    public sealed class AlleyModelPatch : PatchClass<AlleyModelPatch, DefaultAlleyModel>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Postfix(nameof(GetTroopsOfAIOwnedAlleyPostfix), "GetTroopsOfAIOwnedAlley");
        }

        /// <summary>
        /// Replaces the result with a roster containing the bounty hero plus this
        /// bounty's guard troops, if the given alley is currently claimed by an
        /// active alley bounty. Leaves vanilla's result untouched otherwise.
        /// </summary>
        private static void GetTroopsOfAIOwnedAlleyPostfix(Alley alley, ref TroopRoster __result)
        {
            var behavior = Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>();
            var bounty = behavior?.FindActiveAlleyBountyForAlley(alley);
            if (bounty?.TargetHero?.CharacterObject == null)
            {
                return;
            }

            var customRoster = TroopRoster.CreateDummyTroopRoster();
            customRoster.AddToCounts(bounty.TargetHero.CharacterObject, 1, false, 0, 0, true, -1);

            var guardTroop = MBObjectManager.Instance.GetObject<CharacterObject>(bounty.TroopId);
            int guardCount = bounty.GuardPartySize > 0 ? bounty.GuardPartySize : 4;

            if (guardTroop != null)
            {
                customRoster.AddToCounts(guardTroop, guardCount, false, 0, 0, true, -1);
            }
            else
            {
                BountyLogger.Log($"[Harmony/AlleyModelPatch] guard TroopId '{bounty.TroopId}' did not resolve — alley bounty '{bounty.TargetHero.Name}' will have no guards, only the boss.");
            }

            __result = customRoster;
            BountyLogger.Log($"[Harmony/AlleyModelPatch] Injected bounty roster for alley '{alley.Tag}' at '{alley.Settlement?.Name}' — hero '{bounty.TargetHero.Name}' + {guardCount}x '{bounty.TroopId}'.");
        }
    }
}