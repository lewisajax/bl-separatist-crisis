using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.MissionLogics;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch on MissionAlleyHandler.CheckAndTriggerConversationWithRivalThug.
    /// Vanilla's version picks whichever alley member the player is near and has
    /// line of sight to first — it has no concept of "the boss" at all, because in
    /// vanilla's own normal case an AI-owned alley's owner Hero is never physically
    /// spawned as an agent, only anonymous thugs (see AlleyModelPatch). Since our
    /// alley bounties DO spawn the owner as a real, fightable agent, this patch
    /// reimplements the same trigger logic but redirects the conversation to the
    /// bounty's boss agent specifically whenever the triggered alley belongs to an
    /// active alley bounty, so the player talks to the actual named target instead
    /// of a random guard. Non-bounty alleys behave identically to vanilla.
    /// </summary>
    /// <remarks>
    /// This only changes who the player has the pre-fight chit-chat with — the
    /// fight itself (StartCommonAreaBattle) already gathers every agent tagged
    /// MemberOfAlley == alley regardless of who initiated the conversation, so the
    /// boss was always included in the fight and capture either way. This patch is
    /// purely a presentation fix, not a functional one.
    ///
    /// The two injected private fields are typed as plain `object` rather than
    /// their real declared types (Dictionary&lt;Agent, AgentNavigator&gt; and
    /// Dictionary&lt;Alley, bool&gt;) deliberately — AgentNavigator's actual
    /// namespace/accessibility didn't match what the decompiled source implied, so
    /// rather than guess again, this avoids needing a compile-time reference to
    /// that type at all. The first dictionary's values are accessed via `dynamic`
    /// (resolved by the runtime, not the compiler) purely to call MemberOfAlley and
    /// CanSeeAgent on them.
    /// </remarks>
    public sealed class AlleyThugConversationPatch : PatchClass<AlleyThugConversationPatch, MissionAlleyHandler>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Prefix(nameof(CheckAndTriggerConversationWithRivalThugPrefix), "CheckAndTriggerConversationWithRivalThug");
        }

        /// <summary>
        /// Reimplements vanilla's candidate-selection loop, substituting the alley
        /// bounty's boss agent for the found candidate whenever the alley belongs to
        /// an active alley bounty. Always returns false — this fully replaces the
        /// original method rather than running alongside it.
        /// </summary>
        private static bool CheckAndTriggerConversationWithRivalThugPrefix(
            object ____rivalThugAgentsAndAgentNavigators,
            object ____conversationTriggeredAlleys)
        {
            if (Campaign.Current.ConversationManager.IsConversationFlowActive || Agent.Main == null)
            {
                return false;
            }

            var behavior = Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>();

            var rivalThugsDict = (IDictionary)____rivalThugAgentsAndAgentNavigators;
            var triggeredAlleysDict = (IDictionary)____conversationTriggeredAlleys;

            foreach (DictionaryEntry entry in rivalThugsDict)
            {
                var candidateAgent = (Agent)entry.Key;
                dynamic navigator = entry.Value;
                Alley alley = (Alley)navigator.MemberOfAlley;

                if (!candidateAgent.IsActive())
                {
                    continue;
                }
                if (alley == null || !triggeredAlleysDict.Contains(alley) || (bool)triggeredAlleysDict[alley])
                {
                    continue;
                }
                if (candidateAgent.GetDistanceTo(Agent.Main) >= 5f || !(bool)navigator.CanSeeAgent(Agent.Main))
                {
                    continue;
                }

                Agent agentToTalkTo = candidateAgent;

                var bounty = behavior?.FindActiveAlleyBountyForAlley(alley);
                if (bounty?.TargetHero?.CharacterObject != null)
                {
                    var bossAgent = Mission.Current.Agents.FirstOrDefault(a =>
                        a.IsActive() && a.Character == bounty.TargetHero.CharacterObject);

                    if (bossAgent != null)
                    {
                        agentToTalkTo = bossAgent;
                        BountyLogger.Log($"[Harmony/AlleyThugConversationPatch] Redirecting alley conversation to bounty boss '{bounty.TargetHero.Name}' (alley '{alley.Tag}') instead of a random thug.");
                    }
                    else
                    {
                        BountyLogger.Log($"[Harmony/AlleyThugConversationPatch] Alley '{alley.Tag}' belongs to bounty target '{bounty.TargetHero.Name}', but no active Agent for them was found — falling back to the random thug.");
                    }
                }

                Mission.Current.GetMissionBehavior<MissionConversationLogic>().StartConversation(agentToTalkTo, false, false);
                triggeredAlleysDict[alley] = true;
                break;
            }

            return false;
        }
    }
}