using HarmonyLib;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Patches
{
    public class TradeBoundPatch : PatchClass<TradeBoundPatch, DefaultVillageTradeModel>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Transpiler(nameof(TradeBoundTranspiler), "TradeBoundDistanceLimitAsDays"),
        };

        private static IEnumerable<CodeInstruction> TradeBoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_R4)
                ).ThrowIfInvalid("ldc.r4 could not be found")
                .SetOperandAndAdvance(1000.0f); // We just hardcode a number that will never be reached

            return matcher.Instructions();
        }
    }
}
