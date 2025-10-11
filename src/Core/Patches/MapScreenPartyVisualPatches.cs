using HarmonyLib;
using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SandBox.ViewModelCollection.Nameplate;
using SeparatistCrisis.PartyVisuals;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.Patches
{
    public class MapScreenPartyVisualPatches : PatchClass<MapScreenPartyVisualPatches, MapScreen>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Transpiler(nameof(InitializeVisualsTranspiler), "InitializeVisuals"),
            new Prefix(nameof(StepSoundsPrefix), "StepSounds")
        };

        private static IEnumerable<CodeInstruction> InitializeVisualsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo addEntityOperand = AccessTools.Method(typeof(SandBoxViewVisualManager), "AddEntityComponent", null, new Type[] { typeof(MobilePartyVisualManager) });
            MethodInfo updatedOperand = AccessTools.Method(typeof(SandBoxViewVisualManager), "AddEntityComponent", null, new Type[] { typeof(SCMobilePartyVisualManager) });

            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == addEntityOperand)
                {
                    code.operand = updatedOperand;
                }
            }
            return instructions;
        }

        // This might be a performance issue
        private static bool StepSoundsPrefix(MapScreen __instance, MobileParty party)
        {
            if (party.IsVisible && party.MemberRoster.TotalManCount > 0)
            {
                SCMobilePartyVisual partyVisual = SCMobilePartyVisualManager.Current.GetPartyVisual(party.Party);
                if (partyVisual.HumanAgentVisuals != null)
                {
                    TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);
                    AgentVisuals agentVisuals = null;
                    TerrainTypeSoundSlot soundType = TerrainTypeSoundSlot.Dismounted;
                    if (partyVisual.CaravanMountAgentVisuals != null)
                    {
                        soundType = TerrainTypeSoundSlot.Caravan;
                        agentVisuals = partyVisual.CaravanMountAgentVisuals;
                    }
                    else if (partyVisual.HumanAgentVisuals != null)
                    {
                        if (partyVisual.MountAgentVisuals != null)
                        {
                            soundType = TerrainTypeSoundSlot.Mounted;
                            if (party.Army != null && party.AttachedParties.Count > 0)
                            {
                                soundType = TerrainTypeSoundSlot.ArmyMounted;
                            }
                            agentVisuals = partyVisual.MountAgentVisuals;
                        }
                        else
                        {
                            soundType = TerrainTypeSoundSlot.Dismounted;
                            if (party.Army != null && party.AttachedParties.Count > 0)
                            {
                                soundType = TerrainTypeSoundSlot.ArmyDismounted;
                            }
                            agentVisuals = partyVisual.HumanAgentVisuals;
                        }
                    }
                    if (party.AttachedTo == null)
                    {
                        MBMapScene.TickStepSound(__instance.MapScene, agentVisuals.GetVisuals(), (int)faceTerrainType, soundType, party.AttachedParties.Count);
                    }
                }
            }

            return false;
        }
    }
}
