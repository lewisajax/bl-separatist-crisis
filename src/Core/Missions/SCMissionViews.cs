using SandBox.View.Missions;
using SandBox.ViewModelCollection;
using SeparatistCrisis.Views;
using SeparatistCrisis.Views.Placeholders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Sound;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

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

                SCViewCreator.CreateMissionAbilityEquipView(mission),

                ViewCreator.CreateMissionMainAgentEquipDropView(mission),
                missionView,
                new OrderTroopPlacer(null),
                new MissionSingleplayerViewHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                ViewCreator.CreateMissionAgentLockVisualizerView(mission),
                new MusicBattleMissionView(false),
                new DeploymentMissionView(),
                new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f),
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
                new MissionEntitySelectionUIHandler(new Action<WeakGameEntity>(((ISiegeDeploymentView)missionView).OnEntitySelection), new Action<WeakGameEntity>(((ISiegeDeploymentView)missionView).OnEntityHover)),ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, new SPOrderOfBattleVM()),
                
                SCViewCreator.CreateMissionBlasterView(mission),
                // new MissionGauntletBlasterMissileView(), // Custom
            };
        }

        [ViewMethod("SCCustomBattle")]
        public static MissionView[] OpenCustomBattleMission(Mission mission)
        {
            List<MissionView> list = new List<MissionView>();
            list.Add(ViewCreator.CreateMissionSingleplayerEscapeMenu(false));
            list.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
            list.Add(ViewCreator.CreateMissionBattleScoreUIHandler(mission, new CustomBattleScoreboardVM()));
            list.Add(ViewCreator.CreateOptionsUIHandler());
            list.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
            MissionView missionView = ViewCreator.CreateMissionOrderUIHandler(null);
            list.Add(missionView);
            list.Add(new OrderTroopPlacer(null));
            list.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
            list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
            list.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
            list.Add(ViewCreator.CreateMissionAgentLockVisualizerView(mission));
            list.Add(new MusicBattleMissionView(false));
            list.Add(new DeploymentMissionView());
            ISiegeDeploymentView @object = missionView as ISiegeDeploymentView;
            list.Add(new MissionEntitySelectionUIHandler(new Action<WeakGameEntity>(@object.OnEntitySelection), new Action<WeakGameEntity>(@object.OnEntityHover)));
            list.Add(new MissionFormationTargetSelectionHandler());
            list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
            list.Add(new MissionBoundaryWallView());
            list.Add(new MissionDeploymentBoundaryMarker("swallowtail_banner", 2f));
            list.Add(ViewCreator.CreateMissionFormationMarkerUIHandler(mission));
            list.Add(ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler());
            list.Add(ViewCreator.CreateMissionSpectatorControlView(mission));
            list.Add(ViewCreator.CreatePhotoModeView());
            list.Add(new MissionItemContourControllerView());
            list.Add(new MissionAgentContourControllerView());
            list.Add(new MissionCustomBattlePreloadView());
            list.Add(ViewCreator.CreateMissionOrderOfBattleUIHandler(mission, new OrderOfBattleVM()));
            list.Add(ViewCreator.CreateMissionObjectiveView(mission));

            list.Add(SCViewCreator.CreateMissionBlasterView(mission));
            list.Add(SCViewCreator.CreateMissionAbilityEquipView(mission));
            return list.ToArray();
        }
    }
}
