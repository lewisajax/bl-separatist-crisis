using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Entities
{
    public class MissileAgent
    {
        private RangedWeaponOptions? _weaponOptions;
        private int _currFiringModeIndex;

        public float LastShot { get; set; }
        public bool CanFire { get; set; }
        public float ShootInterval { get; set; } = 1f;
        public FiringMode CurrentFiringMode { get; set; } = FiringMode.NONE;

        public int CurrentFiringModeIndex
        {
            get
            {
                return _currFiringModeIndex;
            }
            set
            {
                this._currFiringModeIndex = value;

                if (this.WeaponOptions != null)
                {
                    this.ShootInterval = this.WeaponOptions.ShootIntervals[value];
                    this.CurrentFiringMode = this.WeaponOptions.FiringModeFlags[value];
                }
            }
        }

        public RangedWeaponOptions? WeaponOptions
        {
            get
            {
                return this._weaponOptions;
            }
            set
            {
                this._weaponOptions = value;
                this.CurrentFiringModeIndex = 0;

                if (value != null)
                    this.CurrentFiringMode = value.FiringModeFlags[0];
            }
        }

        public MissileAgent(Agent agent)
        {
            this.LastShot = Mission.Current.CurrentTime;
            this.CanFire = true;

            RangedWeaponOptions? options = RangedWeaponOptions.FindFirst(x => x.WeaponId == agent?.WieldedWeapon.Item?.StringId);
            this.WeaponOptions = options;
            this.CurrentFiringModeIndex = 0;

            if (options == null)
                this.CanFire = false;
        }
    }
}
