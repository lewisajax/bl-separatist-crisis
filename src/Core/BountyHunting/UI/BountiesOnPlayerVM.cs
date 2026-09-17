using System;
using System.Linq;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SeparatistCrisis.BountyHunting.UI
{
    public class BountiesOnPlayerVM : ViewModel
    {
        private readonly BountyHunterBehavior _behavior;
        private readonly Action _closeScreen;

        private MBBindingList<BountyOnPlayerItemVM> _bounties;
        private string _titleText;

        public BountiesOnPlayerVM(BountyHunterBehavior behavior, Action closeScreen)
        {
            _behavior = behavior;
            _closeScreen = closeScreen;
            _bounties = new MBBindingList<BountyOnPlayerItemVM>();
            TitleText = "Bounties on you";

            RefreshList();
        }

        public void Tick(float dt) { }

        private void RefreshList()
        {
            Bounties.Clear();

            foreach (var bounty in _behavior.GetActiveBountiesOnPlayer())
            {
                Bounties.Add(new BountyOnPlayerItemVM(bounty));
            }
        }

        public void ExecuteClose()
        {
            try { _closeScreen?.Invoke(); }
            catch (Exception ex) { BountyLogger.Log($"[BountiesOnPlayerVM] ExecuteClose THREW: {ex}"); }
        }

        [DataSourceProperty]
        public string TitleText
        {
            get => _titleText;
            set { if (value != _titleText) { _titleText = value; OnPropertyChangedWithValue(value, nameof(TitleText)); } }
        }

        [DataSourceProperty]
        public MBBindingList<BountyOnPlayerItemVM> Bounties
        {
            get => _bounties;
            set { if (value != _bounties) { _bounties = value; OnPropertyChangedWithValue(value, nameof(Bounties)); } }
        }
    }
}