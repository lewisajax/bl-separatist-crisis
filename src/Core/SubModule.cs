using SeparatistCrisis.PatchTools;
using Bannerlord.UIExtenderEx;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using SeparatistCrisis.ObjectTypes;
using TaleWorlds.ObjectSystem;
using System.Collections.Generic;
using TaleWorlds.InputSystem;
using SeparatistCrisis.InputSystem;
using SeparatistCrisis.MissionManagers;
using SeparatistCrisis.Components;

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
            SubModule.Instance = this;
            PatchManager.ApplyMainPatches(MainHarmonyDomain);

            var extender = UIExtender.Create(Name);
            extender.Register(typeof(SubModule).Assembly);
            extender.Enable();

            this.InitializeHotKeyManager(true);
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

        public override void OnGameInitializationFinished(Game game)
        {
            // We override SandBoxSubModule's CampaignMissionManager assignment since our mod loads after SandBox
            Campaign campaign = game.GameType as Campaign;
            if (campaign != null)
            {
                campaign.CampaignMissionManager = new SCCampaignMissionManager();
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game != null && game.GameType is Campaign)
            {
                PatchManager.ApplyCampaignPatches(CampaignHarmonyDomain);

                var gameStarter = (CampaignGameStarter) gameStarterObject;
                this.OnRegisterTypes();
            }
        }

        private void OnRegisterTypes()
        {
            MBObjectManager.Instance.RegisterType<SettlementGroup>("SettlementGroup", "SettlementGroups", 100U, true);
            MBObjectManager.Instance.RegisterType<SettlementGroupComponent>("SettlementGroupComponent", "SettlementGroupComponents", 101U, true);
        }

        public override void RegisterSubModuleObjects(bool isSavedCampaign)
        {
            MBObjectManager.Instance.LoadXML("SettlementGroups", false);
        }

        public override void BeginGameStart(Game game)
        {
            if (game.GameType.GetType() == typeof(Campaign))
            {
                if (game.ObjectManager != null)
                {
                    game.ObjectManager.RegisterType<RangedWeaponOptions>("RangedWeaponOptions", "RangedWeaponOptionSets", 100U, true);
                    MBObjectManager.Instance.LoadXML("RangedWeaponOptionSets", false);
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

        private void InitializeHotKeyManager(bool loadKeys)
        {
            Dictionary<string, GameKeyContext>.ValueCollection prevContexts = HotKeyManager.GetAllCategories();
            List<GameKeyContext> newContexts = prevContexts.ToList();

            newContexts.Add(new SCGameKeyContext());

            HotKeyManager.RegisterInitialContexts(newContexts, loadKeys);
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);

            if (game != null &&  game.GameType is Campaign)
            {
                //PatchManager.RemoveCampaignPatches();// Not sure we should do this...
            }
        }
    }
}