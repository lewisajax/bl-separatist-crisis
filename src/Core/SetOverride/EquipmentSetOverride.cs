using HarmonyLib;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.SetOverride
{
    public static class EquipmentSetOverride
    {
        // There might be better performance if we use it as a delegate
        public static MethodInfo AllEquipments = AccessTools.PropertyGetter(typeof(BasicCharacterObject), "AllEquipments");

        public static void AssignEquipment(AgentBuildData agentBuildData)
        {
            CustomBattleAgentOrigin origin = (CustomBattleAgentOrigin)agentBuildData.AgentOrigin;
            BasicCultureObject culture = origin.CustomBattleCombatant.BasicCulture;
            BasicCharacterObject? leader = origin.CustomBattleCombatant.General;
            if (leader == null && agentBuildData.AgentTeam.Leader != null) leader = agentBuildData.AgentTeam.Leader.Character;

            // If a troop does not allow for fixed sets
            // I can't see the AssignedSet list growing to high numbers so we probably don't need to cache the char object
            if (AssignedSet.Find(agentBuildData.AgentCharacter) == null) return;

            if (culture != null && leader != null)
            {
                // We get the index of the equipmentroster or equipmentset that can be find in the character xml
                int setIndex = SetAssignments.Instance.GetSetIndex(leader, culture);

                MBReadOnlyList<Equipment> allEquipment = (MBReadOnlyList<Equipment>)EquipmentSetOverride.AllEquipments.Invoke(agentBuildData.AgentCharacter, new object[] { });
                Equipment? equipmentSet = allEquipment?.Count >= setIndex ? allEquipment[setIndex].Clone() : null;
                if (equipmentSet != null)
                {
                    agentBuildData.Equipment(equipmentSet);
                }
            }
        }
    }
}
