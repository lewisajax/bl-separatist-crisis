using System.Collections.Generic;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Roster;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch on PlayerEncounter.DoCaptureHeroes that intercepts bounty-target
    /// captures to show custom dialogue instead of vanilla's generic captured-lord
    /// line, and handles heroes whose death was vetoed rather than captured normally.
    /// </summary>
    public sealed class DoCaptureHeroesPatch : PatchClass<DoCaptureHeroesPatch, PlayerEncounter>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Prefix(nameof(DoCaptureHeroesPrefix), "DoCaptureHeroes");
            yield return new Postfix(nameof(DoCaptureHeroesPostfix), "DoCaptureHeroes");
        }

        /// <summary>
        /// Removes a bounty target from vanilla's captured-heroes queue and runs the
        /// mod's own capture handling and dialogue instead.
        /// </summary>
        private static bool DoCaptureHeroesPrefix(PlayerEncounter __instance,
                           ref List<TroopRosterElement> ____capturedHeroes,
                           ref bool ____stateHandled)
        {
            var behavior = Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>();

            if (behavior != null && behavior.IsBountyConversationInProgress)
            {
                ____stateHandled = true;
                return false;
            }

            if (____capturedHeroes == null || ____capturedHeroes.Count == 0)
            {
                return true;
            }

            if (behavior == null)
            {
                return true;
            }

            var lastElement = ____capturedHeroes[____capturedHeroes.Count - 1];
            var hero = lastElement.Character?.HeroObject;

            if (hero == null || !behavior.HasActiveBountyOn(hero))
            {
                return true;
            }

            ____capturedHeroes.RemoveAt(____capturedHeroes.Count - 1);
            behavior.HandleBountyTargetCapturedInBattle(hero);

            ____stateHandled = true;

            return false;
        }

        /// <summary>
        /// Catches vanilla's transition to FreeHeroes and, if a bounty conversation is
        /// still in progress or a death-veto capture is pending, holds or reverses the
        /// state so those resolve before the loot/prisoner screens open.
        /// </summary>
        private static void DoCaptureHeroesPostfix(PlayerEncounter __instance,
                             ref PlayerEncounterState ____mapEventState,
                             ref bool ____stateHandled)
        {
            if (____mapEventState != PlayerEncounterState.FreeHeroes)
            {
                return;
            }

            var behavior = Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>();
            if (behavior == null)
            {
                return;
            }

            if (behavior.IsBountyConversationInProgress)
            {
                ____mapEventState = PlayerEncounterState.CaptureHeroes;
                ____stateHandled = true;
                return;
            }

            var pendingHero = behavior.TakeNextPendingDeathVetoCapture();
            if (pendingHero == null)
            {
                return;
            }

            behavior.HandleBountyTargetCapturedInBattle(pendingHero);

            ____mapEventState = PlayerEncounterState.CaptureHeroes;
            ____stateHandled = true;
        }
    }
}