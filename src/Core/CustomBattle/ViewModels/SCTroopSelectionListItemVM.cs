using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SeparatistCrisis.CustomBattle
{
    public class SCTroopSelectionListItemVM: ViewModel
    {
        private readonly string _type;
        private readonly Action _onShowTooltip;
        private readonly Action<SCTroopSelectionListItemVM, bool> _onSelected;
        private string _id;
        private string _name = null!;
        private string _comparedValue = null!;
        private bool _isFiltered;
        private bool _isBookmarked;
        private bool _playerCanSeeValues;
        private bool _hideAvailable;
        private bool _hideSelected;

        // private ImageIdentifierVM _visual;

        public object Object { get; private set; }

        public EncyclopediaListItem ListItem { get; }

        // [DataSourceProperty]
        // public ImageIdentifierVM Visual
        // {
        //     get
        //     {
        //         return this._visual;
        //     }
        //     set
        //     {
        //         if (value != this._visual)
        //         {
        //             this._visual = value;
        //             base.OnPropertyChangedWithValue<ImageIdentifierVM>(value, "Visual");
        //         }
        //     }
        // }

        [DataSourceProperty]
        public bool IsFiltered
        {
            get
            {
                return _isFiltered;
            }
            set
            {
                if (value != _isFiltered)
                {
                    _isFiltered = value;
                    OnPropertyChangedWithValue(value, "IsFiltered");
                    RefreshValues(); // I didnt want to take control of even more classes so we're taking a shortcut with this.
                }
            }
        }

        [DataSourceProperty]
        public bool PlayerCanSeeValues
        {
            get
            {
                return _playerCanSeeValues;
            }
            set
            {
                if (value != _playerCanSeeValues)
                {
                    _playerCanSeeValues = value;
                    OnPropertyChangedWithValue(value, "PlayerCanSeeValues");
                }
            }
        }

        [DataSourceProperty]
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChangedWithValue(value, "Id");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string ComparedValue
        {
            get
            {
                return _comparedValue;
            }
            set
            {
                if (value != _comparedValue)
                {
                    _comparedValue = value;
                    OnPropertyChangedWithValue(value, "ComparedValue");
                }
            }
        }

        [DataSourceProperty]
        public bool IsBookmarked
        {
            get
            {
                return _isBookmarked;
            }
            set
            {
                if (value != _isBookmarked)
                {
                    _isBookmarked = value;
                    OnPropertyChangedWithValue(value, "IsBookmarked");
                    _onSelected(this, value);
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
                return _hideAvailable;
            }
            set
            {
                if (value != _hideAvailable)
                {
                    _hideAvailable = value;
                    OnPropertyChangedWithValue(value, "HideAvailable");
                }
            }
        }

        [DataSourceProperty]
        public bool HideSelected
        {
            get
            {
                return _hideSelected;
            }
            set
            {
                if (value != _hideSelected)
                {
                    _hideSelected = value;
                    OnPropertyChangedWithValue(value, "HideSelected");
                }
            }
        }

        public SCTroopSelectionListItemVM(EncyclopediaListItem listItem, Action<SCTroopSelectionListItemVM, bool> onSelected)
        {
            Object = listItem.Object;
            _id = listItem.Id;
            _type = listItem.TypeName;
            ListItem = listItem;

            // if (this.Object != null)
            // {
            //     this.Visual = new ImageIdentifierVM(CharacterCode.CreateFrom((BasicCharacterObject)this.Object));
            // }

            PlayerCanSeeValues = listItem.PlayerCanSeeValues;
            _onShowTooltip = listItem.OnShowTooltip;
            _isBookmarked = false;
            _hideAvailable = false;
            _hideSelected = true;
            _onSelected = onSelected;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = ListItem.Name;

            HideAvailable = ShouldHideAvailable();
            HideSelected = ShouldHideSelected();
        }

        private bool ShouldHideSelected()
        {
            if (!IsBookmarked) return true;

            if (IsFiltered)
                return true;
            else
                return false;
        }

        private bool ShouldHideAvailable()
        {
            return IsBookmarked || IsFiltered;
        }

        public void Execute()
        {
            IsBookmarked = !_isBookmarked;
            RefreshValues();
        }

        public void SetComparedValue(EncyclopediaListItemComparerBase comparer)
        {
            ComparedValue = comparer.GetComparedValueText(ListItem);
        }

        public void ExecuteBeginTooltip()
        {
            Action onShowTooltip = _onShowTooltip;
            if (onShowTooltip == null)
            {
                return;
            }
            onShowTooltip();
        }

        public void ExecuteEndTooltip()
        {
            if (_onShowTooltip != null)
            {
                MBInformationManager.HideInformations();
            }
        }
    }
}
