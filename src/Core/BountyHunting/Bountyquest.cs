using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace SeparatistCrisis.BountyHunting
{
    /// <summary>
    /// Thin QuestBase shell around an accepted BountyTarget. BountyTarget remains the
    /// source of truth for spawn/combat/capture logic; this class only adds the
    /// vanilla Quests-screen entry, journal log, map tracking, and completion.
    /// </summary>
    public sealed class BountyQuest : QuestBase
    {
        [SaveableField(1)]
        private string _bountyTrackerId;

        [SaveableField(2)]
        private BountyTarget _bounty;

        private JournalLog _captureLog;

        public override bool IsRemainingTimeHidden => false;

        public override TextObject Title
        {
            get
            {
                string heroName = _bounty?.TargetHero?.Name?.ToString() ?? "Unknown";
                return new TextObject("{=BountyQuestTitle}Bounty: {HERO_NAME}").SetTextVariable("HERO_NAME", heroName);
            }
        }

        public override string SpecialQuestType => "bounty_hunting";

        /// <summary>
        /// Creates a quest wrapping the given bounty, with the given giver and
        /// duration, and sets up its journal log and map tracking.
        /// </summary>
        public BountyQuest(BountyTarget bounty, Hero questGiver, CampaignTime duration)
            : base("bountyquest_" + bounty.TrackerId, questGiver, duration, 0)
        {
            _bountyTrackerId = bounty.TrackerId;
            _bounty = bounty;

            InitializeQuestOnCreation();
            SetupBountyQuestContent();
        }

        /// <summary>
        /// Adds the capture journal log and starts tracking the bounty's location on
        /// the map.
        /// </summary>
        private void SetupBountyQuestContent()
        {
            if (_bounty == null)
            {
                BountyLogger.Log($"BountyQuest.SetupBountyQuestContent: ABORT — no bounty reference (tracker '{_bountyTrackerId}').");
                return;
            }

            string heroName = _bounty.TargetHero?.Name?.ToString() ?? "the target";

            _captureLog = AddDiscreteLog(
                new TextObject("{=BountyQuestLogTitleCapture}Capture {HERO_NAME}").SetTextVariable("HERO_NAME", heroName),
                new TextObject("{=BountyQuestLogTextCapture}{HERO_NAME} has a price on their head. Track them down, defeat them, and bring them in — dead or alive.").SetTextVariable("HERO_NAME", heroName),
                0,
                1);

            TrackBountyLocation();

            BountyLogger.Log($"BountyQuest.SetupBountyQuestContent: quest set up for {heroName} (tracker '{_bountyTrackerId}').");
        }

        /// <summary>
        /// Re-resolves the bounty reference if needed and resumes map tracking after
        /// a save is loaded.
        /// </summary>
        protected override void InitializeQuestOnGameLoad()
        {
            if (_bounty == null)
            {
                _bounty = ResolveBountyByTrackerId();
                BountyLogger.Log($"BountyQuest.InitializeQuestOnGameLoad: re-resolved bounty for tracker '{_bountyTrackerId}' — found={_bounty != null}.");
            }

            TrackBountyLocation();
        }

        /// <summary>
        /// Looks up the live BountyTarget for this quest's tracker id via the
        /// campaign behavior.
        /// </summary>
        private BountyTarget ResolveBountyByTrackerId()
        {
            return Campaign.Current?.GetCampaignBehavior<BountyHunterBehavior>()?.FindBountyByTrackerId(_bountyTrackerId);
        }

        /// <summary>
        /// Adds a tracked map object for the bounty's settlement or, if not
        /// settlement-anchored, its target's current party.
        /// </summary>
        private void TrackBountyLocation()
        {
            if (_bounty == null) return;

            if (_bounty.IsSettlementAnchored && !_bounty.IsPatrolBounty)
            {
                var settlement = Settlement.All.FirstOrDefault(s => s.StringId == _bounty.TargetSettlementId);
                if (settlement != null)
                {
                    AddTrackedObject(settlement);
                }
                return;
            }

            if (_bounty.TargetHero?.PartyBelongedTo != null)
            {
                AddTrackedObject(_bounty.TargetHero.PartyBelongedTo);
            }
        }

        /// <summary>
        /// Advances the journal log's progress when the bounty target is captured,
        /// without completing the quest.
        /// </summary>
        public void NotifyBountyCaptured()
        {
            _captureLog?.UpdateCurrentProgress(1);
            BountyLogger.Log($"BountyQuest.NotifyBountyCaptured: tracker '{_bountyTrackerId}'.");
        }

        /// <summary>
        /// Completes the quest with success or failure, called externally once the
        /// underlying bounty is resolved one way or the other.
        /// </summary>
        public void CompleteBountyQuest(bool success)
        {
            if (success)
            {
                BountyLogger.Log($"BountyQuest.CompleteBountyQuest: SUCCESS — tracker '{_bountyTrackerId}'.");
                CompleteQuestWithSuccess();
            }
            else
            {
                BountyLogger.Log($"BountyQuest.CompleteBountyQuest: FAIL — tracker '{_bountyTrackerId}'.");
                CompleteQuestWithFail(new TextObject("{=BountyQuestFailed}The bounty is no longer available."));
            }
        }

        /// <summary>
        /// No custom quest-giver dialogue; the per-bounty-type combat dialogue
        /// already covers the encounter with the target.
        /// </summary>
        protected override void SetDialogs()
        {
        }

        /// <summary>
        /// Registers base quest events. No additional listeners are added.
        /// </summary>
        protected override void RegisterEvents()
        {
            base.RegisterEvents();
        }

        /// <summary>
        /// Hourly tick, unused.
        /// </summary>
        protected override void HourlyTick()
        {
        }
    }
}