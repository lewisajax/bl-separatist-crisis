using SeparatistCrisis.Entities;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Abilities
{
    public interface IAbility
    {
        public Ability Options { get; }
        public AbilityAgent AbilityAgent { get; }
        public bool IsActive { get; set; }
        public void OnTick(float dt);
    }
}
