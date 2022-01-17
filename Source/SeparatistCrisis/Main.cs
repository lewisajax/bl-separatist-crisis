using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis
{
    public class Main : MBSubModuleBase
    {

        protected override void OnSubModuleLoad()
        {
            // Apply harmony patches
            new Harmony("com.separatistcrisis.patches").PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            
            if (!(game.GameType is Campaign))
            {
                return;
            }
            
            ((CampaignGameStarter)gameStarterObject).LoadGameTexts(ModuleHelper.GetModuleFullPath("SeparatistCrisis") + "ModuleData/module_strings.xml");
        }
        
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            InformationManager.DisplayMessage(new InformationMessage("SeparatistCrisis: Crosshair patched", new Color(42f, 0f, 209f)));
        }

        // This allows us to add custom maps to the custom battle mode
        // Loading custom_battle_scenes.xml in our module
        /*
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is CustomGame))
            {
                return;
            }
            CustomGame customGame = (CustomGame)game.GameType;
            string filePath = Path.Combine("..", "..", "Modules", "SeparatistCrisis", "ModuleData", "custom_battle_scenes.xml");
            
            if (File.Exists(filePath))
            {
                customGame.LoadCustomBattleScenes(filePath);
            }
        }

        
        ##### This code changes the ruling clan of the southern empire on campaign start ######
        public override void OnGameInitializationFinished(Game game)
        {
            if (!(game.GameType is Campaign))
            {
                return;
            }
            Kingdom kingdom = MBObjectManager.Instance.GetObject<Kingdom>("empire_s");
            IReadOnlyList<Clan> clans = kingdom.Clans;
            kingdom.RulingClan = clans[1];
            Debug.Print(kingdom.RulingClan.Name.ToString());
        }
        */
    }
}
