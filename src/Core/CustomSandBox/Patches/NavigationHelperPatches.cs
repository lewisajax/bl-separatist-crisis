using HarmonyLib;
using Helpers;
using SandBox;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using static TaleWorlds.MountAndBlade.Launcher.Library.NativeMessageBox;

namespace SeparatistCrisis.CustomSandBox
{
    public class NavigationHelperPatches: PatchClass<NavigationHelperPatches>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(FindReachablePointAroundPosition), new Utils.Reflect.Method(typeof(NavigationHelper), "FindReachablePointAroundPosition", new Type[] { typeof(CampaignVec2), typeof(MobileParty.NavigationType), typeof(float), typeof(float), typeof(bool) })),
            new Prefix(nameof(GetFaceIndex), new Utils.Reflect.Method(typeof(MapScene), "GetFaceIndex", new Type[] { typeof(CampaignVec2).MakeByRefType() })),
            new Prefix(nameof(GetHeightAtPoint), new Utils.Reflect.Method(typeof(MapScene), "GetHeightAtPoint", new Type[] { typeof(CampaignVec2).MakeByRefType(), typeof(float).MakeByRefType() })),
        };

        private static bool FindReachablePointAroundPosition(CampaignVec2 __result, CampaignVec2 center, MobileParty.NavigationType navigationCapability, float maxDistance, float minDistance = 0f, bool useUniformDistribution = false)
        {
            __result = center;
            return false;
        }

        private static bool GetFaceIndex(PathFaceRecord __result, in CampaignVec2 vec2)
        {
            __result = new PathFaceRecord(-1, -1, -1);
            return false;
        }

        private static bool GetHeightAtPoint(in CampaignVec2 point, ref float height)
        {
            height = 1f;
            return false;
        }
    }
}
