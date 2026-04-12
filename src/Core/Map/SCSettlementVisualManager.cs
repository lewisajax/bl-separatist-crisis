using HarmonyLib;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace SeparatistCrisis.Map
{
    // - We inherit from SettlementVisualManager
    // - We use a transpiler on MapScreen.InitializeVisuals to swap out SettlementVisualManager for this
    // - SettlementVisualManager.Current returns the entity component which will be this

    public class SCSettlementVisualManager: SettlementVisualManager
    {
        protected static FieldInfo SettlementVisualsField = AccessTools.Field(typeof(SettlementVisualManager), "_settlementVisuals");
        protected static FieldInfo VisualsFlattenedField = AccessTools.Field(typeof(SettlementVisualManager), "_visualsFlattened");

        public override void OnTick(float realDt, float dt)
        {
            base.OnTick(realDt, dt);
        }

        protected override void OnInitialize()
        {
            Dictionary<PartyBase, SettlementVisual> settVisuals = (Dictionary<PartyBase, SettlementVisual>)SettlementVisualsField.GetValue(this);
            List<SettlementVisual> visFlattened = (List<SettlementVisual>)VisualsFlattenedField.GetValue(this);

            foreach (Settlement settlement in Settlement.All)
            {
                SCSettlementVisual settlementVisual = new SCSettlementVisual(settlement.Party);
                settlementVisual.OnStartup();
                settVisuals.Add(settlement.Party, settlementVisual);
                visFlattened.Add(settlementVisual);
            }
        }
    }
}
