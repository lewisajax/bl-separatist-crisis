using SandBox.View;
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
    public static class SCCustomSandBoxMissionViews
    {
        [ViewMethod("SCCustomSandBoxHideoutBattle")]
        public static MissionView[] OpenHideoutBattleMission(Mission mission)
        {
            return new List<MissionView>
            {
                new MissionCampaignView(),
                new MissionConversationCameraView(),
                new MissionHideoutCinematicView(),
                SandBoxViewCreator.CreateMissionConversationView(mission),
                ViewCreator.CreateMissionSingleplayerEscapeMenu(CampaignOptions.IsIronmanMode),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionMainAgentEquipDropView(mission),
                ViewCreator.CreateMissionBattleScoreUIHandler(mission, new SPScoreboardVM(null)),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMissionOrderUIHandler(null),
                new OrderTroopPlacer(null),
                new MissionSingleplayerViewHandler(),
                new MusicSilencedMissionView(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
                ViewCreator.CreateMissionAgentLockVisualizerView(mission),
                ViewCreator.CreateMissionBoundaryCrossingView(),
                new MissionBoundaryWallView(),
                ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
                new MissionFormationTargetSelectionHandler(),
                ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionSpectatorControlView(mission),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                new MissionCampaignBattleSpectatorView(),
                new MissionPreloadView(),
                ViewCreator.CreatePhotoModeView()
            }.ToArray();
        }
    }
}
