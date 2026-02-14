using SandBox;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem.Load;

namespace SeparatistCrisis.CustomSandBox
{
    // Instantiate the Campaign obj in this class and pass it GameMode.Campaign
    // We'll still have CustomSandBox as the GameType
    public class CustomSandBoxGameManager: MBGameManager
    {
        // I don't like that these are in here
        public static readonly string CustomSandBoxHarmonyDomain = "bannerlord." + typeof(CustomSandBoxGameManager).Namespace!.ToLower(System.Globalization.CultureInfo.CurrentCulture) + ".customsandbox";
        
        private readonly PatchClass[] _customSandBoxPatchClasses = new PatchClass[]
        {
            new CampaignPatches(),
            new NavigationHelperPatches(),
            new DefaultMapDistanceModelPatches()
        };

        private SandBoxGameManager.CampaignCreatorDelegate _campaignCreator;

        public delegate Campaign CampaignCreatorDelegate();

        public CustomSandBoxGameManager(SandBoxGameManager.CampaignCreatorDelegate campaignCreator)
        {
            this._campaignCreator = campaignCreator;
        }

        protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
        {
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
                    return;

                case GameManagerLoadingSteps.FirstInitializeFirstStep:
                    MBGameManager.LoadModuleData(false);
                    nextStep = GameManagerLoadingSteps.WaitSecondStep;
                    return;

                case GameManagerLoadingSteps.WaitSecondStep:
                    MBGameManager.StartNewGame();
                    PatchManager.ApplyCustomPatches(CustomSandBoxGameManager.CustomSandBoxHarmonyDomain, this._customSandBoxPatchClasses);
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    return;

                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    MBGlobals.InitializeReferences();

                    MBDebug.Print("Initializing custom sandbox begin...");
                    Campaign campaign = this._campaignCreator();
                    Game.CreateGame(campaign, this);
                    campaign.SetLoadingParameters(Campaign.GameLoadingType.NewCampaign);
                    MBDebug.Print("Initializing custom sandbox end...");
                    
                    Game.Current.DoLoading();
                    nextStep = GameManagerLoadingSteps.PostInitializeFourthState;
                    return;

                case GameManagerLoadingSteps.PostInitializeFourthState:
                    {
                        bool flag = true;
                        foreach (MBSubModuleBase mbsubModuleBase in Module.CurrentModule.CollectSubModules())
                        {
                            flag = (flag && mbsubModuleBase.DoLoading(Game.Current));
                        }
                        nextStep = (flag ? GameManagerLoadingSteps.FinishLoadingFifthStep : GameManagerLoadingSteps.PostInitializeFourthState);
                        return;
                    }

                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = (Game.Current.DoLoading() ? GameManagerLoadingSteps.None : GameManagerLoadingSteps.FinishLoadingFifthStep);
                    return;

                default:
                    return;
            }
        }

        public override void OnAfterCampaignStart(Game game)
        {
        }

        public override void OnLoadFinished()
        {
            MBDebug.Print("Switching to menu window...");
            
            MBDebug.Print("OnLoadFinished DevelopmentMode");
            MBDebug.Print("Launching Custom Sandbox");
            CustomSandBoxState gameState = Game.Current.GameStateManager.CreateState<CustomSandBoxState>();
            Game.Current.GameStateManager.CleanAndPushState(gameState, 0);

            base.IsLoaded = true;
        }

        public override void OnGameEnd(Game game)
        {
            MBDebug.SetErrorReportScene(null);
            base.OnGameEnd(game);
            PatchManager.RemoveCustomPatches();
        }
    }
}
