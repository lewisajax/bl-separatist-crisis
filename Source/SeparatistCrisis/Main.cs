using HarmonyLib;
using SeparatistCrisis.Patches;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
            new MainMenuPatches();
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
