using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

// Maybe holding down x will switch firing modes

/*
 * id: string
 * weaponId: string
 * firingModeFlags: uint
 * scopeUi?? (For faction specific snipers maybe)
 */

namespace SeparatistCrisis.ObjectTypes
{
    [Flags]
    public enum FiringMode
    {
        NONE = 0x0,
        FULLAUTO = 0x01,
        SHOTGUN = 0x02,
        // REPEATER_VERTICAL = 0x04,
        // REPEATER_HORIZONTAL = 0x08,
        // SNIPER = 0x10,
    }

    public class RangedWeaponOptions : MBObjectBase
    {
        public string WeaponId { get; private set; } = null!;

        public List<FiringMode> FiringModeFlags { get; private set; } = new List<FiringMode>();

        public List<float> ShootIntervals { get; private set; } = new List<float>();

        public RangedWeaponOptions(): this(string.Empty, new List<FiringMode>(), new List<float> { }) {}

        public RangedWeaponOptions(string weaponId, List<FiringMode> firingModeFlags, List<float> shootIntervals)
        {
            this.WeaponId = weaponId;
            this.FiringModeFlags = firingModeFlags;
            this.ShootIntervals = shootIntervals;
        }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            bool isInitialized = base.IsInitialized;
            base.Deserialize(objectManager, node);

            if (node == null)
                return;

            string? wepId = node.Attributes?["weapon_id"]?.Value;

            if (wepId != null)
                this.WeaponId = wepId;
            else
                this.WeaponId = string.Empty;

            // Hex string
            string? modeFlags = node.Attributes?["firing_mode_flags"]?.Value;

            // Need to remove leading 0x for TryParse to work
            if (modeFlags != null)
                modeFlags = modeFlags.Substring(2);

            if (uint.TryParse(modeFlags, NumberStyles.HexNumber, null, out uint flagRes) && flagRes > 0)
            {
                foreach (FiringMode val in Enum.GetValues(((FiringMode)flagRes).GetType()))
                {
                    if ((val & (FiringMode)flagRes) > 0)
                        this.FiringModeFlags.Add(val);
                }
            }
            else
            {
                Debug.Print("Firing mode flag string could not be parsed");
            }

            // Can be a string of multiple intervals
            string? intervals = node.Attributes?["shoot_intervals"]?.Value;

            if (intervals != null)
            {
                char[] match = new char[] { ',' };
                string[] intervalsSplit = intervals.Split(match, StringSplitOptions.RemoveEmptyEntries);

                foreach (string val in intervalsSplit)
                {
                    if (float.TryParse(val, out float interval))
                    {
                        this.ShootIntervals.Add(interval);
                    }
                }
            }

            if (this.ShootIntervals.Count != this.FiringModeFlags.Count)
                Debug.Print("ShootIntervals has a different size to FiringModeFlags");

        }

        public static MBReadOnlyList<RangedWeaponOptions> All
        {
            get
            {
                return MBObjectManager.Instance.GetObjectTypeList<RangedWeaponOptions>();
            }
        }

        public static RangedWeaponOptions Find(string idString)
        {
            return MBObjectManager.Instance.GetObject<RangedWeaponOptions>(idString);
        }

        public static RangedWeaponOptions FindFirst(Func<RangedWeaponOptions, bool> predicate)
        {
            return RangedWeaponOptions.All.FirstOrDefault(predicate);
        }

        public static IEnumerable<RangedWeaponOptions> FindAll(Func<RangedWeaponOptions, bool> predicate)
        {
            return RangedWeaponOptions.All.Where(predicate);
        }
    }
}
