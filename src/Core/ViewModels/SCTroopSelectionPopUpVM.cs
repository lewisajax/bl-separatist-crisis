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

        public Action? OnDone { get; set; }

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

        public void OpenPopUp(string title)
        {
            this.ExecuteReset();

            if (this.ItemList != null && !this.ItemList.HasSearchFilter)
            {
                // We add the search filter but we don't have it show up in the UI
                EncyclopediaFilterGroup filterGroup = new EncyclopediaFilterGroup(new List<EncyclopediaFilterItem>()
                {
                    // Captures names and ids
                    new EncyclopediaFilterItem(new TextObject("Search"), (object f) => ((BasicCharacterObject)f).Name.Value.ToLower().Contains(this.SearchText.ToLower()) || ((BasicCharacterObject)f).StringId.ToLower().Contains(this.SearchText.ToLower()))
                }, new TextObject("Other"));

                var _ = filterGroup.Filters.All(x => x.IsActive = true);

                // Atm, with ~1000 entries, there's no need for debouncing. Might need to revisit this later though.
                this.ItemList.Filters.Add(filterGroup);
                this.ItemList.HasSearchFilter = true;
            }

            this.IsOpen = true;
            this.RefreshValues();
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

        public void ExecuteReset()
        {
            if (ItemList != null)
            {
                IEnumerable<BasicCharacterObject> troopCharacters = new MBBindingList<CustomBattleTroopTypeVM>[] {
                    this.ItemList.MeleeTroopTypes, this.ItemList.RangedTroopTypes, this.ItemList.CavalryTroopTypes, this.ItemList.MountedArcherTroopTypes
                }.SelectMany(x => x.Aggregate(new List<BasicCharacterObject>(), (prev, curr) =>
                {
                    if (curr.IsSelected)
                        prev.Add(curr.Character);

                    return prev;
                }));

                this.SetInitialTroops(troopCharacters);
            }
        }

        // This could definitely do with some optimising. Noticeable freeze which will no doubt get worse, the more troops we add.
        public void SetInitialTroops(IEnumerable<BasicCharacterObject> troops)
        {
            if (this.ItemList != null)
            {
                foreach (SCTroopSelectionListItemVM itemVM in this.ItemList.Items)
                {
                    if (troops.Contains((BasicCharacterObject)itemVM.Object))
                        itemVM.IsBookmarked = true;
                    else
                        itemVM.IsBookmarked = false;
                }
            }
        }

        public void ExecuteCancel()
        {
            Action? onPopUpClosed = this.OnPopUpClosed;
            if (onPopUpClosed != null)
            {
                onPopUpClosed();
            }
            this.IsOpen = false;
        }

        public void ExecuteDone()
        {
            // Don't let them leave the popup if there's no troops selected. We should look at disabling the 'Done' button instead.
            if (this.ItemList?.SelectedItems.Count <= 0) return;

            this.IsOpen = false;

            // Do we handle the troop assignment in here or should we hand it off the ArmyCompItemVM?

            if (this.ItemList != null)
            {
                // There are caching opportunities here. Don't ignore yourself
                this.ItemList.MeleeTroopTypes.Clear();
                this.ItemList.RangedTroopTypes.Clear();
                this.ItemList.CavalryTroopTypes.Clear();
                this.ItemList.MountedArcherTroopTypes.Clear();

                MBReadOnlyList<SkillObject> allSkills = Game.Current.ObjectManager.GetObjectTypeList<SkillObject>();

                foreach (SCTroopSelectionListItemVM item in this.ItemList.SelectedItems)
                {
                    BasicCharacterObject? character = item.Object as BasicCharacterObject;

                    if (character != null)
                    {
                        switch(character.GetFormationClass())
                        {
                            case FormationClass.HorseArcher:
                                CustomBattleTroopTypeVM troopType = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(this.OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.RangedCavalry), allSkills, false);
                                troopType.IsSelected = true;
                                this.ItemList.MountedArcherTroopTypes.Add(troopType);
                                break;
                            case FormationClass.Cavalry:
                                CustomBattleTroopTypeVM troopType2 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(this.OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.MeleeCavalry), allSkills, false);
                                troopType2.IsSelected = true;
                                this.ItemList.CavalryTroopTypes.Add(troopType2);
                                break;
                            case FormationClass.Ranged:
                                CustomBattleTroopTypeVM troopType3 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(this.OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.RangedInfantry), allSkills, false);
                                troopType3.IsSelected = true;
                                this.ItemList.RangedTroopTypes.Add(troopType3);
                                break;
                            case FormationClass.Infantry:
                                CustomBattleTroopTypeVM troopType4 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(this.OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.MeleeInfantry), allSkills, false);
                                troopType4.IsSelected = true;
                                this.ItemList.MeleeTroopTypes.Add(troopType4);
                                break;
                            default:
                                return;
                        }
                    }
                }

                this.OnDone?.Invoke();
            }
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
