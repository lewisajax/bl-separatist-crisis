using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace SeparatistCrisis.BountyHunting
{
    public enum BountyStatus
    {
        Active,
        Captured,
        Killed,
        Expired
    }

    /// <summary>
    /// Represents a single bounty: its target hero, value, status, and the data
    /// needed by each bounty type (settlement anchor, gang party, associated
    /// quest, and owning faction). SaveableField indices 11, 15, 16, 17, 18, and
    /// 19 are retired (previously IsStealthBounty, IsTavernBounty, Phase,
    /// Phase1PartyIds, ThugCharacterIds, and GangCount) and must never be reused.
    /// </summary>
    public class BountyTarget
    {
        [SaveableField(1)]
        public Hero TargetHero;

        [SaveableField(2)]
        public int BountyValue;

        [SaveableField(3)]
        public BountyStatus Status;

        [SaveableField(4)]
        public int GuardPartySize;

        [SaveableField(5)]
        public CampaignTime ExpiryDate;

        [SaveableField(6)]
        public bool IsPlayerBounty;

        [SaveableField(7)]
        public bool IsTracked;

        [SaveableField(8)]
        public string TrackerId;

        [SaveableField(9)]
        public string TargetSettlementId;

        [SaveableField(10)]
        public string GangPartyId;

        [SaveableField(12)]
        public string Description;

        [SaveableField(13)]
        public bool IsHideoutBounty;

        [SaveableField(14)]
        public string TroopId;

        [SaveableField(20)]
        public BountyQuest AssociatedQuest;

        [SaveableField(21)]
        public string FactionId;

        [SaveableField(24)]
        public string TargetFactionId;

        [SaveableField(22)]
        public bool IsPatrolBounty;

        [SaveableField(23)]
        public float PatrolRadius;

        /// <summary>
        /// Creates a new active bounty targeting the given hero.
        /// </summary>
        public BountyTarget(Hero targetHero, int bountyValue, int guardPartySize, int daysUntilExpiry, bool isPlayerBounty = false)
        {
            TargetHero = targetHero;
            BountyValue = bountyValue;
            GuardPartySize = guardPartySize;
            Status = BountyStatus.Active;
            ExpiryDate = CampaignTime.DaysFromNow(daysUntilExpiry);
            IsPlayerBounty = isPlayerBounty;
            IsTracked = false;
            TrackerId = Guid.NewGuid().ToString();
        }

        public BountyTarget() { }

        /// <summary>
        /// Whether this bounty's expiry date has passed.
        /// </summary>
        public bool IsExpired => CampaignTime.Now > ExpiryDate;

        /// <summary>
        /// Whether this bounty is anchored to a specific settlement.
        /// </summary>
        public bool IsSettlementAnchored => !string.IsNullOrEmpty(TargetSettlementId);
    }
}