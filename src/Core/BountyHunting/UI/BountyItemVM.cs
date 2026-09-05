using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// View model for a single row in the bounty board's list — a selectable name
    /// button whose value/status/portrait are read by the detail panel once selected.
    /// </summary>
    public class BountyItemVM : ViewModel
    {
        private readonly BountyTarget _bounty;
        private readonly Action<BountyItemVM> _onSelected;

        private string _heroName;
        private string _bountyValueText;
        private string _statusText;
        private bool _isCaptured;
        private bool _isSelected;
        private ImageIdentifierVM _visual;

        /// <summary>
        /// Creates a row view model wrapping the given bounty.
        /// </summary>
        public BountyItemVM(BountyTarget bounty, Action<BountyItemVM> onSelected)
        {
            _bounty = bounty;
            _onSelected = onSelected;

            if (_bounty.TargetHero != null)
            {
                Visual = new CharacterImageIdentifierVM(CharacterCode.CreateFrom(_bounty.TargetHero.CharacterObject));
            }

            RefreshValues();
        }

        public BountyTarget Bounty => _bounty;

        /// <summary>
        /// Recomputes the display fields (name, value, status) from the underlying
        /// bounty.
        /// </summary>
        public void RefreshValues()
        {
            HeroName = _bounty.TargetHero?.Name?.ToString() ?? "Unknown";
            BountyValueText = $"{_bounty.BountyValue} gold";
            IsCaptured = _bounty.Status == BountyStatus.Captured;
            StatusText = IsCaptured
                ? "Captured — awaiting turn-in"
                : $"Expires in {_bounty.ExpiryDate.RemainingDaysFromNow:0} days";
        }

        [DataSourceProperty]
        public string HeroName
        {
            get => _heroName;
            set { if (value != _heroName) { _heroName = value; OnPropertyChangedWithValue(value, nameof(HeroName)); } }
        }

        [DataSourceProperty]
        public string BountyValueText
        {
            get => _bountyValueText;
            set { if (value != _bountyValueText) { _bountyValueText = value; OnPropertyChangedWithValue(value, nameof(BountyValueText)); } }
        }

        [DataSourceProperty]
        public string StatusText
        {
            get => _statusText;
            set { if (value != _statusText) { _statusText = value; OnPropertyChangedWithValue(value, nameof(StatusText)); } }
        }

        [DataSourceProperty]
        public bool IsCaptured
        {
            get => _isCaptured;
            set { if (value != _isCaptured) { _isCaptured = value; OnPropertyChangedWithValue(value, nameof(IsCaptured)); } }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set { if (value != _isSelected) { _isSelected = value; OnPropertyChangedWithValue(value, nameof(IsSelected)); } }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Visual
        {
            get => _visual;
            set { if (value != _visual) { _visual = value; OnPropertyChangedWithValue(value, nameof(Visual)); } }
        }

        /// <summary>
        /// Invoked when this row's name button is clicked.
        /// </summary>
        public void ExecuteSelection()
        {
            _onSelected?.Invoke(this);
        }
    }
}
