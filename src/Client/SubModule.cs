using SeparatistCrisis.PatchTools;

using Bannerlord.UIExtenderEx;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using SeparatistCrisis.Missions;
using SeparatistCrisis.ObjectTypes;
using TaleWorlds.ObjectSystem;
using SandBox.Objects;
using SandBox;
using SeparatistCrisis.Components;

namespace SeparatistCrisis
{
    public sealed class SubModule : MBSubModuleBase
    {
        public static readonly string Version = $"v{typeof(SubModule).Assembly.GetName().Version!.ToString(3)}";
        public static readonly string Name = typeof(SubModule).Namespace!;
        public static readonly string DisplayName = new TextObject($"{{=MYz8nKqq}}{Name}").ToString();
        public static readonly string MainHarmonyDomain = "bannerlord." + Name.ToLower(System.Globalization.CultureInfo.CurrentCulture);
        public static readonly string CampaignHarmonyDomain = MainHarmonyDomain + ".campaign";
        public static readonly string WidgetHarmonyDomain = MainHarmonyDomain + ".widgets";

        internal static readonly Color StdTextColor = Color.FromUint(0x00F16D26); // Orange

        internal static SubModule Instance { get; set; } = default!;

        private bool _hasLoaded;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;

            var extender = UIExtender.Create(Name);
            extender.Register(typeof(SubModule).Assembly);
            extender.Enable();

            PatchManager.ApplyMainPatches(MainHarmonyDomain);
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            if (mission != null)
                mission.AddMissionBehavior(new LogicForceAtmosphereMission());
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