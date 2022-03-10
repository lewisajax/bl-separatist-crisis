using System;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.MissionSC
{
    public class MissionLogicForceAtmosphere : MissionLogic
    {
        private readonly string forceAtmosphereSuffix = "geonosis";
        
        public override void EarlyStart()
        {
            if (Mission.Scene != null && Mission.SceneName.StartsWith(forceAtmosphereSuffix, StringComparison.Ordinal))
            {
                Mission.Scene.SetAtmosphereWithName(Mission.SceneName);
            }
        }
    }
}