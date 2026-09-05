using System;
using System.Collections.Generic;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.Party;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch that suppresses a NullReferenceException thrown by
    /// AiPatrollingBehavior.AiHourlyTick for custom patrol parties that aren't
    /// PatrolPartyComponent-based.
    /// </summary>
    public sealed class AiPatrollingBehaviorSafetyPatch : PatchClass<AiPatrollingBehaviorSafetyPatch, AiPatrollingBehavior>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Finalizer(nameof(AiHourlyTickFinalizer), "AiHourlyTick");
        }

        /// <summary>
        /// Swallows any exception thrown by AiHourlyTick for the given party, logging
        /// it instead of letting it propagate.
        /// </summary>
        private static Exception AiHourlyTickFinalizer(Exception __exception, MobileParty mobileParty)
        {
            if (__exception != null)
            {
                BountyLogger.Log($"[Harmony/AiPatrollingBehaviorSafetyPatch] Suppressed exception from AiPatrollingBehavior.AiHourlyTick for party '{mobileParty?.StringId}': {__exception}");
            }

            return null;
        }
    }
}
