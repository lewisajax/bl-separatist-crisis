using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SeparatistCrisis.ViewModels
{
    public class SCTroopSelectionListItemVM: ViewModel
    {
        private readonly string _type;

        private readonly Action _onShowTooltip;

        private string _id;

        private string _name = null!;

        private string _comparedValue = null!;

        private bool _isFiltered;

        private bool _isBookmarked;

        private bool _playerCanSeeValues;

        private bool _hideAvailable;

        private bool _hideSelected;

        public object Object { get; private set; }

        public EncyclopediaListItem ListItem { get; }

        [DataSourceProperty]
        public bool IsFiltered
        {
            get
            {
                return this._isFiltered;
            }
            set
            {
                if (value != this._isFiltered)
                {
                    this._isFiltered = value;
                    base.OnPropertyChangedWithValue(value, "IsFiltered");
                    this.RefreshValues();
                }
            }
        }

        [DataSourceProperty]
        public bool PlayerCanSeeValues
        {
            get
            {
                return this._playerCanSeeValues;
            }
            set
            {
                if (value != this._playerCanSeeValues)
                {
                    this._playerCanSeeValues = value;
                    base.OnPropertyChangedWithValue(value, "PlayerCanSeeValues");
                }
            }
        }

        [DataSourceProperty]
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (value != this._id)
                {
                    this._id = value;
                    base.OnPropertyChangedWithValue<string>(value, "Id");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue<string>(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string ComparedValue
        {
            get
            {
                return this._comparedValue;
            }
            set
            {
                if (value != this._comparedValue)
                {
                    this._comparedValue = value;
                    base.OnPropertyChangedWithValue<string>(value, "ComparedValue");
                }
            }
        }

        [DataSourceProperty]
        public bool IsBookmarked
        {
            get
            {
                return this._isBookmarked;
            }
            set
            {
                if (value != this._isBookmarked)
                {
                    this._isBookmarked = value;
                    base.OnPropertyChangedWithValue(value, "IsBookmarked");
                }
            }
        }

        /*
         * If it's in available. Completely hides bookmarks regardless of filters
         * If it's in selected. Only show bookmarks but hide any filtered bookmarks
         */

        [DataSourceProperty]
        public bool HideAvailable
        {
            get
            {
                return this._hideAvailable;
            }
            set
            {
                if (value != this._hideAvailable)
                {
                    this._hideAvailable = value;
                    base.OnPropertyChangedWithValue(value, "HideAvailable");
                }
            }
        }

        [DataSourceProperty]
        public bool HideSelected
        {
            get
            {
                return this._hideSelected;
            }
            set
            {
                if (value != this._hideSelected)
                {
                    this._hideSelected = value;
                    base.OnPropertyChangedWithValue(value, "HideSelected");
                }
            }
        }

        public SCTroopSelectionListItemVM(EncyclopediaListItem listItem)
        {
            this.Object = listItem.Object;
            this._id = listItem.Id;
            this._type = listItem.TypeName;
            this.ListItem = listItem;
            this.PlayerCanSeeValues = listItem.PlayerCanSeeValues;
            this._onShowTooltip = listItem.OnShowTooltip;
            this._isBookmarked = false;
            this._hideAvailable = false;
            this._hideSelected = true;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Name = this.ListItem.Name;

            this.HideAvailable = this.ShouldHideAvailable();
            this.HideSelected = this.ShouldHideSelected();
        }

        private bool ShouldHideSelected()
        {
            if (!this.IsBookmarked) return true;

            if (this.IsFiltered)
                return true;
            else
                return false;
        }

        private bool ShouldHideAvailable()
        {
            return this.IsBookmarked || this.IsFiltered;
        }

        public void Execute()
        {
            this.IsBookmarked = !this._isBookmarked;
            this.RefreshValues();
        }

        public void SetComparedValue(EncyclopediaListItemComparerBase comparer)
        {
            this.ComparedValue = comparer.GetComparedValueText(this.ListItem);
        }

        public void ExecuteBeginTooltip()
        {
            Action onShowTooltip = this._onShowTooltip;
            if (onShowTooltip == null)
            {
                return;
            }
            onShowTooltip();
        }

        public void ExecuteEndTooltip()
        {
            if (this._onShowTooltip != null)
            {
                MBInformationManager.HideInformations();
            }
        }
    }
}
