using Helpers;
using SandBox;
using SandBox.Missions.MissionLogics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.TroopSuppliers;
using SeparatistCrisis.Behaviors;

namespace SeparatistCrisis.Missions
{
    [MissionManager]
    public static class SCMissions
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
                    new BlasterMissileLogic(), // Custom
                    new MissionAgentPanicHandler(),
                    new BattleMissionAgentInteractionLogic(),
                    new AgentMoraleInteractionLogic(),
                    new AssignPlayerRoleInTeamMissionController(!isPlayerSergeant, isPlayerSergeant, isPlayerInArmy, heroesOnPlayerSideByPriority, FormationClass.NumberOfRegularFormations),
                    new SandboxGeneralsAndCaptainsAssignmentLogic(attackerGeneralName, (leaderHero2 != null) ? leaderHero2.Name : null, null, null, true),
                    new EquipmentControllerLeaveLogic(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new HighlightsController(),
                    new BattleHighlightsController(),
                    new DeploymentMissionController(isPlayerAttacker),
                    new BattleDeploymentHandler(isPlayerAttacker)
                };
            }, true, true);
        }

        public static MissionAgentSpawnLogic CreateCampaignMissionAgentSpawnLogic(Mission.BattleSizeType battleSizeType, FlattenedTroopRoster priorTroopsForDefenders = null, FlattenedTroopRoster priorTroopsForAttackers = null)
        {
            return new MissionAgentSpawnLogic(new IMissionTroopSupplier[]
            {
                new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Defender, priorTroopsForDefenders, null),
                new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, BattleSideEnum.Attacker, priorTroopsForAttackers, null)
            }, PartyBase.MainParty.Side, battleSizeType);
        }
    }
}
