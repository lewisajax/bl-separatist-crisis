using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// View model for the bounty board screen: the faction-filtered bounty list, the
    /// selected bounty's detail panel, and the accept/turn-in action.
    /// </summary>
    public class BountyBoardVM : ViewModel
    {
        private const string FillerDescriptionText =
            "Wanted for banditry, extortion, and the murder of a local merchant caught " +
            "refusing to pay tribute. Last reported moving through the borderlands with " +
            "a small band of loyal followers, avoiding major roads and settlements. " +
            "Known to be armed and unwilling to surrender without a fight — approach " +
            "with caution. A bounty has been placed for their capture; delivering them " +
            "alive is preferred, but proof of death will still be honored.";

        private readonly BountyHunterBehavior _behavior;
        private readonly Action _closeScreen;

        private BountyItemVM _selectedItem;

        private MBBindingList<BountyItemVM> _bounties;
        private string _titleText;
        private ImageIdentifierVM _detailVisual;
        private HeroViewModel _heroCharacter;
        private string _detailDescriptionText;
        private string _detailRewardText;
        private string _detailButtonText;
        private bool _isDetailVisible;
        private bool _isDetailVisibleNegated;

        /// <summary>
        /// Creates the board view model for the given behavior and wires up the
        /// close-screen callback.
        /// </summary>
        public BountyBoardVM(BountyHunterBehavior behavior, Action closeScreen)
        {
            _behavior = behavior;
            _closeScreen = closeScreen;
            _bounties = new MBBindingList<BountyItemVM>();
            TitleText = "Bounty Board";
            IsDetailVisible = false;
            IsDetailVisibleNegated = true;

            HeroCharacter = new HeroViewModel(CharacterViewModel.StanceTypes.EmphasizeFace);

            RefreshList();
        }

        /// <summary>
        /// Per-frame tick, currently unused.
        /// </summary>
        public void Tick(float dt)
        {
        }

        /// <summary>
        /// Rebuilds the bounty list from the behavior's active and captured bounties,
        /// filtered to the current settlement's culture (plus any faction-less
        /// bounties, which show on every board). Culture-based rather than exact
        /// Kingdom-id matching, since SC splits what's narratively one side (e.g.
        /// "the Republic") across many distinct Kingdom objects sharing the same
        /// culture — an exact-id match would only ever show a bounty on the one
        /// specific Kingdom it happened to be generated against.
        /// </summary>
        private void RefreshList()
        {
            Bounties.Clear();

            string currentCultureId = Settlement.CurrentSettlement?.MapFaction?.Culture?.StringId;

            var all = _behavior.GetActiveBounties()
                .Concat(_behavior.GetCapturedAwaitingTurnIn())
                .ToList();

            BountyLogger.Log($"BountyBoardVM.RefreshList: currentSettlement='{Settlement.CurrentSettlement?.StringId ?? "NULL"}', currentCultureId='{currentCultureId ?? "NULL"}'. " +
                $"{all.Count} total bounty(ies) before faction filter: [{string.Join(", ", all.Select(b => $"{b.TargetHero?.Name}(FactionId='{b.FactionId ?? "NULL"}', cultureId='{GetCultureIdForFaction(b.FactionId) ?? "NULL"}')"))}].");

            var combined = all
                .Where(b => string.IsNullOrEmpty(b.FactionId) || GetCultureIdForFaction(b.FactionId) == currentCultureId)
                .ToList();

            BountyLogger.Log($"BountyBoardVM.RefreshList: {combined.Count}/{all.Count} bounty(ies) passed the faction filter.");

            foreach (var bounty in combined)
            {
                Bounties.Add(new BountyItemVM(bounty, SelectBounty));
            }
        }

        /// <summary>
        /// Resolves a Kingdom StringId to that Kingdom's own culture StringId, or
        /// null if the id doesn't resolve to a real Kingdom.
        /// </summary>
        private static string GetCultureIdForFaction(string factionId)
        {
            if (string.IsNullOrEmpty(factionId)) return null;
            return Kingdom.All.FirstOrDefault(k => k.StringId == factionId)?.Culture?.StringId;
        }

        /// <summary>
        /// Selects (or clears, if null) a bounty row, populating the detail panel and
        /// live hero portrait.
        /// </summary>
        public void SelectBounty(BountyItemVM item)
        {
            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = false;
            }

            _selectedItem = item;

            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = true;
            }

            if (item?.Bounty?.TargetHero != null)
            {
                var hero = item.Bounty.TargetHero;

                hero.IsKnownToPlayer = true;

                HeroCharacter.FillFrom(hero, -1, hero.IsNotable, true);
                HeroCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
                HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
                HeroCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));
            }

            DetailVisual = _selectedItem?.Visual;
            DetailDescriptionText = _selectedItem != null
                ? (!string.IsNullOrEmpty(_selectedItem.Bounty?.Description) ? _selectedItem.Bounty.Description : FillerDescriptionText)
                : string.Empty;
            DetailRewardText = _selectedItem?.BountyValueText ?? string.Empty;
            DetailButtonText = _selectedItem != null && _selectedItem.IsCaptured ? "Turn In" : "Accept Contract";
            IsDetailVisible = _selectedItem != null;
            IsDetailVisibleNegated = !IsDetailVisible;
        }

        /// <summary>
        /// Executes the detail panel's single action button: turns in the selected
        /// bounty if captured, otherwise accepts it as a tracked contract.
        /// </summary>
        public void ExecuteAcceptOrTurnIn()
        {
            if (_selectedItem == null)
            {
                return;
            }

            try
            {
                if (_selectedItem.Bounty.Status == BountyStatus.Captured)
                {
                    _behavior.TurnInBounty(_selectedItem.Bounty);
                }
                else
                {
                    _behavior.TrackBounty(_selectedItem.Bounty);
                }

                RefreshList();
                SelectBounty(null);
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"[BountyBoardVM] ExecuteAcceptOrTurnIn THREW: {ex}");
            }
        }

        /// <summary>
        /// Closes the bounty board screen.
        /// </summary>
        public void ExecuteClose()
        {
            try
            {
                _closeScreen?.Invoke();
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"[BountyBoardVM] ExecuteClose THREW: {ex}");
            }
        }

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set { if (value != _titleText) { _titleText = value; OnPropertyChangedWithValue(value, nameof(TitleText)); } }
        }

        [DataSourceProperty]
        public MBBindingList<BountyItemVM> Bounties
        {
            get => _bounties;
            set { if (value != _bounties) { _bounties = value; OnPropertyChangedWithValue(value, nameof(Bounties)); } }
        }

        [DataSourceProperty]
        public ImageIdentifierVM DetailVisual
        {
            get => _detailVisual;
            set { if (value != _detailVisual) { _detailVisual = value; OnPropertyChangedWithValue(value, nameof(DetailVisual)); } }
        }

        [DataSourceProperty]
        public HeroViewModel HeroCharacter
        {
            get => _heroCharacter;
            set { if (value != _heroCharacter) { _heroCharacter = value; OnPropertyChangedWithValue(value, nameof(HeroCharacter)); } }
        }

        /// <summary>
        /// Finalizes the live hero view model alongside the base view model.
        /// </summary>
        public override void OnFinalize()
        {
            base.OnFinalize();
            HeroCharacter?.OnFinalize();
        }

        [DataSourceProperty]
        public string DetailDescriptionText
        {
            get => _detailDescriptionText;
            set { if (value != _detailDescriptionText) { _detailDescriptionText = value; OnPropertyChangedWithValue(value, nameof(DetailDescriptionText)); } }
        }

        [DataSourceProperty]
        public string DetailRewardText
        {
            get => _detailRewardText;
            set { if (value != _detailRewardText) { _detailRewardText = value; OnPropertyChangedWithValue(value, nameof(DetailRewardText)); } }
        }

        [DataSourceProperty]
        public string DetailButtonText
        {
            get => _detailButtonText;
            set { if (value != _detailButtonText) { _detailButtonText = value; OnPropertyChangedWithValue(value, nameof(DetailButtonText)); } }
        }

        [DataSourceProperty]
        public bool IsDetailVisible
        {
            get => _isDetailVisible;
            set { if (value != _isDetailVisible) { _isDetailVisible = value; OnPropertyChangedWithValue(value, nameof(IsDetailVisible)); } }
        }

        [DataSourceProperty]
        public bool IsDetailVisibleNegated
        {
            get => _isDetailVisibleNegated;
            set { if (value != _isDetailVisibleNegated) { _isDetailVisibleNegated = value; OnPropertyChangedWithValue(value, nameof(IsDetailVisibleNegated)); } }
        }
    }
}