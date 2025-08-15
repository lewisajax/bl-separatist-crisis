using System;
using System.IO;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Behaviors
{
    public class ForceAtmosphereLogic : MissionLogic
    {
        private readonly string forceAtmosphereSuffix = "geonosis";
        private readonly string defaultAtmosphere = "geonosis";
        
        public override void EarlyStart()
        {
            if (Mission.Scene != null && Mission.SceneName.StartsWith(forceAtmosphereSuffix, StringComparison.Ordinal))
            {
                if (File.Exists("../../Modules/SeparatistCrisisGeonosisAssetsandMaps/Atmospheres/" + Mission.SceneName + ".xml"))
                {
                    Mission.Scene.SetAtmosphereWithName(Mission.SceneName);
                    return;
                }
                
                Mission.Scene.SetAtmosphereWithName(defaultAtmosphere);
            }
        }
    }
}