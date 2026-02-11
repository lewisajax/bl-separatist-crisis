using Bannerlord.UIExtenderEx;
using Newtonsoft.Json.Serialization;
using SandBox;
using SeparatistCrisis.Behaviors;
using SeparatistCrisis.Extensions;
using SeparatistCrisis.InputSystem;
using SeparatistCrisis.MissionManagers;
using SeparatistCrisis.Missions;
using SeparatistCrisis.ObjectTypes;
using SeparatistCrisis.PatchTools;
using SeparatistCrisis.SetOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Engine.InputSystem;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.GameKeyCategory;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis
{
    public sealed class SubModule : MBSubModuleBase
    {
        public static readonly string Version = $"v{typeof(SubModule).Assembly.GetName().Version!.ToString(3)}";
        public static readonly string Name = typeof(SubModule).Namespace!;
        public static readonly string DisplayName = new TextObject($"{{=MYz8nKqq}}{Name}.Core").ToString();
        public static readonly string MainHarmonyDomain = "bannerlord." + Name.ToLower(System.Globalization.CultureInfo.CurrentCulture);
        public static readonly string CampaignHarmonyDomain = MainHarmonyDomain + ".campaign";
        public static readonly string WidgetHarmonyDomain = MainHarmonyDomain + ".widgets";

        internal static readonly Color StdTextColor = Color.FromUint(0x00F16D26); // Orange

        internal static SubModule Instance { get; set; } = default!;

        private bool _hasLoaded;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            AppDomain.CurrentDomain.UnhandledException += SubModule.OnError;

            SubModule.Instance = this;
            PatchManager.ApplyMainPatches(MainHarmonyDomain);

            var extender = UIExtender.Create(Name);
            extender.Register(typeof(SubModule).Assembly);
            extender.Enable();

            this.InitializeHotKeyManager(true);

            // Using the launcher.exe will reinitialize the hotkeys a 2nd time, overwriting our first init, where as using bannerlord.exe will only initialize it once during the startup screen.
            // This might cause async issues, if so opt for patching the methods in ViewSubModule
            Input.OnControllerTypeChanged = (Action<Input.ControllerTypes>)Delegate.Combine(Input.OnControllerTypeChanged, new Action<Input.ControllerTypes>(this.OnControllerTypeChanged));
            NativeOptions.OnNativeOptionChanged = (NativeOptions.OnNativeOptionChangedDelegate)Delegate.Combine(NativeOptions.OnNativeOptionChanged, new NativeOptions.OnNativeOptionChangedDelegate(this.OnNativeOptionChanged));
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            //if (mission != null)
            //    mission.AddMissionBehavior(new ForceAtmosphereLogic());
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!_hasLoaded)
            {
                _hasLoaded = true;

                InformationManager.DisplayMessage(new InformationMessage(new TextObject($"{{=hPERH3u4}}Loaded {{NAME}}").SetTextVariable("NAME", DisplayName).ToString(), StdTextColor));
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game != null && game.GameType is Campaign)
            {
                PatchManager.ApplyCampaignPatches(CampaignHarmonyDomain);

                var gameStarter = (CampaignGameStarter) gameStarterObject;
            }
        }

        public override void BeginGameStart(Game game)
        {
            if (game?.ObjectManager != null) 
            {
                if (game.GameType.GetType() == typeof(CustomGame) || game.GameType.GetType() == typeof(Campaign))
                {
                    game.ObjectManager.RegisterType<Blaster>("Blaster", "Blasters", 100U, true);
                    game.ObjectManager.RegisterType<Ability>("Ability", "Abilities", 101U, true);
                    MBObjectManager.Instance.LoadXML("Blasters", false);
                    MBObjectManager.Instance.LoadXML("Abilities", false);
                }
            }
        }

        public override void OnGameInitializationFinished(Game game)
        {
            if (game?.ObjectManager != null)
            {
                // CustomGame loads the NPCCharacters near the end of the pipeline. We need to go after it for BasicCharacterObjects
                if (game.GameType.GetType() == typeof(CustomGame) || game.GameType.GetType() == typeof(Campaign))
                {
                    game.ObjectManager.RegisterType<AbilityHero>("AbilityHero", "AbilityHeroes", 102U, true);
                    game.ObjectManager.RegisterType<AssignedSet>("AssignedSet", "AssignedSets", 103U, true);
                    MBObjectManager.Instance.LoadXML("AbilityHeroes", false);
                    MBObjectManager.Instance.LoadXML("AssignedSets", false);
                }

                // We override SandBoxSubModule's CampaignMissionManager assignment since our mod loads after SandBox
                if (game.GameType.GetType() == typeof(Campaign))
                {
                    ((Campaign)game.GameType).CampaignMissionManager = new SCCampaignMissionManager();
                }
            }
        }

        private T? GetGameModel<T>(IGameStarter gameStarterObject) where T : GameModel
        {
            var models = gameStarterObject.Models.ToArray();

            for (int index = models.Length - 1; index >= 0; --index)
            {
                if (models[index] is T gameModel1)
                    return gameModel1;
            }
            return default;
        }

        private void OnControllerTypeChanged(Input.ControllerTypes newType)
        {
            this.ReInitializeHotKeyManager();
        }

        private void OnNativeOptionChanged(NativeOptions.NativeOptionsType changedNativeOptionsType)
        {
            if (changedNativeOptionsType == NativeOptions.NativeOptionsType.EnableTouchpadMouse)
            {
                this.ReInitializeHotKeyManager();
            }
        }

        private void InitializeHotKeyManager(bool loadKeys)
        {
            Dictionary<string, GameKeyContext>.ValueCollection prevContexts = HotKeyManager.GetAllCategories();
            List<GameKeyContext> newContexts = prevContexts.ToList();

            newContexts.Add(new SCGameKeyContext());
            newContexts.Add(new SCCombatHotKeyCategory());

            HotKeyManager.RegisterInitialContexts(newContexts, loadKeys);
        }

        private void ReInitializeHotKeyManager()
        {
            this.InitializeHotKeyManager(true);
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            if (game != null &&  game.GameType is Campaign)
            {
                //PatchManager.RemoveCampaignPatches();// Not sure we should do this...
            }

            // We could send out an event so the submodule doesn't need to know about the singletons
            SetAssignments.Instance.Dispose();
        }

        public static void OnError(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
    }
}