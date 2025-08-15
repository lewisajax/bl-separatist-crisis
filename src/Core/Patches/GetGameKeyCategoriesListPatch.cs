using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade.Options;
using SeparatistCrisis.PatchTools;
using HarmonyLib;

namespace SeparatistCrisis.Patches
{
    // Copied from TOR but I don't think we need it
    public class GetGameKeyCategoriesListPatch : PatchClass<GetGameKeyCategoriesListPatch>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(GetGameKeyCategoriesListPostfix), new Utils.Reflect.Method(typeof(OptionsProvider), "GetGameKeyCategoriesList", new Type[]{ typeof(bool) }))
        };

        private static IEnumerable<string> GetGameKeyCategoriesListPostfix(IEnumerable<string> __result)
        {
            __result.AddItem("SeparatistCrisis");
            return __result;
        }
    }
}
