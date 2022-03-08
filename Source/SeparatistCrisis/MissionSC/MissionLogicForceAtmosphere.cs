using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.MissionSC
{
    public class MissionLogicForceAtmosphere : MissionLogic
    {
        private readonly string forceAtmosphereSuffix = "forceatmo";
        
        public override void EarlyStart()
        {
            if (Mission.Scene != null && Mission.SceneName.EndsWith(forceAtmosphereSuffix))
            {
                Mission.Scene.SetAtmosphereWithName(Mission.SceneName);
            }
        }
    }
}