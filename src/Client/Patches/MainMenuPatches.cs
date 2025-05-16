using System.Collections.Generic;
using System.Linq;
using SeparatistCrisis.Utils;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Patches
{
    /*public class MainMenuPatches
    {

        public MainMenuPatches()
        {
            SetMenuOptions();
        }

        private void SetMenuOptions()
        {
            List<InitialStateOption> stateOptions = Module.CurrentModule.GetInitialStateOptions().ToList();
            Module.CurrentModule.ClearStateOptions();

            foreach (InitialStateOption stateOption in stateOptions)
            {
                if (stateOption.Id.Equals("StoryModeNewGame", System.StringComparison.Ordinal))
                {
                    Module.CurrentModule.AddInitialStateOption(new InitialStateOption(stateOption.Id, stateOption.Name,
                        stateOption.OrderIndex, null,
                        () => (true,
                            new TextObject(
                                "Separatist Crisis does not support Story Mode at the moment. You can start Sandbox instead."))));
                    continue;
                }

                Module.CurrentModule.AddInitialStateOption(stateOption);
            }

            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ScShaderCache",
                new TextObject("Build Shader Cache"), 4, OnForceClick, () => (false, new TextObject())));
        }

        private void OnForceClick()
        {
            DisplayWindow();
        }

        private void DisplayWindow()
        {
            var data = new InquiryData(
                "Important warning",
                "This will load a scene with all the unique troops and NPCs present in our mod. The purpose of this is to compile the local shader cache on your PC.\n \n" +
                "THIS WILL TAKE A LONG TIME!!!\n" +
                "Our users report anything between 20 and 60 minutes.\n \n" +
                "You only need to do this once after installing or updating this mod \n \n" +
                "This ensures that you won't need to compile the shaders individually during normal gameplay, as it can cause issues with stability.\n" +
                "This is meant to reduce the number of UI portrait generation crashes and also eliminate the long battle loading times during normal gameplay.\n \n" +
                "We thank the TOW mod team for providing us with this feature. Please check them out on ModDB!",
                true,
                true,
                "Do it",
                "Not now",
                BuildShaderCache,
                HideWindow
            );
            InformationManager.ShowInquiry(data);
        }

        private void BuildShaderCache()
        {
            MBGameManager.StartNewGame(new ShaderGameManager());
        }

        private void HideWindow()
        {
            InformationManager.HideInquiry();
        }
    }*/
}