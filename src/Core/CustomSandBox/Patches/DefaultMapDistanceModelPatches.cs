using HarmonyLib;
using Helpers;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Options;

namespace SeparatistCrisis.CustomSandBox
{
    public class DefaultMapDistanceModelPatches : PatchClass<DefaultMapDistanceModelPatches>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            // We skip all the distance calcs as some arguments or stuff that gets used inside each method will be null
            new Prefix(nameof(GetDistance1), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(Settlement), typeof(Settlement), typeof(bool), typeof(bool), typeof(MobileParty.NavigationType) })),
            new Prefix(nameof(GetDistance2), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(Settlement), typeof(Settlement), typeof(bool), typeof(bool), typeof(MobileParty.NavigationType), typeof(float).MakeByRefType() })),
            new Prefix(nameof(GetDistance3), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(MobileParty), typeof(Settlement), typeof(bool), typeof(MobileParty.NavigationType), typeof(float).MakeByRefType() })),
            new Prefix(nameof(GetDistance4), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(MobileParty), typeof(MobileParty), typeof(MobileParty.NavigationType), typeof(float).MakeByRefType() })),
            new Prefix(nameof(GetDistance5), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(MobileParty), typeof(MobileParty), typeof(MobileParty.NavigationType), typeof(float), typeof(float).MakeByRefType(), typeof(float).MakeByRefType() })),
            new Prefix(nameof(GetDistance6), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(MobileParty), typeof(CampaignVec2).MakeByRefType(), typeof(MobileParty.NavigationType), typeof(float).MakeByRefType() })),
            new Prefix(nameof(GetDistance7), new Utils.Reflect.Method(typeof(DefaultMapDistanceModel), "GetDistance", new Type[]{ typeof(Settlement), typeof(CampaignVec2).MakeByRefType(), typeof(bool), typeof(MobileParty.NavigationType) })),
        
            new Prefix(nameof(GetNeighborsOfFortificationDistanceModel), typeof(DefaultMapDistanceModel), "GetNeighborsOfFortification"),
            new Prefix(nameof(GetClosestEntranceToFace), typeof(DefaultMapDistanceModel), "GetClosestEntranceToFace"),
            new Prefix(nameof(GetNeighborScoreForConsideringClan), typeof(SettlementHelper), "GetNeighborScoreForConsideringClan"),
        };

        private static bool GetDistance1(ref float __result, Settlement fromSettlement, Settlement toSettlement, bool isFromPort = false, bool isTargetingPort = false, MobileParty.NavigationType navigationCapability = MobileParty.NavigationType.Default)
        {
            __result = 1.0f;
            return false;
        }

        private static bool GetDistance2(ref float __result, Settlement fromSettlement, Settlement toSettlement, bool isFromPort, bool isTargetingPort, MobileParty.NavigationType navigationCapability, out float landRatio)
        {
            landRatio = 1.0f;
            __result = 1.0f;
            return false;
        }

        private static bool GetDistance3(ref float __result, MobileParty fromMobileParty, Settlement toSettlement, bool isTargetingPort, MobileParty.NavigationType customCapability, out float estimatedLandRatio)
        {
            estimatedLandRatio = 1.0f;
            __result = 1.0f;
            return false;
        }

        private static bool GetDistance4(ref float __result, MobileParty fromMobileParty, MobileParty toMobileParty, MobileParty.NavigationType customCapability, out float landRatio)
        {
            landRatio = 1.0f;
            __result = 1.0f;
            return false;
        }

        private static bool GetDistance5(ref bool __result, MobileParty fromMobileParty, MobileParty toMobileParty, MobileParty.NavigationType customCapability, float maxDistance, out float distance, out float landRatio)
        {
            landRatio = 1.0f;
            distance = 1.0f;
            __result = true;
            return false;
        }

        private static bool GetDistance6(ref float __result, MobileParty fromMobileParty, in CampaignVec2 toPoint, MobileParty.NavigationType customCapability, out float landRatio)
        {
            landRatio = 1.0f;
            __result = 1.0f;
            return false;
        }

        private static bool GetDistance7(ref float __result, Settlement fromSettlement, in CampaignVec2 toPoint, bool isFromPort, MobileParty.NavigationType customCapability)
        {
            __result = 1.0f;
            return false;
        }

        private static bool GetNeighborsOfFortificationDistanceModel(MBReadOnlyList<Settlement> __result, Town town, MobileParty.NavigationType navigationCapabilities)
        {
            __result = new MBReadOnlyList<Settlement>();
            return false;
        }

        private static bool GetClosestEntranceToFace(ValueTuple<Settlement, bool> __result, PathFaceRecord face, MobileParty.NavigationType navigationCapabilities)
        {
            __result = new ValueTuple<Settlement, bool>(null, false);
            return false;
        }

        private static bool GetNeighborScoreForConsideringClan(float __result, Settlement settlement, Clan consideringClan)
        {
            __result = 1f;
            return false;
        }

    }
}
