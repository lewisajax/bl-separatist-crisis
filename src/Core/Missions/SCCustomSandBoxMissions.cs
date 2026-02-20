using Helpers;
using SandBox;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Hideout;
using SeparatistCrisis.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace SeparatistCrisis.Missions
{
    [MissionManager]
    public static class SCCustomSandBoxMissions
    {
        [MissionMethod]
        public static Mission OpenBattleMission(string scene, bool usesTownDecalAtlas)
        {
            return SCMissions.OpenBattleMission(SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, "", false, usesTownDecalAtlas ? DecalAtlasGroup.Town : DecalAtlasGroup.Battle));
        }

        [MissionMethod]
        public static Mission OpenBattleMission(MissionInitializerRecord rec)
        {
            bool isPlayerSergeant = MobileParty.MainParty.MapEvent.IsPlayerSergeant();
            bool isPlayerInArmy = MobileParty.MainParty.Army != null;
            List<string> heroesOnPlayerSideByPriority = HeroHelper.OrderHeroesOnPlayerSideByPriority();
            bool isPlayerAttacker = !(from p in MobileParty.MainParty.MapEvent.AttackerSide.Parties
                                      where p.Party == MobileParty.MainParty.Party
                                      select p).IsEmpty<MapEventParty>();

            return MissionState.OpenNew("SCBattle", rec, delegate (Mission mission)
            {
                Hero leaderHero = MapEvent.PlayerMapEvent.AttackerSide.LeaderParty.LeaderHero;
                TextObject attackerGeneralName = (leaderHero != null) ? leaderHero.Name : null;
                Hero leaderHero2 = MapEvent.PlayerMapEvent.DefenderSide.LeaderParty.LeaderHero;

                return new MissionBehavior[]
                {
                    SCMissions.CreateCampaignMissionAgentSpawnLogic(Mission.BattleSizeType.Battle, null, null),
                    new BattlePowerCalculationLogic(),
                    new BattleSpawnLogic("battle_set"),
                    new SandBoxBattleMissionSpawnHandler(),
                    new CampaignMissionComponent(),
                    new BattleAgentLogic(),
                    new MountAgentLogic(),
                    new BannerBearerLogic(),
                    new MissionOptionsComponent(),
                    new BattleEndLogic(),
                    new BattleReinforcementsSpawnController(),
                    new MissionCombatantsLogic(MobileParty.MainParty.MapEvent.InvolvedParties, PartyBase.MainParty, MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Defender), MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Attacker), Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant),
                    new BattleObserverMissionLogic(),
                    new AgentHumanAILogic(),
                    new AgentVictoryLogic(),
                    new BattleSurgeonLogic(),

                    new BlasterMissileLogic(),
                    new AbilitiesLogic(),

                    new MissionAgentPanicHandler(),
                    new BattleMissionAgentInteractionLogic(),
                    new AgentMoraleInteractionLogic(),
                    new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority),new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null, null, null, true),

                    new AbilityControllerLeaveLogic(),
                    new EquipmentControllerLeaveLogic(),

                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new BattleDeploymentMissionController(isPlayerAttacker),
                    new BattleDeploymentHandler(isPlayerAttacker)
                };
            }, true, true);
        }

        [MissionMethod]
        public static Mission OpenHideoutBattleMission(string scene, FlattenedTroopRoster playerTroops, bool isTutorial)
        {
            List<MobileParty> list = new List<MobileParty>();
            foreach (MapEventParty mapEventParty in MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender))
            {
                if (mapEventParty.Party.IsMobile)
                {
                    list.Add(mapEventParty.Party.MobileParty);
                }
            }
            string sceneLevels = isTutorial ? "level_1" : "level_2";
            int firstPhaseEnemySideTroopCount;
            FlattenedTroopRoster banditPriorityList = MapEventHelper.GetPriorityListForHideoutMission(list, out firstPhaseEnemySideTroopCount);
            FlattenedTroopRoster playerPriorityList = playerTroops ?? MobilePartyHelper.GetStrongestAndPriorTroops(MobileParty.MainParty, Campaign.Current.Models.BanditDensityModel.GetMaximumTroopCountForHideoutMission(MobileParty.MainParty, true), true).ToFlattenedRoster();
            int firstPhasePlayerSideTroopCount = playerPriorityList.Count<FlattenedTroopRosterElement>();
            MissionInitializerRecord rec = SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, sceneLevels, false, DecalAtlasGroup.Town);
            return MissionState.OpenNew("SCCustomSandBoxHideoutBattle", rec, delegate (Mission mission)
            {
                IMissionTroopSupplier[] suppliers = new IMissionTroopSupplier[]
                {
                    new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, banditPriorityList, null),
                    new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Attacker, playerPriorityList, null)
                };
                return new MissionBehavior[]
                {
                    new MissionOptionsComponent(),
                    new CampaignMissionComponent(),
                    new BattleEndLogic(),
                    new MissionCombatantsLogic(MobileParty.MainParty.MapEvent.InvolvedParties, PartyBase.MainParty, MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Defender), MobileParty.MainParty.MapEvent.GetLeaderParty(BattleSideEnum.Attacker), Mission.MissionTeamAITypeEnum.NoTeamAI, false),
                    new AgentHumanAILogic(),
                    new HideoutCinematicController(),
                    new MissionConversationLogic(),
                    new HideoutMissionController(suppliers, PartyBase.MainParty.Side, firstPhaseEnemySideTroopCount, firstPhasePlayerSideTroopCount),
                    new BattleObserverMissionLogic(),
                    new BattleAgentLogic(),
                    new MountAgentLogic(),
                    new AgentVictoryLogic(),
                    new MissionAgentPanicHandler(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(10f),
                    new AgentMoraleInteractionLogic(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new EquipmentControllerLeaveLogic(),
                    new BattleSurgeonLogic()
                };
            }, true, true);
        }

        public static MissionAgentSpawnLogic CreateCampaignMissionAgentSpawnLogic(Mission.BattleSizeType battleSizeType, FlattenedTroopRoster priorTroopsForDefenders = null, FlattenedTroopRoster priorTroopsForAttackers = null)
        {
            return new MissionAgentSpawnLogic(new IMissionTroopSupplier[]
            {
                new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, priorTroopsForDefenders),
                new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Attacker, priorTroopsForAttackers)
            }, PartyBase.MainParty.Side, battleSizeType);
        }
    }
}
