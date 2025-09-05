using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle;

namespace SeparatistCrisis.CustomBattle
{
    public class SCTroopSelectionItemListVM: ViewModel
    {
        public static TextObject FilterTypeSoldier { get; } = new TextObject("Soldier");
        public static TextObject FilterTypeBandit { get; } = new TextObject("Bandit");
        public static TextObject FilterTypeCivilian { get; } = new TextObject("Civilian");
        public static TextObject FilterTypeHero { get; } = new TextObject("Hero");
        public static TextObject FilterClassInfantry { get; } = new TextObject("Infantry");
        public static TextObject FilterClassRanged { get; } = new TextObject("Ranged");
        public static TextObject FilterClassCavalary { get; } = new TextObject("Cavalry");
        public static TextObject FilterClassMountedArcher { get; } = new TextObject("Mounted Archer");

        private MBBindingList<EncyclopediaFilterGroupVM> _filterGroupVMs;

        private MBBindingList<SCTroopSelectionListItemVM> _itemVMs;

        private SCTroopSelectionListSortControllerVM _sortControllerVM;

        private bool _isInitializationOver;

        private bool _isFilterHighlightEnabled;

        private string _emptyListText = "";

        private string _lastSelectedItemId = "";

        private List<EncyclopediaFilterGroup> _filters;

        private IEnumerable<EncyclopediaListItem> _listItems;

        public MBBindingList<CustomBattleTroopTypeVM> MeleeTroopTypes { get; set; } = null!;
        public MBBindingList<CustomBattleTroopTypeVM> RangedTroopTypes { get; set; } = null!;
        public MBBindingList<CustomBattleTroopTypeVM> CavalryTroopTypes { get; set; } = null!;
        public MBBindingList<CustomBattleTroopTypeVM> MountedArcherTroopTypes { get; set; } = null!;

        public bool HasSearchFilter { get; set; }

        public List<EncyclopediaFilterGroup> Filters { get { return _filters; } }

        public List<SCTroopSelectionListItemVM> SelectedItems { get; } = new List<SCTroopSelectionListItemVM>();

        [DataSourceProperty]
        public string EmptyListText
        {
            get
            {
                return _emptyListText;
            }
            set
            {
                if (value != _emptyListText)
                {
                    _emptyListText = value;
                    OnPropertyChangedWithValue(value, "EmptyListText");
                }
            }
        }

        [DataSourceProperty]
        public string LastSelectedItemId
        {
            get
            {
                return _lastSelectedItemId;
            }
            set
            {
                if (value != _lastSelectedItemId)
                {
                    _lastSelectedItemId = value;
                    OnPropertyChangedWithValue(value, "LastSelectedItemId");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<SCTroopSelectionListItemVM> Items
        {
            get
            {
                return _itemVMs;
            }
            set
            {
                if (value != _itemVMs)
                {
                    _itemVMs = value;
                    OnPropertyChangedWithValue(value, "Items");
                }
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionListSortControllerVM SortController
        {
            get
            {
                return _sortControllerVM;
            }
            set
            {
                if (value != _sortControllerVM)
                {
                    _sortControllerVM = value;
                    OnPropertyChangedWithValue(value, "SortController");
                }
            }
        }

        [DataSourceProperty]
        public bool IsInitializationOver
        {
            get
            {
                return _isInitializationOver;
            }
            set
            {
                if (value != _isInitializationOver)
                {
                    _isInitializationOver = value;
                    OnPropertyChangedWithValue(value, "IsInitializationOver");
                }
            }
        }

        [DataSourceProperty]
        public bool IsFilterHighlightEnabled
        {
            get
            {
                return _isFilterHighlightEnabled;
            }
            set
            {
                if (value != _isFilterHighlightEnabled)
                {
                    _isFilterHighlightEnabled = value;
                    OnPropertyChangedWithValue(value, "IsFilterHighlightEnabled");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFilterGroupVM> FilterGroups
        {
            get
            {
                return _filterGroupVMs;
            }
            set
            {
                if (value != _filterGroupVMs)
                {
                    _filterGroupVMs = value;
                    OnPropertyChangedWithValue(value, "FilterGroups");
                }
            }
        }

        public SCTroopSelectionItemListVM()
        {
            _itemVMs = new MBBindingList<SCTroopSelectionListItemVM>();
            _filterGroupVMs = new MBBindingList<EncyclopediaFilterGroupVM>();
            _sortControllerVM = new SCTroopSelectionListSortControllerVM(Items);

            IsInitializationOver = true;

            _filters = InitializeFilterItems();
            _listItems = InitializeListItems();

            foreach (EncyclopediaFilterGroup filterGroup in _filters)
            {
                _filterGroupVMs.Add(new EncyclopediaFilterGroupVM(filterGroup, new Action<EncyclopediaListFilterVM>(UpdateFilters)));
            }

            HasSearchFilter = false;

            IsInitializationOver = false;

            _itemVMs.Clear();
            foreach (EncyclopediaListItem listItem in _listItems)
            {
                SCTroopSelectionListItemVM encyclopediaListItemVM = new SCTroopSelectionListItemVM(listItem, new Action<SCTroopSelectionListItemVM, bool>(OnSelected));
                encyclopediaListItemVM.IsFiltered = IsFiltered(encyclopediaListItemVM.Object, _filters);
                _itemVMs.Add(encyclopediaListItemVM);
            }

            RefreshValues();
            IsInitializationOver = true;
        }

        public void OnSelected(SCTroopSelectionListItemVM item, bool isBookmarked)
        {
            // I'm using the vanilla bookmarked prop for when we select troops
            if (isBookmarked)
                SelectedItems.Add(item);
            else
                SelectedItems.Remove(item);
        }

        public bool IsFiltered(object o, IEnumerable<EncyclopediaFilterGroup> filterGroups)
        {
            if (filterGroups != null)
            {
                using (IEnumerator<EncyclopediaFilterGroup> enumerator = filterGroups.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Predicate(o))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public List<EncyclopediaFilterGroup> InitializeFilterItems()
        {
            List<EncyclopediaFilterGroup> filterGroups = new List<EncyclopediaFilterGroup>();

            List<EncyclopediaFilterItem> types = new List<EncyclopediaFilterItem>();
            types.Add(new EncyclopediaFilterItem(FilterTypeSoldier, (f) => ((BasicCharacterObject)f).IsSoldier));
            types.Add(new EncyclopediaFilterItem(FilterTypeBandit, (f) => ((BasicCharacterObject)f).Culture.IsBandit));
            types.Add(new EncyclopediaFilterItem(FilterTypeCivilian, (f) => ((BasicCharacterObject)f).IsInfantry));
            types.Add(new EncyclopediaFilterItem(FilterTypeHero, (f) => ((BasicCharacterObject)f).IsHero));
            // types.Add(new EncyclopediaFilterItem(new TextObject("Template"), (object f) => ((BasicCharacterObject)f).)); // BasicCharacterObject doesn't deserialise the IsTemplate property
            filterGroups.Add(new EncyclopediaFilterGroup(types, new TextObject("Types")));

            List<EncyclopediaFilterItem> classes = new List<EncyclopediaFilterItem>();
            classes.Add(new EncyclopediaFilterItem(FilterClassInfantry, (f) => !((BasicCharacterObject)f).IsRanged && !((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(FilterClassRanged, (f) => ((BasicCharacterObject)f).IsRanged && !((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(FilterClassCavalary, (f) => !((BasicCharacterObject)f).IsRanged && ((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(FilterClassMountedArcher, (f) => ((BasicCharacterObject)f).IsRanged && ((BasicCharacterObject)f).IsMounted));
            filterGroups.Add(new EncyclopediaFilterGroup(classes, new TextObject("Classes")));

            List<EncyclopediaFilterItem> cultures = new List<EncyclopediaFilterItem>();
            using (List<BasicCultureObject>.Enumerator enumerator = (from x in Game.Current.ObjectManager.GetObjectTypeList<BasicCultureObject>()
                                                                orderby !x.IsMainCulture descending
                                                                select x).ThenBy((f) => f.Name.ToString()).ToList().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BasicCultureObject culture = enumerator.Current;
                    if (culture.StringId != "neutral_culture")
                    {
                        cultures.Add(new EncyclopediaFilterItem(culture.Name, (c) => ((BasicCharacterObject)c).Culture == culture));
                    }
                }
            }
            filterGroups.Add(new EncyclopediaFilterGroup(cultures, GameTexts.FindText("str_culture", null)));

            return filterGroups;
        }

        public IEnumerable<EncyclopediaListItem> InitializeListItems()
        {
            // To get bandits and other characters/templates, we'd need to copy them into our module and xslt the native stuff out. The SandBox submodule.xml only loads those xml files on Campaign and StoryMode
            using (List<BasicCharacterObject>.Enumerator enumerator = Game.Current.ObjectManager.GetObjectTypeList<BasicCharacterObject>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BasicCharacterObject character = enumerator.Current;
                    if (IsValidEncyclopediaItem(character))
                    {
                        yield return new EncyclopediaListItem(character, character.Name.ToString(), "", character.StringId, "", true, delegate ()
                        {
                            InformationManager.ShowTooltip(typeof(BasicCharacterObject), new object[]
                            {
                                character
                            });
                        });
                    }
                }
            }

            yield break;
        }

        public bool IsValidEncyclopediaItem(BasicCharacterObject? characterObject)
        {
            return characterObject != null;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            SortController.RefreshValues();
            EmptyListText = GameTexts.FindText("str_encyclopedia_empty_list_error", null).ToString();
            Items.ApplyActionOnAllItems(delegate (SCTroopSelectionListItemVM x)
            {
                x.RefreshValues();
            });
            FilterGroups.ApplyActionOnAllItems(delegate (EncyclopediaFilterGroupVM x)
            {
                x.RefreshValues();
            });
        }

        private void ExecuteResetFilters()
        {
            foreach (EncyclopediaFilterGroupVM encyclopediaFilterGroupVM in FilterGroups)
            {
                foreach (EncyclopediaListFilterVM encyclopediaListFilterVM in encyclopediaFilterGroupVM.Filters)
                {
                    encyclopediaListFilterVM.IsSelected = false;
                }
            }
        }

        public void CopyFiltersFrom(Dictionary<EncyclopediaFilterItem, bool> filters)
        {
            FilterGroups.ApplyActionOnAllItems(delegate (EncyclopediaFilterGroupVM x)
            {
                x.CopyFiltersFrom(filters);
            });
        }

        public void Refresh()
        {
        }

        // Goes through and disables all the filters except for the ones inside filterNames
        public void ReCheckFilters(IEnumerable<TextObject> filterNames)
        {
            foreach (EncyclopediaFilterGroupVM filterGroup in FilterGroups)
            {
                // 'Other' filters are not shown in the UI. We want them to always be active.
                if (filterGroup.FilterGroup.Name.Value == "Other") continue;

                foreach (EncyclopediaListFilterVM listFilter in filterGroup.Filters)
                    listFilter.IsSelected = filterNames.Contains(listFilter.Filter.Name);
            }
            
        }

        public void UpdateFilters(EncyclopediaListFilterVM filterVM = null)
        {
            IsInitializationOver = false;
            foreach (SCTroopSelectionListItemVM encyclopediaListItemVM in Items)
            {
                encyclopediaListItemVM.IsFiltered = IsFiltered(encyclopediaListItemVM.Object, _filters);
            }
            IsInitializationOver = true;
        }
    }
}
