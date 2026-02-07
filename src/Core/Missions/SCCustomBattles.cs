using SeparatistCrisis.Behaviors;
using SeparatistCrisis.SetOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace SeparatistCrisis.Missions
{
    [MissionManager]
    public static class SCCustomBattles
    {
        public static void StartGame(CustomBattleData data)
        {
            Game.Current.PlayerTroop = data.PlayerCharacter;
            if (data.GameTypeStringId == "Siege")
            {
                OpenSiegeMissionWithDeployment(data.SceneId, data.PlayerCharacter, data.PlayerParty, data.EnemyParty, data.IsPlayerGeneral, data.WallHitpointPercentages, data.HasAnySiegeTower, data.AttackerMachines, data.DefenderMachines, data.IsPlayerAttacker, data.SceneUpgradeLevel, data.SeasonId, data.IsSallyOut, data.IsReliefAttack, data.TimeOfDay);
                return;
            }
            OpenCustomBattleMission(data.SceneId, data.PlayerCharacter, data.PlayerParty, data.EnemyParty, data.IsPlayerGeneral, data.PlayerSideGeneralCharacter, data.SceneLevel, data.SeasonId, data.TimeOfDay);
        }

        private static Type GetSiegeWeaponType(SiegeEngineType siegeWeaponType)
        {
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ladder)
            {
                return typeof(SiegeLadder);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ballista)
            {
                return typeof(Ballista);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireBallista)
            {
                return typeof(FireBallista);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.Ram || siegeWeaponType == DefaultSiegeEngineTypes.ImprovedRam)
            {
                return typeof(BatteringRam);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.SiegeTower || siegeWeaponType == DefaultSiegeEngineTypes.HeavySiegeTower)
            {
                return typeof(SiegeTower);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.Onager || siegeWeaponType == DefaultSiegeEngineTypes.Catapult)
            {
                return typeof(Mangonel);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.FireOnager || siegeWeaponType == DefaultSiegeEngineTypes.FireCatapult)
            {
                return typeof(FireMangonel);
            }
            if (siegeWeaponType == DefaultSiegeEngineTypes.Trebuchet || siegeWeaponType == DefaultSiegeEngineTypes.Bricole)
            {
                return typeof(Trebuchet);
            }
            return null;
        }

        private static Dictionary<Type, int> GetSiegeWeaponTypes(Dictionary<SiegeEngineType, int> values)
        {
            Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
            foreach (KeyValuePair<SiegeEngineType, int> keyValuePair in values)
            {
                dictionary.Add(GetSiegeWeaponType(keyValuePair.Key), keyValuePair.Value);
            }
            return dictionary;
        }

        private static AtmosphereInfo CreateAtmosphereInfoForMission(string seasonId, int timeOfDay)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            dictionary.Add("spring", 0);
            dictionary.Add("summer", 1);
            dictionary.Add("fall", 2);
            dictionary.Add("winter", 3);
            int season = 0;
            dictionary.TryGetValue(seasonId, out season);
            Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
            dictionary2.Add(6, "TOD_06_00_SemiCloudy");
            dictionary2.Add(12, "TOD_12_00_SemiCloudy");
            dictionary2.Add(15, "TOD_04_00_SemiCloudy");
            dictionary2.Add(18, "TOD_03_00_SemiCloudy");
            dictionary2.Add(22, "TOD_01_00_SemiCloudy");
            string atmosphereName = "field_battle";
            dictionary2.TryGetValue(timeOfDay, out atmosphereName);
            return new AtmosphereInfo
            {
                AtmosphereName = atmosphereName,
                TimeInfo = new TimeInformation
                {
                    Season = season
                }
            };
        }

        [MissionMethod]
        public static Mission OpenCustomBattleMission(string scene, BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, bool isPlayerGeneral, BasicCharacterObject playerSideGeneralCharacter, string sceneLevels = "", string seasonString = "", float timeOfDay = 6f)
        {
            BattleSideEnum playerSide = playerParty.Side;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            CustomBattleTroopSupplier customBattleTroopSupplier = new CustomBattleTroopSupplier(playerParty, true, isPlayerGeneral, false, null);
            troopSuppliers[(int)playerParty.Side] = customBattleTroopSupplier;
            CustomBattleTroopSupplier customBattleTroopSupplier2 = new CustomBattleTroopSupplier(enemyParty, false, false, false, null);
            troopSuppliers[(int)enemyParty.Side] = customBattleTroopSupplier2;
            bool isPlayerSergeant = !isPlayerGeneral;
            return MissionState.OpenNew("SCCustomBattle", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = CreateAtmosphereInfoForMission(seasonString, (int)timeOfDay),
                SceneLevels = sceneLevels,
                DecalAtlasGroup = 2
            }, (missionController) => new MissionBehavior[]
            {
                new MissionAgentSpawnLogic(troopSuppliers, playerSide, Mission.BattleSizeType.Battle),
                new EquipmentSetOverrideLogic(isPlayerAttacker ? playerParty : enemyParty, !isPlayerAttacker ? playerParty : enemyParty),
                new BattlePowerCalculationLogic(),
                new CustomBattleAgentLogic(),
                new BannerBearerLogic(),
                new CustomBattleMissionSpawnHandler(!isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty),
                new MissionOptionsComponent(),
                new BattleEndLogic(),
                new BattleReinforcementsSpawnController(),
                new MissionCombatantsLogic(null, playerParty, !isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant),
                new BattleObserverMissionLogic(),
                new AgentHumanAILogic(),
                new AgentVictoryLogic(),
                new MissionAgentPanicHandler(),

                new BlasterMissileLogic(),
                new AbilitiesLogic(),

                new BattleMissionAgentInteractionLogic(),
                new AgentMoraleInteractionLogic(),
                new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, isPlayerSergeant ? Enumerable.Repeat<string>(playerCharacter.StringId, 1).ToList<string>() : new List<string>()),new GeneralsAndCaptainsAssignmentLogic(isPlayerAttacker & isPlayerGeneral ? playerCharacter.GetName() : isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter.GetName() : null, !isPlayerAttacker & isPlayerGeneral ? playerCharacter.GetName() : !isPlayerAttacker & isPlayerSergeant ? playerSideGeneralCharacter.GetName() : null, null, null, true),
                new EquipmentControllerLeaveLogic(),

                new AbilityControllerLeaveLogic(),

                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new HighlightsController(),
                new BattleHighlightsController(),
                new BattleDeploymentMissionController(isPlayerAttacker),
                new BattleDeploymentHandler(isPlayerAttacker)
            }, true, true);
        }

        [MissionMethod]
        public static Mission OpenSiegeMissionWithDeployment(string scene, BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, bool isPlayerGeneral, float[] wallHitPointPercentages, bool hasAnySiegeTower, List<MissionSiegeWeapon> siegeWeaponsOfAttackers, List<MissionSiegeWeapon> siegeWeaponsOfDefenders, bool isPlayerAttacker, int sceneUpgradeLevel = 0, string seasonString = "", bool isSallyOut = false, bool isReliefForceAttack = false, float timeOfDay = 6f)
        {
            string text = sceneUpgradeLevel == 1 ? "level_1" : sceneUpgradeLevel == 2 ? "level_2" : "level_3";
            text += " siege";
            BattleSideEnum playerSide = playerParty.Side;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            CustomBattleTroopSupplier customBattleTroopSupplier = new CustomBattleTroopSupplier(playerParty, true, isPlayerGeneral, isSallyOut, null);
            troopSuppliers[(int)playerParty.Side] = customBattleTroopSupplier;
            CustomBattleTroopSupplier customBattleTroopSupplier2 = new CustomBattleTroopSupplier(enemyParty, false, false, isSallyOut, null);
            troopSuppliers[(int)enemyParty.Side] = customBattleTroopSupplier2;
            bool isPlayerSergeant = !isPlayerGeneral;
            return MissionState.OpenNew("CustomSiegeBattle", new MissionInitializerRecord(scene)
            {
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = CreateAtmosphereInfoForMission(seasonString, (int)timeOfDay),
                SceneLevels = text,
            }, delegate (Mission mission)
            {
                List<MissionBehavior> list = new List<MissionBehavior>();
                list.Add(new BattleSpawnLogic(isSallyOut ? "sally_out_set" : isReliefForceAttack ? "relief_force_attack_set" : "battle_set"));
                list.Add(new MissionOptionsComponent());
                list.Add(new BattleEndLogic());
                list.Add(new BattleReinforcementsSpawnController());
                list.Add(new MissionCombatantsLogic(null, playerParty, !isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, !isSallyOut ? Mission.MissionTeamAITypeEnum.Siege : Mission.MissionTeamAITypeEnum.SallyOut, isPlayerSergeant));
                list.Add(new SiegeMissionPreparationHandler(isSallyOut, isReliefForceAttack, wallHitPointPercentages, hasAnySiegeTower));
                Mission.BattleSizeType battleSizeType = isSallyOut ? Mission.BattleSizeType.SallyOut : Mission.BattleSizeType.Siege;
                list.Add(new EquipmentSetOverrideLogic(playerSide == BattleSideEnum.Attacker ? playerParty : enemyParty, playerSide == BattleSideEnum.Defender ? playerParty : enemyParty));
                list.Add(new MissionAgentSpawnLogic(troopSuppliers, playerSide, battleSizeType));
                list.Add(new BattlePowerCalculationLogic());
                if (isSallyOut)
                {
                    list.Add(new CustomSallyOutMissionController(!isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty));
                }
                else if (isReliefForceAttack)
                {
                    list.Add(new CustomSallyOutMissionController(!isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty));
                }
                else
                {
                    list.Add(new CustomSiegeMissionSpawnHandler(!isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, false));
                }
                list.Add(new BattleObserverMissionLogic());
                list.Add(new CustomBattleAgentLogic());
                list.Add(new BannerBearerLogic());
                list.Add(new AgentHumanAILogic());
                if (!isSallyOut)
                {
                    list.Add(new AmmoSupplyLogic(new List<BattleSideEnum>
                {
                    BattleSideEnum.Defender
                }));
                }
                list.Add(new AgentVictoryLogic());
                list.Add(new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, false, null));
                list.Add(new GeneralsAndCaptainsAssignmentLogic(isPlayerAttacker & isPlayerGeneral ? playerCharacter.GetName() : null, null, null, null, false));
                list.Add(new MissionAgentPanicHandler());
                list.Add(new MissionBoundaryPlacer());
                list.Add(new MissionBoundaryCrossingHandler());
                list.Add(new AgentMoraleInteractionLogic());
                list.Add(new HighlightsController());
                list.Add(new BattleHighlightsController());
                list.Add(new EquipmentControllerLeaveLogic());

                list.Add(new AbilityControllerLeaveLogic());

                if (isSallyOut)
                {
                    list.Add(new MissionSiegeEnginesLogic(new List<MissionSiegeWeapon>(), siegeWeaponsOfAttackers));
                }
                else
                {
                    list.Add(new MissionSiegeEnginesLogic(siegeWeaponsOfDefenders, siegeWeaponsOfAttackers));
                }
                list.Add(new SiegeDeploymentHandler(isPlayerAttacker));
                list.Add(new SiegeDeploymentMissionController(isPlayerAttacker));
                return list.ToArray();
            }, true, true);
        }

        [MissionMethod]
        public static Mission OpenCustomBattleLordsHallMission(string scene, BasicCharacterObject playerCharacter, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, BasicCharacterObject playerSideGeneralCharacter, string sceneLevels = "", int sceneUpgradeLevel = 0, string seasonString = "", float timeOfDay = 6f)
        {
            int remainingDefenderArcherCount = TaleWorlds.Library.MathF.Round(18.9f);
            BattleSideEnum playerSide = BattleSideEnum.Attacker;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            CustomBattleTroopSupplier customBattleTroopSupplier = new CustomBattleTroopSupplier(playerParty, true, playerCharacter == playerSideGeneralCharacter, false, null);
            troopSuppliers[(int)playerParty.Side] = customBattleTroopSupplier;
            CustomBattleTroopSupplier customBattleTroopSupplier2 = new CustomBattleTroopSupplier(enemyParty, false, false, false, delegate (BasicCharacterObject basicCharacterObject)
            {
                bool result = true;
                if (basicCharacterObject.IsRanged)
                {
                    if (remainingDefenderArcherCount > 0)
                    {
                        remainingDefenderArcherCount--;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            });
            troopSuppliers[(int)enemyParty.Side] = customBattleTroopSupplier2;
            return MissionState.OpenNew("CustomBattleLordsHall", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                SceneLevels = "siege",
            }, (missionController) => new MissionBehavior[]
            {
            new MissionOptionsComponent(),
            new BattleEndLogic(),
            new MissionCombatantsLogic(null, playerParty, !isPlayerAttacker ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, Mission.MissionTeamAITypeEnum.NoTeamAI, false),
            new BattleMissionStarterLogic(),
            new AgentHumanAILogic(),
            new LordsHallFightMissionController(troopSuppliers, 3f, 0.7f, 19, 27, playerSide),
            new BattleObserverMissionLogic(),
            new CustomBattleAgentLogic(),
            new AgentVictoryLogic(),
            new AmmoSupplyLogic(new List<BattleSideEnum>
            {
                BattleSideEnum.Defender
            }),
            new EquipmentControllerLeaveLogic(),
            new MissionHardBorderPlacer(),
            new MissionBoundaryPlacer(),
            new MissionBoundaryCrossingHandler(),
            new BattleMissionAgentInteractionLogic(),
            new HighlightsController(),
            new BattleHighlightsController()
            }, true, true);
        }

        private enum CustomBattleGameTypes
        {
            AttackerGeneral,
            DefenderGeneral,
            AttackerSergeant,
            DefenderSergeant
        }
    }
}
