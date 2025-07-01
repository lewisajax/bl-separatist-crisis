using SandBox.View.Missions;
using SandBox.ViewModelCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using SeparatistCrisis.Views;

namespace SeparatistCrisis.Missions
{
    [ViewCreatorModule]
    public static class SCMissionViews
    {
        [ViewMethod("SCBattle")]
        public static MissionView[] OpenBattleMission(Mission mission)
        {
            MissionView missionView = ViewCreator.CreateMissionOrderUIHandler(null);

            return new MissionView[]
            {
                new MissionCampaignView(),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new SPScoreboardVM(null)),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionMainAgentEquipDropView(mission),
                missionView,
                new OrderTroopPlacer(),
                new MissionSingleplayerViewHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                ViewCreator.CreateMissionAgentLockVisualizerView(mission),
                new MusicBattleMissionView(false),
                new DeploymentMissionView(),
                new MissionDeploymentBoundaryMarker(new BorderFlagEntityFactory("swallowtail_banner"), 2f),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionFormationTargetSelectionHandler(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionPreloadView(),
                new MissionCampaignBattleSpectatorView(),
                ViewCreator.CreatePhotoModeView(),
                new MissionEntitySelectionUIHandler(new Action<GameEntity>(((ISiegeDeploymentView)missionView).OnEntitySelection), new Action<GameEntity>(((ISiegeDeploymentView)missionView).OnEntityHover)),
                ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, new SPOrderOfBattleVM()),
                new BlasterMissileView(), // Custom
            };
        }
    }
}
