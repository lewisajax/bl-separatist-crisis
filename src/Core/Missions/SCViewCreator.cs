using SeparatistCrisis.Views.Placeholders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace SeparatistCrisis.Missions
{
    public static class SCViewCreator
    {
        public static MissionView CreateMissionAbilityEquipView(Mission mission = null)
        {
            return ViewCreatorManager.CreateMissionView<MissionMainAgentAbilityEquipView>(mission != null, mission, Array.Empty<object>());
        }

        public static MissionView CreateMissionBlasterView(Mission mission = null)
        {
            return ViewCreatorManager.CreateMissionView<MissionBlasterMissileView>(mission != null, mission, Array.Empty<object>());
        }
    }
}
