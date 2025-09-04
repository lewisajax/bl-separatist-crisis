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

namespace SeparatistCrisis.ViewModels
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
                return this._emptyListText;
            }
            set
            {
                if (value != this._emptyListText)
                {
                    this._emptyListText = value;
                    base.OnPropertyChangedWithValue<string>(value, "EmptyListText");
                }
            }
        }

        [DataSourceProperty]
        public string LastSelectedItemId
        {
            get
            {
                return this._lastSelectedItemId;
            }
            set
            {
                if (value != this._lastSelectedItemId)
                {
                    this._lastSelectedItemId = value;
                    base.OnPropertyChangedWithValue<string>(value, "LastSelectedItemId");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<SCTroopSelectionListItemVM> Items
        {
            get
            {
                return this._itemVMs;
            }
            set
            {
                if (value != this._itemVMs)
                {
                    this._itemVMs = value;
                    base.OnPropertyChangedWithValue<MBBindingList<SCTroopSelectionListItemVM>>(value, "Items");
                }
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionListSortControllerVM SortController
        {
            get
            {
                return this._sortControllerVM;
            }
            set
            {
                if (value != this._sortControllerVM)
                {
                    this._sortControllerVM = value;
                    base.OnPropertyChangedWithValue<SCTroopSelectionListSortControllerVM>(value, "SortController");
                }
            }
        }

        [DataSourceProperty]
        public bool IsInitializationOver
        {
            get
            {
                return this._isInitializationOver;
            }
            set
            {
                if (value != this._isInitializationOver)
                {
                    this._isInitializationOver = value;
                    base.OnPropertyChangedWithValue(value, "IsInitializationOver");
                }
            }
        }

        [DataSourceProperty]
        public bool IsFilterHighlightEnabled
        {
            get
            {
                return this._isFilterHighlightEnabled;
            }
            set
            {
                if (value != this._isFilterHighlightEnabled)
                {
                    this._isFilterHighlightEnabled = value;
                    base.OnPropertyChangedWithValue(value, "IsFilterHighlightEnabled");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFilterGroupVM> FilterGroups
        {
            get
            {
                return this._filterGroupVMs;
            }
            set
            {
                if (value != this._filterGroupVMs)
                {
                    this._filterGroupVMs = value;
                    base.OnPropertyChangedWithValue<MBBindingList<EncyclopediaFilterGroupVM>>(value, "FilterGroups");
                }
            }
        }

        public SCTroopSelectionItemListVM()
        {
            this._itemVMs = new MBBindingList<SCTroopSelectionListItemVM>();
            this._filterGroupVMs = new MBBindingList<EncyclopediaFilterGroupVM>();
            this._sortControllerVM = new SCTroopSelectionListSortControllerVM(this.Items);

            this.IsInitializationOver = true;

            this._filters = this.InitializeFilterItems();
            this._listItems = this.InitializeListItems();

            foreach (EncyclopediaFilterGroup filterGroup in this._filters)
            {
                this._filterGroupVMs.Add(new EncyclopediaFilterGroupVM(filterGroup, new Action<EncyclopediaListFilterVM>(this.UpdateFilters)));
            }

            this.HasSearchFilter = false;

            this.IsInitializationOver = false;

            this._itemVMs.Clear();
            foreach (EncyclopediaListItem listItem in this._listItems)
            {
                SCTroopSelectionListItemVM encyclopediaListItemVM = new SCTroopSelectionListItemVM(listItem, new Action<SCTroopSelectionListItemVM, bool>(this.OnSelected));
                encyclopediaListItemVM.IsFiltered = this.IsFiltered(encyclopediaListItemVM.Object, this._filters);
                this._itemVMs.Add(encyclopediaListItemVM);
            }

            this.RefreshValues();
            this.IsInitializationOver = true;
        }

        public void OnSelected(SCTroopSelectionListItemVM item, bool isBookmarked)
        {
            // I'm using the vanilla bookmarked prop for when we select troops
            if (isBookmarked)
                this.SelectedItems.Add(item);
            else
                this.SelectedItems.Remove(item);
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
            types.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterTypeSoldier, (object f) => ((BasicCharacterObject)f).IsSoldier));
            types.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterTypeBandit, (object f) => ((BasicCharacterObject)f).Culture.IsBandit));
            types.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterTypeCivilian, (object f) => ((BasicCharacterObject)f).IsInfantry));
            types.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterTypeHero, (object f) => ((BasicCharacterObject)f).IsHero));
            // types.Add(new EncyclopediaFilterItem(new TextObject("Template"), (object f) => ((BasicCharacterObject)f).)); // BasicCharacterObject doesn't deserialise the IsTemplate property
            filterGroups.Add(new EncyclopediaFilterGroup(types, new TextObject("Types")));

            List<EncyclopediaFilterItem> classes = new List<EncyclopediaFilterItem>();
            classes.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterClassInfantry, (object f) => (!((BasicCharacterObject)f).IsRanged) && !((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterClassRanged, (object f) => (((BasicCharacterObject)f).IsRanged) && !((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterClassCavalary, (object f) => (!((BasicCharacterObject)f).IsRanged) && ((BasicCharacterObject)f).IsMounted));
            classes.Add(new EncyclopediaFilterItem(SCTroopSelectionItemListVM.FilterClassMountedArcher, (object f) => (((BasicCharacterObject)f).IsRanged) && ((BasicCharacterObject)f).IsMounted));
            filterGroups.Add(new EncyclopediaFilterGroup(classes, new TextObject("Classes")));

            List<EncyclopediaFilterItem> cultures = new List<EncyclopediaFilterItem>();
            using (List<BasicCultureObject>.Enumerator enumerator = (from x in Game.Current.ObjectManager.GetObjectTypeList<BasicCultureObject>()
                                                                orderby !x.IsMainCulture descending
                                                                select x).ThenBy((BasicCultureObject f) => f.Name.ToString()).ToList<BasicCultureObject>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BasicCultureObject culture = enumerator.Current;
                    if (culture.StringId != "neutral_culture")
                    {
                        cultures.Add(new EncyclopediaFilterItem(culture.Name, (object c) => ((BasicCharacterObject)c).Culture == culture));
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
                    if (this.IsValidEncyclopediaItem(character))
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
            this.SortController.RefreshValues();
            this.EmptyListText = GameTexts.FindText("str_encyclopedia_empty_list_error", null).ToString();
            this.Items.ApplyActionOnAllItems(delegate (SCTroopSelectionListItemVM x)
            {
                x.RefreshValues();
            });
            this.FilterGroups.ApplyActionOnAllItems(delegate (EncyclopediaFilterGroupVM x)
            {
                x.RefreshValues();
            });
        }

        private void ExecuteResetFilters()
        {
            foreach (EncyclopediaFilterGroupVM encyclopediaFilterGroupVM in this.FilterGroups)
            {
                foreach (EncyclopediaListFilterVM encyclopediaListFilterVM in encyclopediaFilterGroupVM.Filters)
                {
                    encyclopediaListFilterVM.IsSelected = false;
                }
            }
        }

        public void CopyFiltersFrom(Dictionary<EncyclopediaFilterItem, bool> filters)
        {
            this.FilterGroups.ApplyActionOnAllItems(delegate (EncyclopediaFilterGroupVM x)
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
            foreach (EncyclopediaFilterGroupVM filterGroup in this.FilterGroups)
            {
                // 'Other' filters are not shown in the UI. We want them to always be active.
                if (filterGroup.FilterGroup.Name.Value == "Other") continue;

                foreach (EncyclopediaListFilterVM listFilter in filterGroup.Filters)
                    listFilter.IsSelected = filterNames.Contains(listFilter.Filter.Name);
            }
            
        }

        public void UpdateFilters(EncyclopediaListFilterVM filterVM = null)
        {
            this.IsInitializationOver = false;
            foreach (SCTroopSelectionListItemVM encyclopediaListItemVM in this.Items)
            {
                encyclopediaListItemVM.IsFiltered = this.IsFiltered(encyclopediaListItemVM.Object, this._filters);
            }
            this.IsInitializationOver = true;
        }
    }
}
