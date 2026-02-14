using HarmonyLib;
using Helpers;
using SandBox;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Options;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.CustomSandBox
{
    public class CampaignPatches : PatchClass<CampaignPatches>
    {
        // All this shit needs to go once we find the right chokepoints.
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            // new Transpiler(nameof(DoLoadingForGameType), new Utils.Reflect.Method(typeof(Campaign), "DoLoadingForGameType", new Type[] { typeof(GameTypeLoadingStates), typeof(GameTypeLoadingStates).MakeByRefType() })),
            new Prefix(nameof(LoadMapScene), new Utils.Reflect.Method(typeof(Campaign), "LoadMapScene")),
            new Prefix(nameof(DoLoadingForGameType), new Utils.Reflect.Method(typeof(Campaign), "DoLoadingForGameType", new Type[] { typeof(GameTypeLoadingStates), typeof(GameTypeLoadingStates).MakeByRefType() })),

            new Prefix(nameof(UpdateCurrentStrength), new Utils.Reflect.Method(typeof(Clan), "UpdateCurrentStrength")),
            new Prefix(nameof(OnSessionStart), new Utils.Reflect.Method(typeof(Town), "OnSessionStart")),
            new Prefix(nameof(OnSessionLaunchedEvent), new Utils.Reflect.Method(typeof(MapWeatherCampaignBehavior), "OnSessionLaunchedEvent", new Type[] { typeof(CampaignGameStarter) })),
            new Prefix(nameof(PartyHourlyAiTick), new Utils.Reflect.Method(typeof(AiPartyThinkBehavior), "PartyHourlyAiTick", new Type[] { typeof(MobileParty) })),
            new Prefix(nameof(OnNewGameCreatedPartialFollowUpEnd), new Utils.Reflect.Method(typeof(IssuesCampaignBehavior), "OnNewGameCreatedPartialFollowUpEnd", new Type[] { typeof(CampaignGameStarter) })),
            new Prefix(nameof(GetBestSettlementToSpawnAround), new Utils.Reflect.Method(typeof(SettlementHelper), "GetBestSettlementToSpawnAround", new Type[] { typeof(Hero) })),
            new Prefix(nameof(GetSpawnPositionAroundSettlement), new Utils.Reflect.Method(typeof(BanditSpawnCampaignBehavior), "GetSpawnPositionAroundSettlement", new Type[] { typeof(Clan), typeof(Settlement) })),
        };

        private static bool GetBestSettlementToSpawnAround(Settlement __result, Hero hero)
        {
            __result = MBObjectManager.Instance.GetFirstObject<Settlement>();
            return false;
        }

        private static bool GetSpawnPositionAroundSettlement(CampaignVec2 __result, Clan clan, Settlement settlement)
        {
            __result = CampaignVec2.Zero;
            return false;
        }

        private static bool OnSessionStart(Town __instance)
        {
            __instance.BesiegerCampPositions1 = Array.Empty<MatrixFrame>();
            __instance.BesiegerCampPositions2 = Array.Empty<MatrixFrame>();
            return false;
        }

        private static bool UpdateCurrentStrength(Clan __instance)
        {
            MethodInfo strengthSetter = AccessTools.PropertySetter(typeof(Clan), "CurrentTotalStrength");
            strengthSetter.Invoke(__instance, new object[] { 1f });
            return false;
        }


        private static bool OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter starter)
        {
            return false;
        }

        private static bool OnSessionLaunchedEvent(CampaignGameStarter obj)
        {
            return false;
        }

        // Look at deregistering the hourly listener instead
        private static bool PartyHourlyAiTick(MobileParty mobileParty)
        {
            return false;
        }

        private static bool LoadMapScene(Campaign __instance)
        {
            IMapScene sceneWrapper = __instance.MapSceneCreator.CreateMapScene();
            AccessTools.Field(typeof(Campaign), "_mapSceneWrapper").SetValue(__instance, sceneWrapper);
            sceneWrapper.SetSceneLevels(new List<string>
            {
                "level_1",
                "level_2",
                "level_3",
                "siege",
                "raid",
                "burned"
            });
            
            // sceneWrapper.Load();
            
            Vec2 mapMinimumPosition;
            Vec2 mapMaximumPosition;
            float mapMaximumHeight;
            sceneWrapper.GetMapBorders(out mapMinimumPosition, out mapMaximumPosition, out mapMaximumHeight);

            MethodInfo minimumPositionSetter = AccessTools.PropertySetter(typeof(Campaign), "MapMinimumPosition");
            MethodInfo maximumPositionSetter = AccessTools.PropertySetter(typeof(Campaign), "MapMaximumPosition");
            MethodInfo maximumHeightSetter = AccessTools.PropertySetter(typeof(Campaign), "MapMaximumHeight");
            MethodInfo diagonalSetter = AccessTools.PropertySetter(typeof(Campaign), "MapDiagonal");
            MethodInfo diagonalSquaredSetter = AccessTools.PropertySetter(typeof(Campaign), "MapDiagonalSquared");

            minimumPositionSetter.Invoke(__instance, BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { mapMinimumPosition }, CultureInfo.InvariantCulture);
            maximumPositionSetter.Invoke(__instance, BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { mapMaximumPosition }, CultureInfo.InvariantCulture);
            maximumHeightSetter.Invoke(__instance, BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { mapMaximumHeight }, CultureInfo.InvariantCulture);
            diagonalSetter.Invoke(__instance, BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { Campaign.MapMinimumPosition.Distance(Campaign.MapMaximumPosition) }, CultureInfo.InvariantCulture);
            diagonalSquaredSetter.Invoke(__instance, BindingFlags.NonPublic | BindingFlags.Static, null, new object[] { Campaign.MapDiagonal * Campaign.MapDiagonal }, CultureInfo.InvariantCulture);

            Campaign.PlayerRegionSwitchCostFromLandToSea = (int)(Campaign.MapDiagonal * (float)__instance.Models.MapDistanceModel.RegionSwitchCostFromLandToSea * 0.2f);
            Campaign.PathFindingMaxCostLimit = Math.Max(Campaign.PlayerRegionSwitchCostFromLandToSea * 100, (int)(Campaign.MapDiagonal * 500f));
            sceneWrapper.AfterLoad();

            return false;
        }

        /*private static IEnumerable<CodeInstruction> DoLoadingForGameType(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
            matcher.MatchStartForward(
                    CodeMatch.Calls(typeof(Campaign).GetMethod("CheckMapUpdate"))
                ).ThrowIfInvalid("Could not find the CheckMapUpdate method on Campaign")
                .RemoveInstruction();

            matcher.MatchStartForward(
                    CodeMatch.Calls(typeof(IMapScene).GetMethod("GetSceneXmlCrc"))
                ).ThrowIfInvalid("Could not find the map scene method")
                .Advance(-3)
                .RemoveInstructions(5)
                .Insert(new CodeInstruction[] {
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                });
                

            matcher.MatchStartForward(
                    CodeMatch.Calls(typeof(IMapScene).GetMethod("GetSceneNavigationMeshCrc"))
                ).ThrowIfInvalid("Could not find the map nav method")
                .Advance(-3)
                .RemoveInstructions(5)
                .Insert(new CodeInstruction[] {
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                    new CodeInstruction(OpCodes.Nop),
                });


            return matcher.Instructions();
        }*/

        // The transpiler was fucking up the memory pointer for the Game's instance inside Game.Initialize.
        // Not sure what I'm missing there but we'll go with a prefix instead.
        private static bool DoLoadingForGameType(Campaign __instance, GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
        {
            nextState = GameTypeLoadingStates.None;
            switch (gameTypeLoadingState)
            {
                case GameTypeLoadingStates.InitializeFirstStep:
                    __instance.CurrentGame.Initialize();
                    nextState = GameTypeLoadingStates.WaitSecondStep;
                    return false;
                case GameTypeLoadingStates.WaitSecondStep:
                    nextState = GameTypeLoadingStates.LoadVisualsThirdState;
                    return false;
                case GameTypeLoadingStates.LoadVisualsThirdState:
                    if (__instance.GameMode == CampaignGameMode.Campaign)
                    {
                        MethodInfo loadMapScene = AccessTools.Method(typeof(Campaign), "LoadMapScene");
                        loadMapScene.Invoke(__instance, null);
                    }
                    nextState = GameTypeLoadingStates.PostInitializeFourthState;
                    return false;
                case GameTypeLoadingStates.PostInitializeFourthState:
                    {
                        CampaignGameStarter gameStarter = __instance.SandBoxManager.GameStarter;

                        Campaign.GameLoadingType loadType = (Campaign.GameLoadingType)AccessTools.Field(typeof(Campaign), "_gameLoadingType").GetValue(__instance);
                        // MethodInfo mapSceneCrc = AccessTools.PropertySetter(typeof(Campaign), "_campaignMapSceneXmlCrc");
                        // MethodInfo mapNavCrc = AccessTools.PropertySetter(typeof(Campaign), "_campaignMapSceneNavigationMeshCrc");
                        MethodInfo onDataLoadFinished = AccessTools.Method(typeof(Campaign), "OnDataLoadFinished");
                        MethodInfo calculateCachedValues = AccessTools.Method(typeof(Campaign), "CalculateCachedValues");
                        MethodInfo onNewGameCreated = AccessTools.Method(typeof(Campaign), "OnNewGameCreated", new Type[] { typeof(CampaignGameStarter) });
                        MethodInfo onSessionStart = AccessTools.Method(typeof(Campaign), "OnSessionStart", new Type[] { typeof(CampaignGameStarter) });

                        if (loadType == Campaign.GameLoadingType.NewCampaign)
                        {
                            // mapSceneCrc.Invoke(__instance, new object[] { __instance.MapSceneWrapper.GetSceneXmlCrc() });
                            // mapNavCrc.Invoke(__instance, new object[] { __instance.MapSceneWrapper.GetSceneNavigationMeshCrc() });
                            onDataLoadFinished.Invoke(__instance, new object[] { gameStarter });
                            calculateCachedValues.Invoke(__instance, null);
                            MBSaveLoad.OnNewGame();
                            __instance.InitializeMainParty();
                            foreach (Settlement settlement in Settlement.All)
                            {
                                settlement.OnGameCreated();
                            }
                            MBObjectManager.Instance.RemoveTemporaryTypes();
                            onNewGameCreated.Invoke(__instance, new object[] { gameStarter });
                            onSessionStart.Invoke(__instance, new object[] { gameStarter });
                            Debug.Print("Finished starting a new game.", 0, Debug.DebugColor.White, 17592186044416UL);
                        }

                        __instance.GameManager.OnAfterGameInitializationFinished(__instance.CurrentGame, gameStarter);
                        return false;
                    }
                default:
                    return false;
            }
        }
    }
}
