using System;
using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch on MissionAlleyHandler.OnAlleyFightEnd. Vanilla's own version
    /// of this method, on a player win, always shows a "take over the alley or
    /// leave it empty" inquiry — appropriate for an ordinary NPC gang leader, but
    /// wrong for a bounty target, who should be captured instead. This prefix
    /// intercepts a win against a bounty-claimed alley, runs the mod's own capture
    /// handling, and skips vanilla's inquiry entirely. A loss, or a fight against
    /// any non-bounty alley, falls through to vanilla's normal behavior unchanged.
    /// </summary>
    /// <remarks>
    /// OnAlleyFightEnd itself receives no reference to which alley was fought —
    /// vanilla's own alley dialogue/action code universally relies on
    /// CampaignMission.Current.LastVisitedAlley for this same purpose (see
    /// AlleyCampaignBehavior's alley_talk_* methods), so this patch does the same.
    /// </remarks>
    public sealed class AlleyFightEndPatch : PatchClass<AlleyFightEndPatch, MissionAlleyHandler>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Prefix(nameof(OnAlleyFightEndPrefix), "OnAlleyFightEnd");
        }

        /// <summary>
        /// Captures the bounty target and skips vanilla's take-over/leave-empty
        /// prompt when the player won a fight against a bounty-claimed alley.
        /// Returns true (run vanilla normally) for a loss, a missing alley
        /// reference, or any alley not currently claimed by an active bounty.
        /// </summary>
        private static bool OnAlleyFightEndPrefix(bool isPlayerSideWon)
        {
            if (!isPlayerSideWon)
            {
                return true;
            }

            var alley = CampaignMission.Current?.LastVisitedAlley;
            if (alley == null)
            {
                BountyLogger.Log("[Harmony/AlleyFightEndPatch] Player won an alley fight, but CampaignMission.Current.LastVisitedAlley was null — cannot check for a bounty, letting vanilla handle it.");
                return true;
            }

            var behavior = Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>();
            var bounty = behavior?.FindActiveAlleyBountyForAlley(alley);
            if (bounty == null)
            {
                return true;
            }

            BountyLogger.Log($"[Harmony/AlleyFightEndPatch] Player won alley fight against bounty target '{bounty.TargetHero?.Name}' in alley '{alley.Tag}' at '{alley.Settlement?.Name}' — capturing instead of showing vanilla take-over prompt.");
            behavior.HandleAlleyBountyCaptured(bounty, alley);

            return false;
        }
    }
}