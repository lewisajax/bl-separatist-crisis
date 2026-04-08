using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SeparatistCrisis.PatchTools;
using SeparatistCrisis.SetOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Patches
{
    public class SpawnAgentPatch : PatchClass<SpawnAgentPatch, Mission>
    {
        public static MethodInfo AllEquipments = AccessTools.PropertyGetter(typeof(BasicCharacterObject), "AllEquipments");

        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Transpiler(nameof(SpawnAgentTranspiler), "SpawnAgent"),
        };

        private static IEnumerable<CodeInstruction> SpawnAgentTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Need to get the AgentBuildData param
            // ldarg.1 should be the agentBuildData param
            // Use a yield return inside a foreach to insert instructions before the current one

            // .Insert inserts the list of instructions before the current position
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchStartForward(
                    CodeMatch.IsLdarg(1)
                ).ThrowIfInvalid("Ldarg.1 could not be found")
                .Insert(new CodeInstruction[] {
                    CodeInstruction.LoadArgument(1),
                    CodeInstruction.Call(() => EquipmentSetOverride.AssignEquipment(default))
                });

            return matcher.Instructions();
        }
    }
}
