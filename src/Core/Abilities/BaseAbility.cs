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
    public class BaseAbility
    {
        private Ability _options;
        private AbilityAgent _abilityAgent;

        public Ability Options
        {
            get
            {
                return this._options;
            }
        }

        public AbilityAgent AbilityAgent
        {
            get
            {
                return this._abilityAgent;
            }
        }

        public bool IsActive { get; set; }

        public virtual void OnTick(float dt) {}

        public BaseAbility(AbilityAgent abilityAgent, Ability options)
        {
            this._abilityAgent = abilityAgent;
            this._options = options;
        }

        public virtual void Dispose()
        {
            this._options = null;
            this._abilityAgent = null;
            this.IsActive = false;
        }
    }
}
