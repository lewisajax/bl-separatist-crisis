using System.IO;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Scenes
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
                // this.TransferShaders(SubModule.Name);
            }
        }

        // John_m from KoA or sz from touhou mod both shared a similar script to copy shaders over to the base directory's shaders
        private void TransferShaders(string moduleName)
        {
            string modulePath = ModuleHelper.GetModuleFullPath(moduleName);
            if (modulePath == null || modulePath.Length <= 0)
            {
                MBDebug.Print("Could not find the module path.");
                return;
            }

            string? gamePath = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(typeof(BasePath).Assembly.Location, "/../../Shaders"));
            if (gamePath == null)
            {
                MBDebug.Print("There is no Shaders folder to be found in the base game directory.");
                return;
            }

            // Looks like they've changed how they deal with file paths
            // Use Common.PlatformFileHelper instead
            // There was a .CopyDirectory or .CopyFolder method with recursion that looks to do what I need

            string moduleShaders = System.IO.Path.Combine(modulePath, "/ModuleData/Shaders");

            string[] subDirs = Directory.GetDirectories(moduleShaders);
            foreach (string subDir in subDirs)
            {
                InformationManager.DisplayMessage(new InformationMessage($"{subDir}", StdTextColor));
            }

            // string moduleShaders = Directory.GetFiles($"{modulePath}/ModuleData/Shaders/Sources")

        }
    }
}