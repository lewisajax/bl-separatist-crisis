using SeparatistCrisis.CustomBattle;
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

namespace SeparatistCrisis.ViewModels
{
    public class SCTroopSelectionListSortControllerVM : ViewModel
    {
        private TextObject _sortedValueLabel = TextObject.Empty;

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
                return this._items;
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionListSelectorVM SortSelection
        {
            get
            {
                return this._sortSelection;
            }
            set
            {
                if (value != this._sortSelection)
                {
                    this._sortSelection = value;
                    base.OnPropertyChangedWithValue<SCTroopSelectionListSelectorVM>(value, "SortSelection");
                }
            }
        }

        [DataSourceProperty]
        public string NameLabel
        {
            get
            {
                return this._nameLabel;
            }
            set
            {
                if (value != this._nameLabel)
                {
                    this._nameLabel = value;
                    base.OnPropertyChangedWithValue<string>(value, "NameLabel");
                }
            }
        }

        [DataSourceProperty]
        public string SortedValueLabelText
        {
            get
            {
                return this._sortedValueLabelText;
            }
            set
            {
                if (value != this._sortedValueLabelText)
                {
                    this._sortedValueLabelText = value;
                    base.OnPropertyChangedWithValue<string>(value, "SortedValueLabelText");
                }
            }
        }

        [DataSourceProperty]
        public string SortByLabel
        {
            get
            {
                return this._sortByLabel;
            }
            set
            {
                if (value != this._sortByLabel)
                {
                    this._sortByLabel = value;
                    base.OnPropertyChangedWithValue<string>(value, "SortByLabel");
                }
            }
        }

        [DataSourceProperty]
        public int AlternativeSortState
        {
            get
            {
                return this._alternativeSortState;
            }
            set
            {
                if (value != this._alternativeSortState)
                {
                    this._alternativeSortState = value;
                    base.OnPropertyChangedWithValue(value, "AlternativeSortState");
                }
            }
        }

        [DataSourceProperty]
        public bool IsAlternativeSortVisible
        {
            get
            {
                return this._isAlternativeSortVisible;
            }
            set
            {
                if (value != this._isAlternativeSortVisible)
                {
                    this._isAlternativeSortVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsAlternativeSortVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool IsHighlightEnabled
        {
            get
            {
                return this._isHighlightEnabled;
            }
            set
            {
                if (value != this._isHighlightEnabled)
                {
                    this._isHighlightEnabled = value;
                    base.OnPropertyChangedWithValue(value, "IsHighlightEnabled");
                }
            }
        }

        public SCTroopSelectionListSortControllerVM(MBBindingList<SCTroopSelectionListItemVM> items)
        {
            this._items = items;
            this.UpdateSortItems();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.NameLabel = GameTexts.FindText("str_sort_by_name_label", null).ToString();
            this.SortByLabel = GameTexts.FindText("str_sort_by_label", null).ToString();
            this.SortedValueLabelText = this._sortedValueLabel.ToString();
        }

        public void SetSortSelection(int index)
        {
            this.SortSelection.SelectedIndex = index;
            this.OnSortSelectionChanged(this.SortSelection);
        }

        private void UpdateSortItems()
        {
            List<EncyclopediaSortController> sortControllers = new List<EncyclopediaSortController>
            {
                new EncyclopediaSortController(new TextObject("{=koX9okuG}None", null), new TroopSelectionListItemNameComparer())
            };

            this.SortSelection = new SCTroopSelectionListSelectorVM(0, new Action<SelectorVM<SCTroopSelectionListSelectorItemVM>>(this.OnSortSelectionChanged), new Action(this.OnSortSelectionActivated));
            foreach (EncyclopediaSortController sortController in sortControllers)
            {
                TroopSelectionListItemComparer comparer = new TroopSelectionListItemComparer(sortController);
                this.SortSelection.AddItem(new SCTroopSelectionListSelectorItemVM(comparer));
            }
        }

        private void UpdateAlternativeSortState(EncyclopediaListItemComparerBase comparer)
        {
            CampaignUIHelper.SortState alternativeSortState = comparer.IsAscending ? CampaignUIHelper.SortState.Ascending : CampaignUIHelper.SortState.Descending;
            this.AlternativeSortState = (int)alternativeSortState;
        }

        private void OnSortSelectionChanged(SelectorVM<SCTroopSelectionListSelectorItemVM> s)
        {
            TroopSelectionListItemComparer? comparer = s.SelectedItem.Comparer;
            if (comparer != null)
            {
                comparer.SortController.Comparer.SetDefaultSortOrder();
                this._items.Sort(comparer);
                this._items.ApplyActionOnAllItems(delegate (SCTroopSelectionListItemVM x)
                {
                    x.SetComparedValue(comparer.SortController.Comparer);
                });
                this._sortedValueLabel = comparer.SortController.Name;
                this.SortedValueLabelText = this._sortedValueLabel.ToString();
                this.IsAlternativeSortVisible = (this.SortSelection.SelectedIndex != 0);
                this.UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public void ExecuteSwitchSortOrder()
        {
            TroopSelectionListItemComparer? comparer = this.SortSelection.SelectedItem.Comparer;
            if (comparer != null)
            {
                comparer.SortController.Comparer.SwitchSortOrder();
                this._items.Sort(comparer);
                this.UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public void SetSortOrder(bool isAscending)
        {
            TroopSelectionListItemComparer? comparer = this.SortSelection.SelectedItem.Comparer;
            if (comparer != null && comparer.SortController.Comparer.IsAscending != isAscending)
            {
                comparer.SortController.Comparer.SetSortOrder(isAscending);
                this.Items.Sort(comparer);
                this.UpdateAlternativeSortState(comparer.SortController.Comparer);
            }
        }

        public bool GetSortOrder()
        {
            return this.SortSelection.SelectedItem.Comparer?.SortController.Comparer.IsAscending ?? false;
        }

        private void OnSortSelectionActivated()
        {
            Game.Current.EventManager.TriggerEvent<OnEncyclopediaListSortedEvent>(new OnEncyclopediaListSortedEvent());
        }
    }
}
