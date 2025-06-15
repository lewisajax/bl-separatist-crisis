using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using SeparatistCrisis.PatchTools;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.Patches
{
    public class InitializeSandboxXMLsPatch : PatchClass<InitializeSandboxXMLsPatch, SandBoxManager>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(InitializeSandboxXMLsPostfix), "InitializeSandboxXMLs")
        };

        private static void InitializeSandboxXMLsPostfix(bool isSavedCampaign)
        {
            if (Campaign.Current.GameMode == CampaignGameMode.Campaign && !Game.Current.IsEditModeOn)
            {
                MBObjectManager.Instance.LoadXML("SettlementGroups", false);
            }
        }
    }
}
