using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Missions
{
    public class AbilityControllerLeaveLogic : MissionLogic
    {
        public bool IsAbilitySelectionActive { get; private set; }

        public void SetIsAbilitySelectionActive(bool isActive)
        {
            this.IsAbilitySelectionActive = isActive;
            Debug.Print("IsAbilitySelectionActive: " + isActive.ToString(), 0, Debug.DebugColor.White, 17592186044416UL);
        }

        public override InquiryData OnEndMissionRequest(out bool canLeave)
        {
            canLeave = !this.IsAbilitySelectionActive;
            return null;
        }
    }
}
