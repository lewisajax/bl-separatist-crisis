using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using HarmonyLib;
using SeparatistCrisis.PatchTools;

namespace SeparatistCrisis.Patches
{
    // Cancels out the backstory that gets added. Since it relies on extra lords and towns that were deleted by the xslt
    public class OnNewGameCreatedPatch: PatchClass<OnNewGameCreatedPatch, BackstoryCampaignBehavior>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(OnNewGameCreatedPrefix), "OnNewGameCreated")
        };

        private static bool OnNewGameCreatedPrefix(CampaignGameStarter campaignGameStarter)
        {
            return false;
        }
    }

    public class OnNewGameCreatedPartialFollowUpPatch: PatchClass<OnNewGameCreatedPartialFollowUpPatch, InitialChildGenerationCampaignBehavior>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(OnNewGameCreatedPartialFollowUpPrefix), "OnNewGameCreatedPartialFollowUp")
        };

        private static bool OnNewGameCreatedPartialFollowUpPrefix(CampaignGameStarter starter, int index)
        {
            return false;
        }
    }
}
