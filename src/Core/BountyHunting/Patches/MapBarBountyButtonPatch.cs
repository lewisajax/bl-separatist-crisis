using System.Collections.Generic;
using System.Linq;
using SandBox.View.Map.Navigation;
using SeparatistCrisis.PatchTools;
using TaleWorlds.CampaignSystem;

namespace SeparatistCrisis.BountyHunting.Patches
{
    /// <summary>
    /// Harmony patch on MapNavigationHandler.OnCreateElements — inserts the bounty
    /// board button into the map's bottom-left navigation bar, just before the
    /// Kingdom button so Kingdom stays rightmost. OnCreateElements runs once from
    /// the handler's constructor, so this covers GetElements(), GetElement(id), and
    /// IsAnyElementActive() for free.
    /// </summary>
    public sealed class MapBarBountyButtonPatch : PatchClass<MapBarBountyButtonPatch, MapNavigationHandler>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            yield return new Postfix(nameof(OnCreateElementsPostfix), "OnCreateElements");
        }

        private static void OnCreateElementsPostfix(MapNavigationHandler __instance, ref INavigationElement[] __result)
        {
            var list = __result.ToList();

            int kingdomIndex = list.FindIndex(e => e.StringId == "kingdom");
            var bountyElement = new UI.BountyNavigationElement(__instance);

            if (kingdomIndex >= 0)
            {
                list.Insert(kingdomIndex, bountyElement);
            }
            else
            {
                list.Add(bountyElement);
            }

            __result = list.ToArray();

            BountyLogger.Log("[Harmony/MapBarBountyButtonPatch] Inserted BountyNavigationElement before Kingdom in the map nav bar.");
        }
    }
}