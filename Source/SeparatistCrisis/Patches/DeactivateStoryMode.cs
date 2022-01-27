using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Patches
{
    public class DeactivateStoryMode
    {

        public DeactivateStoryMode()
        {
            List<InitialStateOption> stateOptions = Module.CurrentModule.GetInitialStateOptions().ToList();
            Module.CurrentModule.ClearStateOptions();
            
            foreach (InitialStateOption stateOption in stateOptions)
            {
                if (stateOption.Id.Equals("StoryModeNewGame"))
                {
                    Module.CurrentModule.AddInitialStateOption(new InitialStateOption(stateOption.Id, stateOption.Name, 
                        stateOption.OrderIndex, null, () => (true, new TextObject("Separatist Crisis does not support Story Mode at the moment. You can start Sandbox instead."))));
                    continue;
                }
                
                Module.CurrentModule.AddInitialStateOption(stateOption);    
            }
        }
    }
}