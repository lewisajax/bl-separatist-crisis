using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Items
{
    public sealed class SubModule : MBSubModuleBase
    {
        public static readonly string Version = $"v{typeof(SubModule).Assembly.GetName().Version!.ToString(3)}";
        public static readonly string Name = typeof(SubModule).Namespace!;
        public static readonly string DisplayName = new TextObject($"{{=MYz8nKqq}}{Name}").ToString();

        internal static readonly Color StdTextColor = Color.FromUint(0x00F16D26); // Orange

        internal static SubModule Instance { get; set; } = default!;

        private bool _hasLoaded;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            SubModule.Instance = this;
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
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
    }
}