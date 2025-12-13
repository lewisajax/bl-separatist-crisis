using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;

namespace SeparatistCrisis.ViewModels
{
    public class MainAgentAbilityEquipVM: ViewModel
    {
        private EquipmentActionItemVM _lastSelectedItem;
        private Action<int> _toggleItem;
        private TextObject _dropTextObject = new TextObject("{=d1tCz15N}Hold to Equip", null);
        private MBBindingList<ControllerEquippedItemVM> _equipActions;
        private bool _isActive;
        private string _holdToDropText;
        private string _pressToEquipText;

        public AbilityHero? AbilityHero { get; private set; }
        public int WieldedIndex { get; set; } = -1; // We will store this someplace as so that we don't have to reference this behaviour

        [DataSourceProperty]
        public MBBindingList<ControllerEquippedItemVM> EquippedAbilities
        {
            get
            {
                return this._equipActions;
            }
            set
            {
                if (value != this._equipActions)
                {
                    this._equipActions = value;
                    base.OnPropertyChangedWithValue<MBBindingList<ControllerEquippedItemVM>>(value, "EquippedAbilities");
                }
            }
        }

        [DataSourceProperty]
        public string HoldToDropText
        {
            get
            {
                return this._holdToDropText;
            }
            set
            {
                bool flag = value != this._holdToDropText;
                if (flag)
                {
                    this._holdToDropText = value;
                    base.OnPropertyChangedWithValue<string>(value, "HoldToDropText");
                }
            }
        }

        [DataSourceProperty]
        public string PressToEquipText
        {
            get
            {
                return this._pressToEquipText;
            }
            set
            {
                if (value != this._pressToEquipText)
                {
                    this._pressToEquipText = value;
                    base.OnPropertyChangedWithValue<string>(value, "PressToEquipText");
                }
            }
        }

        [DataSourceProperty]
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    base.OnPropertyChangedWithValue(value, "IsActive");
                }
            }
        }

        public MainAgentAbilityEquipVM(Action<int> toggleItem)
        {
            this._toggleItem = toggleItem;
            this.EquippedAbilities = new MBBindingList<ControllerEquippedItemVM>();
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.PressToEquipText = new TextObject("{=HEEZhL90}Press to Equip", null).ToString();
        }

        public void InitializeMainAgentProperties()
        {
            Mission.Current.OnMainAgentChanged += this.OnMainAgentChanged;
            this.OnMainAgentChanged(null);
        }

        private void OnMainAgentChanged(Agent oldAgent)
        {
            if (oldAgent != null)
            {
                this.EquippedAbilities.Clear();
            }

            if (Agent.Main != null)
                this.AbilityHero = AbilityHero.Find(Agent.Main.Character);
        }

        // If we have the option to change abilities outside of the radial menu, then we'll need to set up an event or something
        private void OnMainAgentWeaponChange()
        {
            this.UpdateItemsWieldStatus();
        }

        public void OnToggle(bool isEnabled)
        {
            if (this.AbilityHero == null)
                return;

            if (this.AbilityHero.Abilities.Count <= 0)
                return;

            this.EquippedAbilities.ApplyActionOnAllItems(delegate (ControllerEquippedItemVM o)
            {
                o.OnFinalize();
            });
            this.EquippedAbilities.Clear();
            if (isEnabled)
            {
                this.EquippedAbilities.Add(new ControllerEquippedItemVM(GameTexts.FindText("str_cancel", null).ToString(), null, -1, null, new Action<EquipmentActionItemVM>(this.OnItemSelected)));

                int numAbilities = this.AbilityHero.Abilities.Count;
                for (int x = 0; x < numAbilities; x++)
                {
                    string itemTypeAsString = "Shield"; // Will need to look at adding our own icon(s)
                    string abilityName = this.AbilityHero.Abilities[x].Name;
                    this.EquippedAbilities.Add(new ControllerEquippedItemVM(abilityName, itemTypeAsString, x, 
                        MainAgentAbilityEquipVM.GetWeaponHotKey(x, numAbilities), 
                        new Action<EquipmentActionItemVM>(this.OnItemSelected)
                        )
                    );
                }

                this.UpdateItemsWieldStatus();
            }
            else
            {
                if (this._lastSelectedItem != null)
                {
                    Action<int> toggleItem = this._toggleItem;

                    if (toggleItem != null)
                        toggleItem((int)this._lastSelectedItem.Identifier);
                }

                this._lastSelectedItem = null;
            }
            this.IsActive = isEnabled;
        }

        private void OnItemSelected(EquipmentActionItemVM selectedItem)
        {
            if (this._lastSelectedItem != selectedItem)
            {
                this._lastSelectedItem = selectedItem;
            }
        }

        public void OnCancelHoldController()
        {
        }

        /*public void OnWeaponDroppedAtIndex(int droppedWeaponIndex)
        {
            this.OnToggle(true);
        }*/

        private bool IsWieldedWeaponAtIndex(int index)
        {
            return index == this.WieldedIndex;
        }

        public void OnAbilityEquippedAtIndex(int equippedWeaponIndex)
        {
            this.UpdateItemsWieldStatus();
        }

        /*public void SetDropProgressForIndex(EquipmentIndex eqIndex, float progress)
        {
            int i = 0;
            while (i < this.EquippedAbilities.Count)
            {
                object identifier;
                if (!((identifier = this.EquippedAbilities[i].Identifier) is EquipmentIndex))
                {
                    goto IL_31;
                }
                EquipmentIndex equipmentIndex = (EquipmentIndex)identifier;
                if (equipmentIndex != eqIndex || progress <= 0.2f)
                {
                    goto IL_31;
                }
                float num = progress;
            IL_39:
                float dropProgress = num;
                this.EquippedAbilities[i].DropProgress = dropProgress;
                i++;
                continue;
            IL_31:
                num = 0f;
                goto IL_39;
            }
        }*/

        private void UpdateItemsWieldStatus()
        {
            for (int i = 0; i < this.EquippedAbilities.Count; i++)
            {
                this.EquippedAbilities[i].IsWielded = this.IsWieldedWeaponAtIndex(i);
            }
        }

        private string GetAbilityName(MissionWeapon weapon)
        {
            string text = weapon.Item.Name.ToString();
            WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
            if (currentUsageItem != null && currentUsageItem.IsShield)
            {
                text = string.Concat(new object[]
                {
                    text,
                    " (",
                    weapon.HitPoints,
                    " / ",
                    weapon.ModifiedMaxHitPoints,
                    ")"
                });
            }
            else
            {
                WeaponComponentData currentUsageItem2 = weapon.CurrentUsageItem;
                if (currentUsageItem2 != null && currentUsageItem2.IsConsumable && weapon.ModifiedMaxAmount > 1)
                {
                    text = string.Concat(new object[]
                    {
                        text,
                        " (",
                        weapon.Amount,
                        " / ",
                        weapon.ModifiedMaxAmount,
                        ")"
                    });
                }
            }
            return text;
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            this.EquippedAbilities.ApplyActionOnAllItems(delegate (ControllerEquippedItemVM o)
            {
                o.OnFinalize();
            });
            this.EquippedAbilities.Clear();
        }

        // Do we want more than 4 abilities on the radial menu?
        private static HotKey GetWeaponHotKey(int currentIndexOfWeapon, int totalNumOfWeapons)
        {
            if (currentIndexOfWeapon == 0)
            {
                if (totalNumOfWeapons == 1)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility4");
                }
                if (totalNumOfWeapons > 1)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility1");
                }
                Debug.FailedAssert("Wrong number of total weapons!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\HUD\\MissionMainAgentControllerEquipDropVM.cs", "GetWeaponHotKey", 182);
            }
            else if (currentIndexOfWeapon == 1)
            {
                if (totalNumOfWeapons == 2)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility3");
                }
                if (totalNumOfWeapons > 2)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility4");
                }
            }
            else
            {
                if (currentIndexOfWeapon == 2)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility3");
                }
                if (currentIndexOfWeapon == 3)
                {
                    return HotKeyManager.GetCategory("CombatHotKeyCategory").GetHotKey("ControllerEquipAbility2");
                }
                Debug.FailedAssert("Wrong index of current weapon. Cannot be higher than 3", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.ViewModelCollection\\HUD\\MissionMainAgentControllerEquipDropVM.cs", "GetWeaponHotKey", 206);
            }
            return null;
        }

        public void OnGamepadActiveChanged(bool isActive)
        {
            this.HoldToDropText = (isActive ? this._dropTextObject.ToString() : string.Empty);
        }
    }
}
