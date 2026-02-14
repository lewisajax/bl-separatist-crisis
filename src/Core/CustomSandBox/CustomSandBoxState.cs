using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace SeparatistCrisis.CustomSandBox
{
    public class CustomSandBoxState: GameState
    {
        public override bool IsMusicMenuState
        {
            get
            {
                return true;
            }
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }
    }
}
