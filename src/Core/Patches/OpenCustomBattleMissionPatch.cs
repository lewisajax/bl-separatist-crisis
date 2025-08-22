using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using SeparatistCrisis.PatchTools;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle;
using SeparatistCrisis.MissionManagers;
using HarmonyLib;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace SeparatistCrisis.Patches
{
    //public class OpenCustomBattleMissionPatch : PatchClass<OpenCustomBattleMissionPatch, CustomBattleMenuVM>
    //{
    //    protected override IEnumerable<Patch> Prepare() => new Patch[]
    //    {
    //        new Prefix(nameof(OnNewGameCreatedPrefix), "ExecuteStart")
    //    };

    //    private static bool OnNewGameCreatedPrefix(CustomBattleMenuVM __instance)
    //    {
    //        Type type = __instance.GetType();
    //        SCCustomBattles.StartGame((CustomBattleData)AccessTools.DeclaredMethod(__instance.GetType(), "PrepareBattleData").Invoke(__instance, Array.Empty<object>()));
    //        Debug.Print("EXECUTE START - PRESSED", 0, Debug.DebugColor.Green, 17592186044416UL);
    //        return false;
    //    }
    //}
}
