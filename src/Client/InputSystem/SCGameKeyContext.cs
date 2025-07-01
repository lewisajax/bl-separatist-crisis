using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.InputSystem
{
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
