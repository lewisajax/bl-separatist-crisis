using System;
using System.Collections.Generic;
using SeparatistCrisis.PatchTools;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch that suppresses a NullReferenceException thrown by
    /// Mission.CheckMissionEnded every tick after a peaceful mission has been forced
    /// into MissionMode.Battle for an in-place bounty fight.
    /// </summary>
    public sealed class CheckMissionEndedSafetyPatch : PatchClass<CheckMissionEndedSafetyPatch, Mission>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Finalizer(nameof(CheckMissionEndedFinalizer), "CheckMissionEnded");
        }

        /// <summary>
        /// Swallows any exception thrown by CheckMissionEnded, logging it instead of
        /// letting it propagate.
        /// </summary>
        private static Exception CheckMissionEndedFinalizer(Exception __exception)
        {
            if (__exception != null)
            {
                BountyLogger.Log($"[Harmony/CheckMissionEndedSafetyPatch] Suppressed exception from Mission.CheckMissionEnded: {__exception}");
            }

            return null;
        }
    }

    /// <summary>
    /// Harmony patch that replaces Mission.OnEndMissionRequest with a per-logic
    /// try/catch version, so one broken MissionLogic (surfaced by the same forced
    /// MissionMode.Battle) doesn't prevent the player from leaving the mission.
    /// </summary>
    public sealed class OnEndMissionRequestSafetyPatch : PatchClass<OnEndMissionRequestSafetyPatch, Mission>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Prefix(nameof(OnEndMissionRequestPrefix), "OnEndMissionRequest");
        }

        /// <summary>
        /// Re-implements OnEndMissionRequest's logic, isolating each MissionLogic's
        /// own call in its own try/catch and guaranteeing EndMission() still fires as
        /// a fallback if anything else goes wrong.
        /// </summary>
        private static bool OnEndMissionRequestPrefix(Mission __instance, ref BasicMissionTimer ____leaveMissionTimer)
        {
            try
            {
                if (__instance.MissionLogics != null)
                {
                    foreach (MissionLogic missionLogic in __instance.MissionLogics)
                    {
                        if (missionLogic == null) continue;

                        bool flag;
                        InquiryData inquiryData;
                        try
                        {
                            inquiryData = missionLogic.OnEndMissionRequest(out flag);
                        }
                        catch (Exception ex)
                        {
                            BountyLogger.Log($"[Harmony/OnEndMissionRequestSafetyPatch] Suppressed exception from {missionLogic.GetType().Name}.OnEndMissionRequest — skipping this one logic, continuing: {ex}");
                            continue;
                        }

                        if (!flag)
                        {
                            ____leaveMissionTimer = null;
                            return false;
                        }
                        if (inquiryData != null)
                        {
                            ____leaveMissionTimer = null;
                            InformationManager.ShowInquiry(inquiryData, true, false);
                            return false;
                        }
                    }
                }

                if (____leaveMissionTimer != null)
                {
                    if (____leaveMissionTimer.ElapsedTime > 0.6f)
                    {
                        ____leaveMissionTimer = null;
                        __instance.EndMission();
                        return false;
                    }
                }
                else
                {
                    ____leaveMissionTimer = new BasicMissionTimer();
                }
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"[Harmony/OnEndMissionRequestSafetyPatch] Unexpected exception in replicated logic — forcing EndMission() as a guaranteed fallback so the player isn't stuck: {ex}");
                try
                {
                    __instance.EndMission();
                }
                catch (Exception ex2)
                {
                    BountyLogger.Log($"[Harmony/OnEndMissionRequestSafetyPatch] EndMission() fallback ALSO threw: {ex2}");
                }
            }

            return false;
        }
    }
}
