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
            new Prefix(nameof(SpawnAgentPrefix), "SpawnAgent"),
        };

        //private static IEnumerable<CodeInstruction> SpawnAgentTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    // Need to get the AgentBuildData param
        //    // ldarg.1 should be the agentBuildData param
        //    // Use a yield return inside a foreach to insert instructions before the current one

        //    List<CodeInstruction> list = instructions.ToList();

        //    foreach (CodeInstruction instruction in list)
        //    {
        //        if (instruction.IsLdarg())
        //        {
        //            Console.WriteLine("Is Ldarg");
        //        }
        //    }

        //    //int index = list.FindIndex(code => code.Calls(SpawnAgentPatch.CreateAgentOperand));
        //    //if (index > 0 && index < instructions.Count())
        //    //{
        //    //    var localVar = list[index + 1].Clone();
        //    //    if (localVar.IsLdloc())
        //    //        list.InsertRange(index + 1, new[]
        //    //        {
        //    //                localVar,
        //    //                CodeInstruction.Call(() => DrawZombielandDifficultySettings(default))
        //    //            });
        //    //}
        //    return list.AsEnumerable<CodeInstruction>();
        //}

        private static bool SpawnAgentPrefix(ref AgentBuildData agentBuildData, bool spawnFromAgentVisuals = false)
        {
            // Use a transpiler for better performance
            // Set up a static method or something that we can call to handle all this

            // This is just for CustomGame
            // IAgentOriginBase origin = agentBuildData.AgentOrigin;
            //BasicCultureObject culture = origin.BattleCombatant.BasicCulture;
            //BasicCharacterObject? leader = origin.BattleCombatant.General;

            CustomBattleAgentOrigin origin = (CustomBattleAgentOrigin)agentBuildData.AgentOrigin;
            BasicCultureObject culture = origin.CustomBattleCombatant.BasicCulture;
            BasicCharacterObject? leader = origin.CustomBattleCombatant.General;
            if (leader == null && agentBuildData.AgentTeam.Leader != null) leader = agentBuildData.AgentTeam.Leader.Character;

            if (culture != null && leader != null)
            {
                int setIndex = SetAssignments.Instance.GetSetIndex(leader, culture);
                MBReadOnlyList<Equipment> allEquipment = (MBReadOnlyList<Equipment>)SpawnAgentPatch.AllEquipments.Invoke(agentBuildData.AgentCharacter, new object[]{});
                Equipment? equipmentSet = allEquipment?.Count >= setIndex ? allEquipment[setIndex].Clone() : null;
                if (equipmentSet != null)
                {
                    agentBuildData.Equipment(equipmentSet);
                }
            }

            return true;
        }
    }
}
