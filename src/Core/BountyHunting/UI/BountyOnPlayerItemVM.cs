using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// Row view model for one bounty placed on the player: faction name and reward.
    /// </summary>
    public class BountyOnPlayerItemVM : ViewModel
    {
        private readonly BountyTarget _bounty;

        private string _factionNameText;
        private string _rewardText;

        public BountyOnPlayerItemVM(BountyTarget bounty)
        {
            _bounty = bounty;
            RefreshValues();
        }

        public void RefreshValues()
        {
            FactionNameText = ResolveFactionDisplayName(_bounty.FactionId);
            RewardText = $"{_bounty.BountyValue} gold";
        }

        private static string ResolveFactionDisplayName(string factionId)
        {
            if (string.IsNullOrEmpty(factionId)) return "Unknown";

            var kingdom = TaleWorlds.CampaignSystem.Kingdom.All.FirstOrDefault(k => k.StringId == factionId);
            if (kingdom != null) return kingdom.Name.ToString();

            var clan = TaleWorlds.CampaignSystem.Clan.All.FirstOrDefault(c => c.StringId == factionId);
            return clan != null ? clan.Name.ToString() : factionId;
        }

        [DataSourceProperty]
        public string FactionNameText
        {
            get => _factionNameText;
            set { if (value != _factionNameText) { _factionNameText = value; OnPropertyChangedWithValue(value, nameof(FactionNameText)); } }
        }

        [DataSourceProperty]
        public string RewardText
        {
            get => _rewardText;
            set { if (value != _rewardText) { _rewardText = value; OnPropertyChangedWithValue(value, nameof(RewardText)); } }
        }
    }
}