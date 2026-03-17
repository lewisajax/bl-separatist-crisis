using HarmonyLib;
using SandBox.View;
using SandBox.View.Map.Managers;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics.Patches
{
    // We'll need to look at collecting patches from the assemblies rather than add them all in the patch manager
    public class TacticsTeamPatches : PatchClass<TacticsTeamPatches, Team>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Transpiler(nameof(InitializeTranspiler), "Initialize"),
            new Transpiler(nameof(GetOrderControllerOfTranspiler), "GetOrderControllerOf"),
        };

        private static IEnumerable<CodeInstruction> InitializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo orderControllerCtor = AccessTools.Constructor(typeof(OrderController), new Type[] { typeof(Mission), typeof(Team), typeof(Agent) });
            ConstructorInfo scControllerCtor = AccessTools.Constructor(typeof(SCOrderController), new Type[] { typeof(Mission), typeof(Team), typeof(Agent) });

            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Newobj && (ConstructorInfo)code.operand == orderControllerCtor)
                {
                    code.operand = scControllerCtor;
                }
            }
            return instructions; 
        }

        // Could just reuse the above
        private static IEnumerable<CodeInstruction> GetOrderControllerOfTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo orderControllerCtor = AccessTools.Constructor(typeof(OrderController), new Type[] { typeof(Mission), typeof(Team), typeof(Agent) });
            ConstructorInfo scControllerCtor = AccessTools.Constructor(typeof(SCOrderController), new Type[] { typeof(Mission), typeof(Team), typeof(Agent) });

            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Newobj && (ConstructorInfo)code.operand == orderControllerCtor)
                {
                    code.operand = scControllerCtor;
                }
            }
            return instructions;
        }
    }
}
