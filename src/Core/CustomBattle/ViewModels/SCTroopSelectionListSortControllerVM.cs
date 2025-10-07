using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SeparatistCrisis.CustomBattle
{
    public class SCTroopSelectionListSortControllerVM : ViewModel
    {
        private TextObject _sortedValueLabel = TextObject.GetEmpty();

        private MBBindingList<SCTroopSelectionListItemVM> _items;

        private SCTroopSelectionListSelectorVM _sortSelection = null!;

        private string _nameLabel = null!;

        private string _sortedValueLabelText = null!;

        private string _sortByLabel = null!;

        private int _alternativeSortState;

        private bool _isAlternativeSortVisible;

        private bool _isHighlightEnabled;

        public MBBindingList<SCTroopSelectionListItemVM> Items
        {
            get
            {
                return _items;
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionListSelectorVM SortSelection
        {
            get
            {
                return _sortSelection;
            }
            set
            {
                if (value != _sortSelection)
                {
                    _sortSelection = value;
                    OnPropertyChangedWithValue(value, "SortSelection");
                }
            }
        }

        [DataSourceProperty]
        public string NameLabel
        {
            get
            {
                return _nameLabel;
            }
            set
            {
                if (value != _nameLabel)
                {
                    _nameLabel = value;
                    OnPropertyChangedWithValue(value, "NameLabel");
                }
            }
        }

        [DataSourceProperty]
        public string SortedValueLabelText
        {
            get
            {
                return _sortedValueLabelText;
            }
            set
            {
                if (value != _sortedValueLabelText)
                {
                    _sortedValueLabelText = value;
                    OnPropertyChangedWithValue(value, "SortedValueLabelText");
                }
            }
        }

        [DataSourceProperty]
        public string SortByLabel
        {
            get
            {
                return _sortByLabel;
            }
            set
            {
                if (value != _sortByLabel)
                {
                    _sortByLabel = value;
                    OnPropertyChangedWithValue(value, "SortByLabel");
                }
            }
        }

        [DataSourceProperty]
        public int AlternativeSortState
        {
            get
            {
                return _alternativeSortState;
            }
            set
            {
                if (value != _alternativeSortState)
                {
                    _alternativeSortState = value;
                    OnPropertyChangedWithValue(value, "AlternativeSortState");
                }
            }
        }

        [DataSourceProperty]
        public bool IsAlternativeSortVisible
        {
            get
            {
                return _isAlternativeSortVisible;
            }
            set
            {
                if (value != _isAlternativeSortVisible)
                {
                    _isAlternativeSortVisible = value;
                    OnPropertyChangedWithValue(value, "IsAlternativeSortVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool IsHighlightEnabled
        {
            get
            {
                return _isHighlightEnabled;
            }
            set
            {
                if (value != _isHighlightEnabled)
                {
                    _isHighlightEnabled = value;
                    OnPropertyChangedWithValue(value, "IsHighlightEnabled");
                }
            }
        }

        public SCTroopSelectionListSortControllerVM(MBBindingList<SCTroopSelectionListItemVM> items)
        {
            _items = items;
            UpdateSortItems();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            NameLabel = GameTexts.FindText("str_sort_by_name_label", null).ToString();
            SortByLabel = GameTexts.FindText("str_sort_by_label", null).ToString();
            SortedValueLabelText = _sortedValueLabel.ToString();
        }

        public void SetSortSelection(int index)
        {
            SortSelection.SelectedIndex = index;
            OnSortSelectionChanged(SortSelection);
        }

        private void UpdateSortItems()
        {
            List<EncyclopediaSortController> sortControllers = new List<EncyclopediaSortController>
            {
                new EncyclopediaSortController(new TextObject("{=koX9okuG}None", null), new TroopSelectionListItemNameComparer())
            };

            SortSelection = new SCTroopSelectionListSelectorVM(0, new Action<SelectorVM<SCTroopSelectionListSelectorItemVM>>(OnSortSelectionChanged), new Action(OnSortSelectionActivated));
            foreach (EncyclopediaSortController sortController in sortControllers)
            {
                TroopSelectionListItemComparer comparer = new TroopSelectionListItemComparer(sortController);
                SortSelection.AddItem(new SCTroopSelectionListSelectorItemVM(comparer));
            }
        }

        private void UpdateAlternativeSortState(EncyclopediaListItemComparerBase comparer)
        {
            CampaignUIHelper.SortState alternativeSortState = comparer.IsAscending ? CampaignUIHelper.SortState.Ascending : CampaignUIHelper.SortState.Descending;
            AlternativeSortState = (int)alternativeSortState;
        }

        private void OnSortSelectionChanged(SelectorVM<SCTroopSelectionListSelectorItemVM> s)
        {
            TroopSelectionListItemComparer? comparer = s.SelectedItem.Comparer;
            if (comparer != null)
            {
                comparer.SortController.Comparer.SetDefaultSortOrder();
                _items.Sort(comparer);
                _items.ApplyActionOnAllItems(delegate (SCTroopSelectionListItemVM x)
                {
                    x.SetComparedValue(comparer.SortController.Comparer);
                });
                _sortedValueLabel = comparer.SortController.Name;
                SortedValueLabelText = _sortedValueLabel.ToString();
                IsAlternativeSortVisible = SortSelection.SelectedIndex != 0;
                UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public void ExecuteSwitchSortOrder()
        {
            TroopSelectionListItemComparer? comparer = SortSelection.SelectedItem.Comparer;
            if (comparer != null)
            {
                comparer.SortController.Comparer.SwitchSortOrder();
                _items.Sort(comparer);
                UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public void SetSortOrder(bool isAscending)
        {
            TroopSelectionListItemComparer? comparer = SortSelection.SelectedItem.Comparer;
            if (comparer != null && comparer.SortController.Comparer.IsAscending != isAscending)
            {
                comparer.SortController.Comparer.SetSortOrder(isAscending);
                Items.Sort(comparer);
                UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public bool GetSortOrder()
        {
            return SortSelection.SelectedItem.Comparer?.SortController.Comparer.IsAscending ?? false;
        }

        private void OnSortSelectionActivated()
        {
            Game.Current.EventManager.TriggerEvent(new OnEncyclopediaListSortedEvent());
        }
    }
}
