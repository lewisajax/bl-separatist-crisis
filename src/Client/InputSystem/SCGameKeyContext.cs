using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.InputSystem
{
    // Butterlib has a nice input wrapper which has events that we can listen for instead of polling the default input manager 
    // However, having butterlib as a dependency will force us to use hard versions for certain packages
    // Will we ever need to use said packages, who knows
    // Might just copy the input system from it, seeing as we only need the 1 feature
    // If we need to use the other features it adds, then we'll just add butterlib as a dependency
    // We'll most likely end up doing so in the future regardless
    public sealed class SCGameKeyContext: GameKeyContext
    {
        public static SCGameKeyContext Current { get; private set; } = null!;

        public SCGameKeyContext() : base("SCGameKeyContext", 108 + 1 + 1, GameKeyContext.GameKeyContextType.Default)
        {
            SCGameKeyContext.Current = this;
            this.RegisterHotKeys();
            this.RegisterGameKeys();
            this.RegisterGameAxisKeys();
        }

        private void RegisterHotKeys()
        {
            /*List<Key> firingModeKeys = new List<Key>
            {
                new Key(InputKey.RightMouseButton),
                new Key(InputKey.X)
            };

            base.RegisterHotKey(new HotKey("SwitchFiringMode", "SeparatistCrisis", firingModeKeys, HotKey.Modifiers.None, HotKey.Modifiers.None), true);*/
        }

        private void RegisterGameKeys()
        {
            // ChatLog seems to overwrite the registeredGameKeys since it uses OnLateTick to register it's stuff
            base.RegisterGameKey(new GameKey(109, "SwitchFiringMode", "SeparatistCrisis", InputKey.MiddleMouseButton, "SCGameKeyContext"), true);
        }

        private void RegisterGameAxisKeys()
        {
        }
    }
}
