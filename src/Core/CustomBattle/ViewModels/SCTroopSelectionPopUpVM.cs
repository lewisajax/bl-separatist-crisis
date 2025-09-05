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

// - Disabling/grey out done button when 0 troops selected - Would need to setup our own Standard.TriplePopupCloseButtons xml/widget.

// - Culture icon above faction dropdown - See CustomBattleScreen.xml.
//   If we want to just have the icon without the coloured background, we'd have to set up our own banner_icons.xml since there's no default transparent colours to be had.

// - Troop cards (optional) - It's pretty heavy and I don't think that NavigatableList is a virtual list. I believe that everything gets loaded at once.
//   Uncomment the Visual datasource in SCTroopSelectionListItemVM as well as the xml to see it.

namespace SeparatistCrisis.CustomBattle
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
                return _searchText;
            }
            set
            {
                if (value != null && value != _searchText)
                {
                    bool isAppending = value.ToLower().Contains(_searchText);
                    bool isPasted = string.IsNullOrEmpty(_searchText) && !string.IsNullOrEmpty(value);
                    _searchText = value.ToLower();
                    Debug.Print("isAppending: " + isAppending.ToString() + " isPasted: " + isPasted.ToString(), 0, Debug.DebugColor.White, 17592186044416UL);
                    // this.ItemList?.RefreshSearch(isAppending, isPasted); // The vanilla search bar has some stuff for asian?? characters. If translations dont work, look at that.
                    ItemList?.UpdateFilters();
                    OnPropertyChangedWithValue(value, "SearchText");
                }
            }
        }

        [DataSourceProperty]
        public int MinCharAmountToShowResults
        {
            get
            {
                return _minCharAmountToShowResults;
            }
            set
            {
                if (value != _minCharAmountToShowResults)
                {
                    _minCharAmountToShowResults = value;
                    OnPropertyChangedWithValue(value, "MinCharAmountToShowResults");
                }
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionItemListVM? ItemList
        {
            get
            {
                return _itemListVM;
            }
            set
            {
                if (value != _itemListVM)
                {
                    _itemListVM = value;
                    OnPropertyChangedWithValue(value, "ItemList");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM DoneInputKey
        {
            get
            {
                return _doneInputKey;
            }
            set
            {
                if (value != _doneInputKey)
                {
                    _doneInputKey = value;
                    OnPropertyChangedWithValue(value, "DoneInputKey");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM CancelInputKey
        {
            get
            {
                return _cancelInputKey;
            }
            set
            {
                if (value != _cancelInputKey)
                {
                    _cancelInputKey = value;
                    OnPropertyChangedWithValue(value, "CancelInputKey");
                }
            }
        }

        [DataSourceProperty]
        public InputKeyItemVM ResetInputKey
        {
            get
            {
                return _resetInputKey;
            }
            set
            {
                if (value != _resetInputKey)
                {
                    _resetInputKey = value;
                    OnPropertyChangedWithValue(value, "ResetInputKey");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleTroopTypeVM> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (value != _items)
                {
                    _items = value;
                    OnPropertyChangedWithValue(value, "Items");
                }
            }
        }

        [DataSourceProperty]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChangedWithValue(value, "Title");
                }
            }
        }

        [DataSourceProperty]
        public string DoneLbl
        {
            get
            {
                return _doneLbl;
            }
            set
            {
                if (value != _doneLbl)
                {
                    _doneLbl = value;
                    OnPropertyChangedWithValue(value, "DoneLbl");
                }
            }
        }

        [DataSourceProperty]
        public string CancelLbl
        {
            get
            {
                return _cancelLbl;
            }
            set
            {
                if (value != _cancelLbl)
                {
                    _cancelLbl = value;
                    OnPropertyChangedWithValue(value, "CancelLbl");
                }
            }
        }

        [DataSourceProperty]
        public string SelectAllLbl
        {
            get
            {
                return _selectAllLbl;
            }
            set
            {
                if (value != _selectAllLbl)
                {
                    _selectAllLbl = value;
                    OnPropertyChangedWithValue(value, "SelectAllLbl");
                }
            }
        }

        [DataSourceProperty]
        public string BackToDefaultLbl
        {
            get
            {
                return _backToDefaultLbl;
            }
            set
            {
                if (value != _backToDefaultLbl)
                {
                    _backToDefaultLbl = value;
                    OnPropertyChangedWithValue(value, "BackToDefaultLbl");
                }
            }
        }

        [DataSourceProperty]
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                if (value != _isOpen)
                {
                    _isOpen = value;
                    OnPropertyChangedWithValue(value, "IsOpen");
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            DoneLbl = GameTexts.FindText("str_done", null).ToString();
            CancelLbl = GameTexts.FindText("str_cancel", null).ToString();
            SelectAllLbl = GameTexts.FindText("str_custom_battle_select_all", null).ToString();
            BackToDefaultLbl = GameTexts.FindText("str_custom_battle_back_to_default", null).ToString();

            ItemList?.RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            DoneInputKey.OnFinalize();
            CancelInputKey.OnFinalize();
            ResetInputKey.OnFinalize();
        }

        public void OpenPopUp(string title)
        {
            ExecuteReset();

            if (ItemList != null && !ItemList.HasSearchFilter)
            {
                // We add the search filter but we don't have it show up in the UI
                EncyclopediaFilterGroup filterGroup = new EncyclopediaFilterGroup(new List<EncyclopediaFilterItem>()
                {
                    // Captures names and ids
                    new EncyclopediaFilterItem(new TextObject("Search"), (f) => ((BasicCharacterObject)f).Name.Value.ToLower().Contains(SearchText.ToLower()) || ((BasicCharacterObject)f).StringId.ToLower().Contains(SearchText.ToLower()))
                }, new TextObject("Other"));

                var _ = filterGroup.Filters.All(x => x.IsActive = true);

                // Atm, with ~1000 entries, there's no need for debouncing. Might need to revisit this later though.
                ItemList.Filters.Add(filterGroup);
                ItemList.HasSearchFilter = true;
            }

            IsOpen = true;
            RefreshValues();
        }

        public void OnItemSelectionToggled(CustomBattleTroopTypeVM item)
        {
            if (item == null) return;

            if (_selectedItemCount > 1 || !item.IsSelected)
            {
                item.IsSelected = !item.IsSelected;
                _selectedItemCount += item.IsSelected ? 1 : -1;
            }
        }

        public void ExecuteSelectAll()
        {
            Items.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.IsSelected = true;
            });
            _selectedItemCount = Items.Count;
        }

        public void ExecuteReset()
        {
            if (ItemList != null)
            {
                BasicCharacterObject[] troopCharacters = new MBBindingList<CustomBattleTroopTypeVM>[] {
                    ItemList.MeleeTroopTypes, ItemList.RangedTroopTypes, ItemList.CavalryTroopTypes, ItemList.MountedArcherTroopTypes
                }.SelectMany(x => x.Aggregate(new List<BasicCharacterObject>(), (prev, curr) =>
                {
                    if (curr.IsSelected)
                        prev.Add(curr.Character);

                    return prev;
                })).ToArray();

                SetInitialTroops(troopCharacters);
                RefreshValues();
            }
        }

        // This could definitely do with some optimising. Noticeable freeze which will no doubt get worse, the more troops we add.
        public void SetInitialTroops(BasicCharacterObject[] troops)
        {
            if (ItemList != null)
            {
                foreach (SCTroopSelectionListItemVM itemVM in ItemList.Items)
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
            Action? onPopUpClosed = OnPopUpClosed;
            if (onPopUpClosed != null)
            {
                onPopUpClosed();
            }
            IsOpen = false;
        }

        public void ExecuteDone()
        {
            // Don't let them leave the popup if there's no troops selected. We should look at disabling the 'Done' button instead.
            if (ItemList?.SelectedItems.Count <= 0) return;

            IsOpen = false;

            // Do we handle the troop assignment in here or should we hand it off the ArmyCompItemVM?

            if (ItemList != null)
            {
                // There are caching opportunities here. Don't ignore yourself
                ItemList.MeleeTroopTypes.Clear();
                ItemList.RangedTroopTypes.Clear();
                ItemList.CavalryTroopTypes.Clear();
                ItemList.MountedArcherTroopTypes.Clear();

                MBReadOnlyList<SkillObject> allSkills = Game.Current.ObjectManager.GetObjectTypeList<SkillObject>();

                foreach (SCTroopSelectionListItemVM item in ItemList.SelectedItems)
                {
                    BasicCharacterObject? character = item.Object as BasicCharacterObject;

                    if (character != null)
                    {
                        switch(character.GetFormationClass())
                        {
                            case FormationClass.HorseArcher:
                                CustomBattleTroopTypeVM troopType = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.RangedCavalry), allSkills, false);
                                troopType.IsSelected = true;
                                ItemList.MountedArcherTroopTypes.Add(troopType);
                                break;
                            case FormationClass.Cavalry:
                                CustomBattleTroopTypeVM troopType2 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.MeleeCavalry), allSkills, false);
                                troopType2.IsSelected = true;
                                ItemList.CavalryTroopTypes.Add(troopType2);
                                break;
                            case FormationClass.Ranged:
                                CustomBattleTroopTypeVM troopType3 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.RangedInfantry), allSkills, false);
                                troopType3.IsSelected = true;
                                ItemList.RangedTroopTypes.Add(troopType3);
                                break;
                            case FormationClass.Infantry:
                                CustomBattleTroopTypeVM troopType4 = new CustomBattleTroopTypeVM(character, new Action<CustomBattleTroopTypeVM>(OnItemSelectionToggled),
                                    SCArmyCompositionItemVM.GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType.MeleeInfantry), allSkills, false);
                                troopType4.IsSelected = true;
                                ItemList.MeleeTroopTypes.Add(troopType4);
                                break;
                            default:
                                return;
                        }
                    }
                }

                OnDone?.Invoke();
            }
        }

        public void SetCancelInputKey(HotKey hotkey)
        {
            CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetDoneInputKey(HotKey hotkey)
        {
            DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetResetInputKey(HotKey hotkey)
        {
            ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }
    }
}
