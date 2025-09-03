using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace SeparatistCrisis.ViewModels
{
    public class SCTroopSelectionPopUpVM : ViewModel
    {
        private List<bool> _itemSelectionsBackUp = null!;

        private int _selectedItemCount;

        private InputKeyItemVM _doneInputKey = null!;

        private InputKeyItemVM _cancelInputKey = null!;

        private InputKeyItemVM _resetInputKey = null!;

        private MBBindingList<CustomBattleTroopTypeVM> _items = null!;

        private string _title = null!;

        private string _doneLbl = null!;

        private string _cancelLbl = null!;

        private string _selectAllLbl = null!;

        private string _backToDefaultLbl = null!;

        private bool _isOpen;

        private string _searchText = "";

        private int _minCharAmountToShowResults = 3;

        public Action? OnPopUpClosed { get; set; }

        public SCTroopSelectionItemListVM? _itemListVM { get; set; }

        [DataSourceProperty]
        public string SearchText
        {
            get
            {
                return this._searchText;
            }
            set
            {
                if (value != this._searchText)
                {
                    bool isAppending = value.ToLower().Contains(this._searchText);
                    bool isPasted = string.IsNullOrEmpty(this._searchText) && !string.IsNullOrEmpty(value);
                    this._searchText = value.ToLower();
                    Debug.Print("isAppending: " + isAppending.ToString() + " isPasted: " + isPasted.ToString(), 0, Debug.DebugColor.White, 17592186044416UL);
                    // this.ItemList?.RefreshSearch(isAppending, isPasted); // The vanilla search bar has some stuff for asian characters. If translations dont work, look at that.
                    this.ItemList?.UpdateFilters();
                    base.OnPropertyChangedWithValue<string>(value, "SearchText");
                }
            }
        }

        [DataSourceProperty]
        public int MinCharAmountToShowResults
        {
            get
            {
                return this._minCharAmountToShowResults;
            }
            set
            {
                if (value != this._minCharAmountToShowResults)
                {
                    this._minCharAmountToShowResults = value;
                    base.OnPropertyChangedWithValue(value, "MinCharAmountToShowResults");
                }
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionItemListVM? ItemList
        {
            get
            {
                return this._itemListVM;
            }
            set
            {
                if (value != this._itemListVM)
                {
                    this._itemListVM = value;
                    base.OnPropertyChangedWithValue<SCTroopSelectionItemListVM?>(value, "ItemList");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM DoneInputKey
        {
            get
            {
                return this._doneInputKey;
            }
            set
            {
                if (value != this._doneInputKey)
                {
                    this._doneInputKey = value;
                    base.OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM CancelInputKey
        {
            get
            {
                return this._cancelInputKey;
            }
            set
            {
                if (value != this._cancelInputKey)
                {
                    this._cancelInputKey = value;
                    base.OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM ResetInputKey
        {
            get
            {
                return this._resetInputKey;
            }
            set
            {
                if (value != this._resetInputKey)
                {
                    this._resetInputKey = value;
                    base.OnPropertyChangedWithValue<InputKeyItemVM>(value, "ResetInputKey");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleTroopTypeVM> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                if (value != this._items)
                {
                    this._items = value;
                    base.OnPropertyChangedWithValue<MBBindingList<CustomBattleTroopTypeVM>>(value, "Items");
                }
            }
        }

        [DataSourceProperty]
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if (value != this._title)
                {
                    this._title = value;
                    base.OnPropertyChangedWithValue<string>(value, "Title");
                }
            }
        }

        [DataSourceProperty]
        public string DoneLbl
        {
            get
            {
                return this._doneLbl;
            }
            set
            {
                if (value != this._doneLbl)
                {
                    this._doneLbl = value;
                    base.OnPropertyChangedWithValue<string>(value, "DoneLbl");
                }
            }
        }

        [DataSourceProperty]
        public string CancelLbl
        {
            get
            {
                return this._cancelLbl;
            }
            set
            {
                if (value != this._cancelLbl)
                {
                    this._cancelLbl = value;
                    base.OnPropertyChangedWithValue<string>(value, "CancelLbl");
                }
            }
        }

        [DataSourceProperty]
        public string SelectAllLbl
        {
            get
            {
                return this._selectAllLbl;
            }
            set
            {
                if (value != this._selectAllLbl)
                {
                    this._selectAllLbl = value;
                    base.OnPropertyChangedWithValue<string>(value, "SelectAllLbl");
                }
            }
        }

        [DataSourceProperty]
        public string BackToDefaultLbl
        {
            get
            {
                return this._backToDefaultLbl;
            }
            set
            {
                if (value != this._backToDefaultLbl)
                {
                    this._backToDefaultLbl = value;
                    base.OnPropertyChangedWithValue<string>(value, "BackToDefaultLbl");
                }
            }
        }

        [DataSourceProperty]
        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }
            set
            {
                if (value != this._isOpen)
                {
                    this._isOpen = value;
                    base.OnPropertyChangedWithValue(value, "IsOpen");
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.DoneLbl = GameTexts.FindText("str_done", null).ToString();
            this.CancelLbl = GameTexts.FindText("str_cancel", null).ToString();
            this.SelectAllLbl = GameTexts.FindText("str_custom_battle_select_all", null).ToString();
            this.BackToDefaultLbl = GameTexts.FindText("str_custom_battle_back_to_default", null).ToString();

            this.ItemList?.RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            this.DoneInputKey.OnFinalize();
            this.CancelInputKey.OnFinalize();
            this.ResetInputKey.OnFinalize();
        }

        public void OpenPopUp(string title, MBBindingList<CustomBattleTroopTypeVM> troops)
        {
            this._itemSelectionsBackUp = new List<bool>();
            if (troops != null)
            {
                foreach (CustomBattleTroopTypeVM customBattleTroopTypeVM in troops)
                {
                    this._itemSelectionsBackUp.Add(customBattleTroopTypeVM.IsSelected);
                }
                this._selectedItemCount = troops.Count((CustomBattleTroopTypeVM x) => x.IsSelected);
                this.Title = title;
                this.Items = troops;
                this.IsOpen = true;
            }

            EncyclopediaFilterGroup filterGroup = new EncyclopediaFilterGroup(new List<EncyclopediaFilterItem>()
            {
                new EncyclopediaFilterItem(new TextObject("Search"), (object f) => ((BasicCharacterObject)f).Name.Value.ToLower().Contains(this.SearchText.ToLower()))
            }, new TextObject("Other"));

            var _ = filterGroup.Filters.All(x => x.IsActive = true);
            this.ItemList?.Filters.Add(filterGroup);
        }

        public void OnItemSelectionToggled(CustomBattleTroopTypeVM item)
        {
            if (item == null) return;

            if (this._selectedItemCount > 1 || !item.IsSelected)
            {
                item.IsSelected = !item.IsSelected;
                this._selectedItemCount += (item.IsSelected ? 1 : -1);
            }
        }

        public void ExecuteSelectAll()
        {
            this.Items.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.IsSelected = true;
            });
            this._selectedItemCount = this.Items.Count;
        }

        public void ExecuteBackToDefault()
        {
            this.Items.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.IsSelected = x.IsDefault;
            });
            this._selectedItemCount = this.Items.Count((CustomBattleTroopTypeVM x) => x.IsSelected);
        }

        public void ExecuteCancel()
        {
            this.ExecuteReset();
            Action? onPopUpClosed = this.OnPopUpClosed;
            if (onPopUpClosed != null)
            {
                onPopUpClosed();
            }
            this.IsOpen = false;
        }

        public void ExecuteDone()
        {
            this.IsOpen = false;
        }

        public void ExecuteReset()
        {
            int count = this._itemSelectionsBackUp.Count;
            if (count != this.Items.Count)
            {
                Debug.FailedAssert("Backup troop count does not match with the actual troop count.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.CustomBattle\\CustomBattle\\TroopTypeSelectionPopUpVM.cs", "ExecuteReset", 100);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                this.Items[i].IsSelected = this._itemSelectionsBackUp[i];
            }
            this._selectedItemCount = this.Items.Count((CustomBattleTroopTypeVM x) => x.IsSelected);
        }

        public void SetCancelInputKey(HotKey hotkey)
        {
            this.CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetDoneInputKey(HotKey hotkey)
        {
            this.DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetResetInputKey(HotKey hotkey)
        {
            this.ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }
    }
}
