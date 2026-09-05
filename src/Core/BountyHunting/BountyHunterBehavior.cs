using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using Helpers;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.MountAndBlade.AI.AgentComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.InputSystem;

namespace SeparatistCrisis.BountyHunting
{
    /// <summary>
    /// Main campaign behavior for the bounty hunting system. Owns bounty generation,
    /// tracking, capture/turn-in handling, settlement/mission spawning for each
    /// bounty type, dialogue, and the bounty board menu.
    /// </summary>
    public class BountyHunterBehavior : CampaignBehaviorBase
    {
        private List<BountyTarget> _activeBounties = new List<BountyTarget>();

        private List<string> _usedDefinitionIds = new List<string>();

        private List<string> _pendingHideoutDefinitionIds = new List<string>();

        private readonly HashSet<string> _gangPartiesEngagingBattle = new HashSet<string>();

        private readonly HashSet<string> _gangPartiesBeingCleanedUp = new HashSet<string>();

        private readonly HashSet<string> _stealthSpawnedThisMission = new HashSet<string>();

        private const int MaxActiveBounties = 15;
        private const float DailyBountySpawnChance = 0.15f;
        private const float HideoutResolutionChancePerSettlementEntry = 0.15f;
        private const int MinBountyValue = 500;
        private const int MaxBountyValue = 5000;
        private const int MinExpiryDays = 10;
        private const int MaxExpiryDays = 30;
        private const int MaxDisplayedBountySlots = 15;

        private const float MarkerRefreshIntervalSeconds = 10f;

        private int _tickLogCounter = 0;
        private float _markerRefreshTimer = 0f;

        private Hero _pendingBountyDialogueHero;

        private bool _bountyConversationInProgress = false;

        public bool IsBountyConversationInProgress => _bountyConversationInProgress;

        private HashSet<Hero> _pendingForcedCaptures = new HashSet<Hero>();

        /// <summary>
        /// Subscribes to every campaign event this behavior needs.
        /// </summary>
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
            CampaignEvents.CanHeroDieEvent.AddNonSerializedListener(this, OnCanHeroDie);
            CampaignEvents.CanHeroBecomePrisonerEvent.AddNonSerializedListener(this, OnCanHeroBecomePrisoner);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, OnMissionStarted);
            CampaignEvents.OnHideoutDeactivatedEvent.AddNonSerializedListener(this, OnHideoutDeactivated);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);

            BountyLogger.Log("RegisterEvents called — behavior initialized.");
        }

        /// <summary>
        /// Registers persisted fields with the save system.
        /// </summary>
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_bh_activeBounties", ref _activeBounties);
            if (_activeBounties == null)
                _activeBounties = new List<BountyTarget>();

            dataStore.SyncData("_bh_usedDefinitionIds", ref _usedDefinitionIds);
            if (_usedDefinitionIds == null)
                _usedDefinitionIds = new List<string>();

            dataStore.SyncData("_bh_pendingHideoutDefinitionIds", ref _pendingHideoutDefinitionIds);
            if (_pendingHideoutDefinitionIds == null)
                _pendingHideoutDefinitionIds = new List<string>();

            BountyLogger.Log($"SyncData completed — {_activeBounties.Count} bounties in list, {_usedDefinitionIds.Count} definition(s) already used, {_pendingHideoutDefinitionIds.Count} hideout definition(s) pending settlement entry.");
        }

        /// <summary>
        /// Loads bounty definitions, registers game menus and dialogue, and refreshes
        /// tracked markers and the settlement id log when a session launches.
        /// </summary>
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            BountyDefinitionRegistry.LoadDefinitions();
            AddGameMenus(starter);
            AddDialogs(starter);
            RefreshTrackedMarkers();
            LogAllSettlementIds();

            BountyLogger.Log("Session launched — game menus and dialogs registered.");
        }

        /// <summary>
        /// Logs every village and town's name and StringId, for finding real
        /// settlement ids to use in BountyDefinitions.xml.
        /// </summary>
        private void LogAllSettlementIds()
        {
            var villages = Settlement.All.Where(s => s != null && s.IsVillage).OrderBy(s => s.Name.ToString());
            var towns = Settlement.All.Where(s => s != null && s.IsTown).OrderBy(s => s.Name.ToString());

            BountyLogger.Log("--- Settlement StringId reference (for BountyDefinitions.xml <SettlementId> entries) ---");
            foreach (var v in villages)
            {
                BountyLogger.Log($"  [Village] {v.Name} -> {v.StringId}");
            }
            foreach (var t in towns)
            {
                BountyLogger.Log($"  [Town] {t.Name} -> {t.StringId}");
            }
            BountyLogger.Log("--- End settlement StringId reference ---");
        }

        /// <summary>
        /// Per-frame tick: logs a periodic heartbeat, refreshes tracked markers on
        /// an interval, and checks for the F10 debug force-spawn key.
        /// </summary>
        private void OnTick(float dt)
        {
            _tickLogCounter++;
            if (_tickLogCounter % 300 == 0)
            {
                BountyLogger.Log($"OnTick heartbeat — still running. Counter={_tickLogCounter}");
            }

            _markerRefreshTimer += dt;
            if (_markerRefreshTimer >= MarkerRefreshIntervalSeconds)
            {
                _markerRefreshTimer = 0f;
                RefreshTrackedMarkers();
            }

            // ============================================================
            // DEBUG_FORCE_SPAWN_F10 — TEMPORARY, remove this whole block when
            // done testing. Pressing F10 force-resolves every currently-loaded
            // BountyDefinition that hasn't already been used (by a previous F10
            // press or the normal daily roll), bypassing the daily chance roll
            // entirely but still respecting the one-time-use tracking — it will
            // NOT re-spawn a definition that's already generated. Search
            // "DEBUG_FORCE_SPAWN_F10" to find every piece of this later.
            // ============================================================
            if (Input.IsKeyPressed(InputKey.F10))
            {
                var availableDefinitions = BountyDefinitionRegistry.Definitions.Where(d => !_usedDefinitionIds.Contains(d.Id)).ToList();
                BountyLogger.Log($"DEBUG_FORCE_SPAWN_F10: F10 pressed — {availableDefinitions.Count}/{BountyDefinitionRegistry.Definitions.Count} definition(s) not yet used, force-resolving each.");

                foreach (var definition in availableDefinitions)
                {
                    ResolveDefinitionAndMarkUsedIfProduced(definition, "DEBUG_FORCE_SPAWN_F10");
                }
            }
            // ============================================================
            // END DEBUG_FORCE_SPAWN_F10
            // ============================================================
        }

        /// <summary>
        /// Updates the map marker position for every currently tracked, active
        /// bounty.
        /// </summary>
        private void RefreshTrackedMarkers()
        {
            var trackedBounties = _activeBounties.Where(b => b.Status == BountyStatus.Active && b.IsTracked).ToList();
            if (trackedBounties.Count == 0) return;

            foreach (var bounty in trackedBounties)
            {
                UpdateMarkerPosition(bounty);
            }
        }

        /// <summary>
        /// Recreates the map marker for a single bounty, at its settlement if
        /// settlement-anchored, otherwise at its target's current party position.
        /// </summary>
        private void UpdateMarkerPosition(BountyTarget bounty)
        {
            if (bounty.IsSettlementAnchored && !bounty.IsPatrolBounty)
            {
                var settlement = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
                if (settlement == null)
                {
                    BountyLogger.Log($"UpdateMarkerPosition: settlement '{bounty.TargetSettlementId}' for {bounty.TargetHero?.Name} did not resolve — leaving last known marker in place.");
                    return;
                }

                Campaign.Current.MapMarkerManager.RemoveAllMapMarkersByQuestId(bounty.TrackerId);

                var settlementMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(
                    settlement.Party.Banner,
                    new TextObject("{=BountyMarkerSettlement}Bounty: {HERO_NAME} (near {SETTLEMENT_NAME})")
                        .SetTextVariable("HERO_NAME", bounty.TargetHero?.Name)
                        .SetTextVariable("SETTLEMENT_NAME", settlement.Name),
                    settlement.GatePosition.AsVec3(),
                    true,
                    bounty.TrackerId);

                BountyLogger.Log($"Refreshed settlement-anchored marker for {bounty.TargetHero?.Name} at {settlementMarker.Position} ({settlement.Name}).");
                return;
            }

            var party = bounty.TargetHero?.PartyBelongedTo;
            if (party == null)
            {
                BountyLogger.Log($"UpdateMarkerPosition: {bounty.TargetHero?.Name} has no party — leaving last known marker in place.");
                return;
            }

            Campaign.Current.MapMarkerManager.RemoveAllMapMarkersByQuestId(bounty.TrackerId);

            var newMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(
                party.Banner,
                new TextObject("{=BountyMarker}Bounty: {HERO_NAME}").SetTextVariable("HERO_NAME", bounty.TargetHero.Name),
                party.GetPositionAsVec3(),
                true,
                bounty.TrackerId);

            BountyLogger.Log($"Refreshed marker for {bounty.TargetHero.Name} at {newMarker.Position}.");
        }

        /// <summary>
        /// Daily tick: cleans up expired and unresolvable bounties, then rolls for a
        /// new bounty.
        /// </summary>
        private void OnDailyTick()
        {
            BountyLogger.Log("Daily tick fired.");
            CleanupExpiredBounties();
            CleanupUnresolvableBounties();
            GenerateBounty(forced: false);
        }

        /// <summary>
        /// Removes any active, non-settlement-anchored bounty whose target no longer
        /// has a party and isn't the player's own prisoner.
        /// </summary>
        private void CleanupUnresolvableBounties()
        {
            var unresolvable = _activeBounties.Where(b =>
                b.Status == BountyStatus.Active &&
                (!b.IsSettlementAnchored || b.IsPatrolBounty) &&
                b.TargetHero != null &&
                b.TargetHero.IsAlive &&
                b.TargetHero.PartyBelongedTo == null &&
                b.TargetHero.PartyBelongedToAsPrisoner != PartyBase.MainParty).ToList();

            foreach (var bounty in unresolvable)
            {
                RemoveUnresolvableBounty(bounty, "their party no longer exists.");
            }
        }

        /// <summary>
        /// Marks any active bounty past its expiry date as expired, fails its
        /// associated quest, and prunes old resolved bounty records.
        /// </summary>
        private void CleanupExpiredBounties()
        {
            var expiredNow = _activeBounties.Where(b => b.Status == BountyStatus.Active && b.IsExpired).ToList();
            foreach (var bounty in expiredNow)
            {
                bounty.Status = BountyStatus.Expired;
                UntrackBounty(bounty);
                bounty.AssociatedQuest?.CompleteBountyQuest(false);
                BountyLogger.Log($"Bounty expired: {bounty.TargetHero?.Name} ({bounty.BountyValue} gold).");
            }

            int beforePrune = _activeBounties.Count;
            _activeBounties.RemoveAll(b => b.Status != BountyStatus.Active &&
                                            b.Status != BountyStatus.Captured &&
                                            b.ExpiryDate.ElapsedDaysUntilNow > 30f);
            int pruned = beforePrune - _activeBounties.Count;
            if (pruned > 0)
                BountyLogger.Log($"Pruned {pruned} old resolved bounty record(s).");
        }

        /// <summary>
        /// Rolls for and generates a new bounty from an unused definition, respecting
        /// the active bounty cap and (unless forced) the daily spawn chance.
        /// </summary>
        private void GenerateBounty(bool forced)
        {
            int currentActive = _activeBounties.Count(b => b.Status == BountyStatus.Active);
            BountyLogger.Log($"GenerateBounty called (forced={forced}). Current active: {currentActive}/{MaxActiveBounties}.");

            if (currentActive >= MaxActiveBounties)
            {
                BountyLogger.Log("Bounty board full — skipping generation.");
                return;
            }

            if (!forced && MBRandom.RandomFloat > DailyBountySpawnChance)
            {
                BountyLogger.Log("Random chance roll failed — no bounty generated this tick.");
                return;
            }

            var allDefinitions = BountyDefinitionRegistry.Definitions;
            if (allDefinitions.Count == 0)
            {
                BountyLogger.Log("GenerateBounty: ABORT — no BountyDefinitions loaded at all.");
                return;
            }

            var availableDefinitions = allDefinitions.Where(d => !_usedDefinitionIds.Contains(d.Id)).ToList();
            if (availableDefinitions.Count == 0)
            {
                BountyLogger.Log($"GenerateBounty: ABORT — all {allDefinitions.Count} loaded definition(s) have already been used once each; none left to generate.");
                return;
            }

            var definition = availableDefinitions[MBRandom.RandomInt(availableDefinitions.Count)];
            BountyLogger.Log($"GenerateBounty: selected definition '{definition.Id}' (SpawnType={definition.SpawnType}) — {availableDefinitions.Count}/{allDefinitions.Count} definition(s) still available.");

            ResolveDefinitionAndMarkUsedIfProduced(definition, "GenerateBounty");
        }

        /// <summary>
        /// Resolves a definition and marks it as used only if it actually produced
        /// an active bounty or queued a pending hideout resolution — a definition
        /// that silently aborts (e.g. no eligible settlement right now) keeps its
        /// one-time-use slot available to try again later. Shared by both the
        /// normal daily roll and the DEBUG_FORCE_SPAWN_F10 debug handler.
        /// </summary>
        private void ResolveDefinitionAndMarkUsedIfProduced(BountyDefinition definition, string callerLabel)
        {
            int countBefore = _activeBounties.Count;
            ResolveDefinition(definition);

            bool producedActiveBounty = _activeBounties.Count > countBefore;
            bool queuedForLaterResolution = _pendingHideoutDefinitionIds.Contains(definition.Id);

            if (producedActiveBounty || queuedForLaterResolution)
            {
                _usedDefinitionIds.Add(definition.Id);
            }
            else
            {
                BountyLogger.Log($"{callerLabel}: '{definition.Id}' did not actually produce a bounty this time (see the ABORT log line above) — NOT marking it as used, it can still be picked again later.");
            }
        }

        /// <summary>
        /// Dispatches a bounty definition to the resolver method matching its spawn
        /// type.
        /// </summary>
        private void ResolveDefinition(BountyDefinition definition)
        {
            switch (definition.SpawnType)
            {
                case BountySpawnType.Wandering:
                    ResolveWanderingBounty(definition);
                    break;
                case BountySpawnType.SettlementGang:
                    ResolveSettlementGangBounty(definition);
                    break;
                case BountySpawnType.SettlementStealth:
                    ResolveStealthBounty(definition);
                    break;
                case BountySpawnType.PatrolParty:
                    ResolvePatrolParty(definition);
                    break;
                case BountySpawnType.HideoutBoss:
                    ResolveHideoutBossBounty(definition);
                    break;
                case BountySpawnType.TwoStageSettlement:
                    ResolveTwoStageSettlementBounty(definition);
                    break;
                default:
                    BountyLogger.Log($"ResolveDefinition: ABORT — unhandled SpawnType '{definition.SpawnType}' for definition '{definition.Id}'.");
                    break;
            }
        }

        /// <summary>
        /// Queues a hideout-boss bounty definition for resolution the next time the
        /// player enters any settlement, rather than resolving it immediately —
        /// "nearest hideout to the player" is only meaningful relative to where the
        /// player actually is, and freezing that choice at generation time (which
        /// could be session launch, mid-battle, anywhere) produced a hideout that
        /// had nothing to do with the player's actual location by the time they
        /// went looking for it.
        /// </summary>
        private void ResolveHideoutBossBounty(BountyDefinition definition)
        {
            if (definition?.Template == null)
            {
                BountyLogger.Log($"ResolveHideoutBossBounty: ABORT — definition '{definition?.Id}' has no HeroTemplate.");
                return;
            }

            _pendingHideoutDefinitionIds.Add(definition.Id);
            BountyLogger.Log($"ResolveHideoutBossBounty: queued '{definition.Id}' for resolution on next settlement entry.");
        }

        /// <summary>
        /// Actually resolves a hideout-boss bounty: picks the hideout nearest the
        /// given reference position (normally the settlement just entered),
        /// creates the boss hero, and adds the bounty. Called once per queued
        /// definition when the player enters a settlement.
        /// </summary>
        private void ResolveHideoutBossBountyAt(BountyDefinition definition, CampaignVec2 referencePosition, string referenceDescription)
        {
            var candidates = Hideout.All.Where(h => h?.Settlement != null).ToList();
            if (candidates.Count == 0)
            {
                BountyLogger.Log("ResolveHideoutBossBountyAt: no hideouts exist on the map — aborting this roll.");
                return;
            }

            BountyLogger.Log($"ResolveHideoutBossBountyAt: referencePosition={referencePosition} (source={referenceDescription}).");

            // Settlement.Position (the standard overworld map-icon location) is
            // used for the hideout side of this comparison — GatePosition
            // represents a physical entrance point, meaningful for towns/castles
            // with a real gate, but a Hideout isn't a fortified settlement and
            // isn't confirmed to have a meaningful gate at all. GatePosition is
            // still logged alongside it purely to see how much the two diverge.
            var sortedByDistance = candidates
                .Select(h => new
                {
                    Hideout = h,
                    PositionDistance = h.Settlement.Position.Distance(referencePosition),
                    GatePositionDistance = h.Settlement.GatePosition.Distance(referencePosition)
                })
                .OrderBy(x => x.PositionDistance)
                .ToList();

            float minDist = sortedByDistance.First().PositionDistance;
            float maxDist = sortedByDistance.Last().PositionDistance;
            float medianDist = sortedByDistance[sortedByDistance.Count / 2].PositionDistance;
            BountyLogger.Log($"ResolveHideoutBossBountyAt: Position-based distance distribution across {sortedByDistance.Count} candidates — min={minDist:0.0}, median={medianDist:0.0}, max={maxDist:0.0}. Closest 5 (Position dist / GatePosition dist): [{string.Join(", ", sortedByDistance.Take(5).Select(x => $"{x.Hideout.Settlement.StringId}={x.PositionDistance:0.0}/{x.GatePositionDistance:0.0}"))}].");

            var hideout = sortedByDistance.First().Hideout;

            var heroTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.Template.CharacterTemplateId);
            if (heroTemplate == null)
            {
                BountyLogger.Log($"ResolveHideoutBossBountyAt: ABORT — CharacterTemplateId '{definition.Template.CharacterTemplateId}' did not resolve.");
                return;
            }

            var banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == hideout.Settlement.Culture)
                           ?? Clan.BanditFactions.FirstOrDefault();
            if (banditClan == null)
            {
                BountyLogger.Log("ResolveHideoutBossBountyAt: ABORT — no bandit clan found.");
                return;
            }

            int age = MBRandom.RandomInt(definition.Template.MinAge, definition.Template.MaxAge);
            Hero leaderHero = HeroCreator.CreateSpecialHero(heroTemplate, hideout.Settlement, banditClan, null, age);
            if (leaderHero == null)
            {
                BountyLogger.Log("ResolveHideoutBossBountyAt: ABORT — HeroCreator.CreateSpecialHero returned null.");
                return;
            }

            var leaderName = new TextObject(definition.Template.NameText ?? "Hideout Boss");
            leaderHero.SetName(leaderName, leaderName);
            leaderHero.ChangeState(Hero.CharacterStates.Active);

            ApplyHeroTemplateEquipment(leaderHero, definition.Template);
            ApplyHeroTemplateBodyProperties(leaderHero, definition.Template);

            leaderHero.IsKnownToPlayer = true;

            hideout.IsSpotted = true;

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);

            var bounty = new BountyTarget(leaderHero, value, guardPartySize: definition.TroopCount, expiryDays)
            {
                TargetSettlementId = hideout.Settlement.StringId,
                IsHideoutBounty = true,
                TroopId = definition.TroopId,
                Description = definition.Description,
                FactionId = definition.FactionId
            };
            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolveHideoutBossBountyAt: nearest hideout selected — '{hideout.Settlement.Name}' ({hideout.Settlement.StringId}), {candidates.Count} candidate(s) considered relative to {referenceDescription}. Created leader hero '{leaderHero.Name}' (no party spawned — never visible on the map until captured/removed). value={value}, expiryDays={expiryDays}.");
            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {(string.IsNullOrEmpty(definition.Description) ? "A bandit boss has been located." : definition.Description)} ({leaderHero.Name}, near {hideout.Settlement.Name})"));
        }

        /// <summary>
        /// Creates a wandering-lord bounty targeting either a specific hero by id or
        /// a randomly chosen eligible lord.
        /// </summary>
        private void ResolveWanderingBounty(BountyDefinition definition)
        {
            Hero candidate;
            if (!string.IsNullOrEmpty(definition.ExistingLordId))
            {
                candidate = Hero.AllAliveHeroes.FirstOrDefault(h => h.StringId == definition.ExistingLordId);
                if (candidate == null)
                {
                    BountyLogger.Log($"ResolveWanderingBounty: ABORT — ExistingLordId '{definition.ExistingLordId}' did not resolve to a living hero.");
                    return;
                }
            }
            else
            {
                candidate = FindBountyCandidate(definition.FactionId);
                if (candidate == null)
                {
                    BountyLogger.Log($"ResolveWanderingBounty: no eligible bounty candidates found (FactionId='{definition.FactionId ?? "none"}').");
                    return;
                }
            }

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int guardSize = MBRandom.RandomInt(0, 6);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);

            candidate.IsKnownToPlayer = true;

            var bounty = new BountyTarget(candidate, value, guardSize, expiryDays)
            {
                Description = definition.Description,
                FactionId = definition.BountyFactionId ?? definition.FactionId
            };
            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolveWanderingBounty: new bounty generated on '{definition.Id}': Target={candidate.Name}, Value={value}, GuardSize={guardSize}, ExpiryDays={expiryDays}.");
        }

        /// <summary>
        /// Picks a random eligible lord not already targeted by an active bounty,
        /// optionally restricted to a given faction.
        /// </summary>
        private Hero FindBountyCandidate(string factionId = null)
        {
            var alreadyTargeted = _activeBounties
                .Where(b => b.Status == BountyStatus.Active)
                .Select(b => b.TargetHero)
                .ToHashSet();

            var candidates = Hero.AllAliveHeroes
                .Where(h => h.IsLord
                         && !h.IsPrisoner
                         && h.PartyBelongedTo != null
                         && !alreadyTargeted.Contains(h)
                         && (string.IsNullOrEmpty(factionId) || (h.MapFaction != null && h.MapFaction.StringId == factionId)))
                .ToList();

            BountyLogger.Log($"FindBountyCandidate: {candidates.Count} eligible heroes found (excluding {alreadyTargeted.Count} already-targeted, FactionId='{factionId ?? "none"}').");

            if (candidates.Count == 0) return null;

            return candidates[MBRandom.RandomInt(candidates.Count)];
        }

        /// <summary>
        /// Returns every currently active bounty.
        /// </summary>
        public IReadOnlyList<BountyTarget> GetActiveBounties()
        {
            return _activeBounties.Where(b => b.Status == BountyStatus.Active).ToList();
        }

        /// <summary>
        /// Removes a bounty that has become permanently unresolvable, resetting any
        /// village raid state and despawning its party if applicable.
        /// </summary>
        private void RemoveUnresolvableBounty(BountyTarget bounty, string reason)
        {
            BountyLogger.Log($"RemoveUnresolvableBounty: removing bounty on {bounty.TargetHero?.Name} — {reason}");

            if (bounty.IsSettlementAnchored && !bounty.IsHideoutBounty && !bounty.IsTavernBounty && !bounty.IsPatrolBounty)
            {
                var settlement = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
                if (settlement != null)
                {
                    ChangeVillageStateAction.ApplyBySettingToNormal(settlement);
                    BountyLogger.Log($"RemoveUnresolvableBounty: reset '{settlement.Name}' back to normal state.");
                }
            }

            if (bounty.IsHideoutBounty && !string.IsNullOrEmpty(bounty.GangPartyId))
            {
                var hideoutParty = FindMobilePartyByStringId(bounty.GangPartyId);
                if (hideoutParty != null && hideoutParty.IsActive)
                {
                    _gangPartiesBeingCleanedUp.Add(bounty.GangPartyId);
                    DestroyPartyAction.Apply(null, hideoutParty);
                    BountyLogger.Log($"RemoveUnresolvableBounty: despawned hideout bounty party '{bounty.GangPartyId}'.");
                }
            }

            UntrackBounty(bounty);
            bounty.AssociatedQuest?.CompleteBountyQuest(false);
            _activeBounties.Remove(bounty);

            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {bounty.TargetHero?.Name}'s bounty is no longer available — {reason}"));
        }

        /// <summary>
        /// Removes a bounty whose target's party was destroyed by someone other than
        /// the player, since the bounty becomes unresolvable in that case.
        /// </summary>
        private void OnMobilePartyDestroyed(MobileParty destroyedParty, PartyBase destroyerParty)
        {
            if (destroyedParty == null) return;
            if (destroyerParty == PartyBase.MainParty) return;

            if (_gangPartiesBeingCleanedUp.Remove(destroyedParty.StringId))
            {
                BountyLogger.Log($"OnMobilePartyDestroyed: party '{destroyedParty.StringId}' was our own intentional ephemeral cleanup (settlement-leave despawn), not a real combat loss — bounty preserved.");
                return;
            }

            var bounty = _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                !string.IsNullOrEmpty(b.GangPartyId) &&
                b.GangPartyId == destroyedParty.StringId);

            if (bounty == null)
            {
                bounty = _activeBounties.FirstOrDefault(b =>
                    b.Status == BountyStatus.Active &&
                    b.TargetHero != null &&
                    (destroyedParty.LeaderHero == b.TargetHero ||
                     destroyedParty.MemberRoster.Contains(b.TargetHero.CharacterObject)));
            }

            if (bounty == null)
            {
                BountyLogger.Log($"OnMobilePartyDestroyed: party '{destroyedParty.StringId}' destroyed by '{destroyerParty?.Name}' — no matching active bounty found.");
                return;
            }

            RemoveUnresolvableBounty(bounty, $"their party was destroyed by {destroyerParty?.Name?.ToString() ?? "someone else"}.");
        }

        /// <summary>
        /// Applies a hero template's equipment slots to both battle and civilian
        /// equipment.
        /// </summary>
        private void ApplyHeroTemplateEquipment(Hero hero, HeroTemplate template)
        {
            if (hero == null || template == null) return;

            foreach (var kvp in template.EquipmentSlots)
            {
                if (!Enum.TryParse(kvp.Key, out EquipmentIndex slot))
                {
                    BountyLogger.Log($"ApplyHeroTemplateEquipment: invalid slot name '{kvp.Key}' — skipped.");
                    continue;
                }

                var item = MBObjectManager.Instance.GetObject<ItemObject>(kvp.Value);
                if (item == null)
                {
                    BountyLogger.Log($"ApplyHeroTemplateEquipment: item '{kvp.Value}' for slot '{kvp.Key}' did not resolve — skipped.");
                    continue;
                }

                hero.BattleEquipment[slot] = new EquipmentElement(item);
                hero.CivilianEquipment[slot] = new EquipmentElement(item);
                BountyLogger.Log($"ApplyHeroTemplateEquipment: equipped '{kvp.Value}' in slot '{kvp.Key}' (both Battle and Civilian).");
            }
        }

        /// <summary>
        /// Applies a hero template's custom face/body properties, if set, via the
        /// StaticBodyProperties 8-ulong constructor.
        /// </summary>
        private void ApplyHeroTemplateBodyProperties(Hero hero, HeroTemplate template)
        {
            if (hero == null || template == null || !template.HasBodyProperties) return;

            string hex = template.BodyPropertiesKeyHex ?? "";
            hex = hex.PadRight(128, '0');

            try
            {
                ulong[] parts = new ulong[8];
                for (int i = 0; i < 8; i++)
                {
                    string chunk = hex.Substring(i * 16, 16);
                    parts[i] = Convert.ToUInt64(chunk, 16);
                }

                var staticProps = new TaleWorlds.Core.StaticBodyProperties(
                    parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7]);

                hero.StaticBodyProperties = staticProps;
                hero.Weight = template.BodyPropertiesWeight;
                hero.Build = template.BodyPropertiesBuild;

                BountyLogger.Log($"ApplyHeroTemplateBodyProperties: applied custom body/face to {hero.Name} via direct StaticBodyProperties construction (weight={hero.Weight}, build={hero.Build}).");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"ApplyHeroTemplateBodyProperties: EXCEPTION parsing/applying BodyPropertiesKeyHex '{template.BodyPropertiesKeyHex}': {ex}");
            }
        }

        /// <summary>
        /// Resolves a target settlement for a bounty definition, filtered by faction
        /// (settlements currently owned by that Kingdom/Clan), optional explicit
        /// settlement whitelist, and settlement kind.
        /// </summary>
        private Settlement ResolveDefinitionSettlement(BountyDefinition definition, string callerName, Func<Settlement, bool> settlementKindFilter)
        {
            if (definition == null)
            {
                BountyLogger.Log($"{callerName}: ABORT — definition is null.");
                return null;
            }

            IEnumerable<Settlement> pool = Settlement.All.Where(s => s != null && settlementKindFilter(s));

            if (!string.IsNullOrEmpty(definition.FactionId))
            {
                pool = pool.Where(s => s.MapFaction != null && s.MapFaction.StringId == definition.FactionId);
            }

            if (definition.SettlementIds != null && definition.SettlementIds.Count > 0)
            {
                var idSet = definition.SettlementIds.ToHashSet();
                pool = pool.Where(s => idSet.Contains(s.StringId));
            }

            var resolved = pool.ToList();
            if (resolved.Count == 0)
            {
                BountyLogger.Log($"{callerName}: ABORT — no eligible settlement found (FactionId='{definition.FactionId ?? "none"}', SettlementIds=[{string.Join(",", definition.SettlementIds ?? new List<string>())}]). Check that the faction actually owns a matching settlement right now.");
                return null;
            }

            var chosen = resolved[MBRandom.RandomInt(resolved.Count)];
            BountyLogger.Log($"{callerName}: resolved settlement '{chosen.Name}' ({chosen.StringId}) — {resolved.Count} eligible candidate(s) for FactionId='{definition.FactionId ?? "none"}'.");
            return chosen;
        }

        /// <summary>
        /// Creates a patrol-party bounty: a hero-led party with real ongoing patrol
        /// AI around a settlement, capturable via the normal bounty pipeline.
        /// </summary>
        private void ResolvePatrolParty(BountyDefinition definition)
        {
            if (definition?.Template == null)
            {
                BountyLogger.Log($"ResolvePatrolParty: ABORT — definition '{definition?.Id}' has no HeroTemplate.");
                return;
            }

            Settlement patrolCenter;
            if (!string.IsNullOrEmpty(definition.PatrolCenterSettlementId))
            {
                patrolCenter = Settlement.All.FirstOrDefault(s => s.StringId == definition.PatrolCenterSettlementId);
                if (patrolCenter == null)
                {
                    BountyLogger.Log($"ResolvePatrolParty: ABORT — PatrolCenterSettlementId '{definition.PatrolCenterSettlementId}' did not resolve to a real settlement.");
                    return;
                }
            }
            else
            {
                patrolCenter = ResolveDefinitionSettlement(definition, "ResolvePatrolParty", s => true);
                if (patrolCenter == null)
                {
                    return;
                }
            }

            Clan clan;
            if (string.IsNullOrEmpty(definition.FactionId) || string.Equals(definition.FactionId, "Bandit", StringComparison.OrdinalIgnoreCase))
            {
                clan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == patrolCenter.Culture && c.StringId != "looters")
                    ?? Clan.BanditFactions.FirstOrDefault(c => c.StringId != "looters")
                    ?? Clan.BanditFactions.FirstOrDefault();
            }
            else
            {
                clan = Clan.All.FirstOrDefault(c => c.StringId == definition.FactionId)
                    ?? Kingdom.All.FirstOrDefault(k => k.StringId == definition.FactionId)?.RulingClan;
            }

            if (clan == null)
            {
                BountyLogger.Log($"ResolvePatrolParty: ABORT — FactionId '{definition.FactionId}' did not resolve to any Clan, bandit faction, or Kingdom's ruling clan.");
                return;
            }

            var heroTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.Template.CharacterTemplateId);
            if (heroTemplate == null)
            {
                BountyLogger.Log($"ResolvePatrolParty: ABORT — CharacterTemplateId '{definition.Template.CharacterTemplateId}' did not resolve.");
                return;
            }

            int age = MBRandom.RandomInt(definition.Template.MinAge, definition.Template.MaxAge);
            Hero leaderHero = HeroCreator.CreateSpecialHero(heroTemplate, patrolCenter, clan, null, age);
            if (leaderHero == null)
            {
                BountyLogger.Log("ResolvePatrolParty: ABORT — HeroCreator.CreateSpecialHero returned null.");
                return;
            }

            var leaderName = new TextObject(definition.Template.NameText ?? "{CLAN_NAME} Patrol Leader")
                .SetTextVariable("CLAN_NAME", clan.Name);
            leaderHero.SetName(leaderName, leaderName);
            leaderHero.ChangeState(Hero.CharacterStates.Active);

            ApplyHeroTemplateEquipment(leaderHero, definition.Template);
            ApplyHeroTemplateBodyProperties(leaderHero, definition.Template);

            leaderHero.IsKnownToPlayer = true;

            BountyLogger.Log($"ResolvePatrolParty: created leader hero '{leaderHero.Name}' ({leaderHero.StringId}), clan='{clan.StringId}', patrolling near '{patrolCenter.Name}' (radius={definition.PatrolRadius}).");

            var partyName = new TextObject("{=BountyPatrolPartyName}{LEADER_NAME}'s Patrol")
                .SetTextVariable("LEADER_NAME", leaderHero.Name);

            var patrolParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(
                patrolCenter.GatePosition,
                1f,
                null,
                partyName,
                clan,
                TroopRoster.CreateDummyTroopRoster(),
                TroopRoster.CreateDummyTroopRoster(),
                null);

            var troop = MBObjectManager.Instance.GetObject<CharacterObject>(definition.TroopId);
            if (troop != null)
            {
                patrolParty.MemberRoster.AddToCounts(troop, definition.TroopCount);
                BountyLogger.Log($"ResolvePatrolParty: added {definition.TroopCount}x '{definition.TroopId}' to patrol roster.");
            }
            else
            {
                BountyLogger.Log($"ResolvePatrolParty: TroopId '{definition.TroopId}' did not resolve — patrol party has no rank-and-file troops.");
            }

            patrolParty.MemberRoster.AddToCounts(leaderHero.CharacterObject, 1, false, 0, 0, true, -1);
            AddHeroToPartyAction.Apply(leaderHero, patrolParty);
            patrolParty.ChangePartyLeader(leaderHero);
            patrolParty.ActualClan = clan;

            patrolParty.SetPartyUsedByQuest(true);

            patrolParty.SetMovePatrolAroundPoint(patrolCenter.GatePosition, MobileParty.NavigationType.Default);

            BountyLogger.Log($"ResolvePatrolParty: spawned '{patrolParty.StringId}' at '{patrolCenter.Name}' gate, leader='{leaderHero.Name}', roster total={patrolParty.MemberRoster.TotalManCount}, patrol radius={definition.PatrolRadius}.");

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);

            var bounty = new BountyTarget(leaderHero, value, guardPartySize: definition.TroopCount, expiryDays)
            {
                Description = definition.Description,
                FactionId = definition.BountyFactionId
                    ?? (string.Equals(definition.FactionId, "Bandit", StringComparison.OrdinalIgnoreCase) ? null : definition.FactionId),
                TargetSettlementId = patrolCenter.StringId,
                GangPartyId = patrolParty.StringId,
                IsPatrolBounty = true,
                PatrolRadius = definition.PatrolRadius
            };
            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolvePatrolParty: bounty created on {leaderHero.Name}, value={value}, expiryDays={expiryDays} — now capturable via the normal pipeline.");

            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {(string.IsNullOrEmpty(definition.Description) ? "A patrol has been spotted." : definition.Description)} ({leaderHero.Name}, near {patrolCenter.Name})"));
        }

        /// <summary>
        /// Creates a tavern bounty: a target hero and its accompanying thugs, ready to
        /// spawn in the settlement's tavern as soon as the player enters it.
        /// </summary>
        private void ResolveTwoStageSettlementBounty(BountyDefinition definition)
        {
            if (definition?.Template == null)
            {
                BountyLogger.Log($"ResolveTwoStageSettlementBounty: ABORT — definition '{definition?.Id}' has no HeroTemplate.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Tavern bounty definition missing or invalid."));
                return;
            }

            Settlement targetSettlement = ResolveDefinitionSettlement(definition, "ResolveTwoStageSettlementBounty", s => s.IsTown);
            if (targetSettlement == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No valid settlement configured for this bounty."));
                return;
            }

            Clan banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == targetSettlement.Culture && c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault(c => c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault();

            if (banditClan == null)
            {
                BountyLogger.Log("ResolveTwoStageSettlementBounty: ABORT — no bandit clan found.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No bandit clan found."));
                return;
            }

            var heroTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.Template.CharacterTemplateId);
            if (heroTemplate == null)
            {
                BountyLogger.Log($"ResolveTwoStageSettlementBounty: ABORT — CharacterTemplateId '{definition.Template.CharacterTemplateId}' did not resolve.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Could not resolve hero template."));
                return;
            }

            int age = MBRandom.RandomInt(definition.Template.MinAge, definition.Template.MaxAge);
            Hero targetHero = HeroCreator.CreateSpecialHero(heroTemplate, targetSettlement, banditClan, null, age);
            if (targetHero == null)
            {
                BountyLogger.Log("ResolveTwoStageSettlementBounty: ABORT — HeroCreator.CreateSpecialHero returned null.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Failed to create tavern bounty hero."));
                return;
            }

            var targetName = new TextObject(definition.Template.NameText ?? "Gang Leader");
            targetHero.SetName(targetName, targetName);
            targetHero.ChangeState(Hero.CharacterStates.Active);

            ApplyHeroTemplateEquipment(targetHero, definition.Template);
            ApplyHeroTemplateBodyProperties(targetHero, definition.Template);
            targetHero.IsKnownToPlayer = true;

            BountyLogger.Log($"ResolveTwoStageSettlementBounty: created target hero '{targetHero.Name}' ({targetHero.StringId}) at '{targetSettlement.Name}'.");

            var thugBaseTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.TroopId)
                                 ?? MBObjectManager.Instance.GetObject<CharacterObject>("looter");

            var thugIds = new MBList<string>();
            if (thugBaseTemplate != null)
            {
                for (int i = 0; i < definition.ThugCount; i++)
                {
                    var thug = CharacterObject.CreateFrom(thugBaseTemplate);
                    if (thug != null)
                    {
                        thugIds.Add(thug.StringId);
                    }
                }
                BountyLogger.Log($"ResolveTwoStageSettlementBounty: created {thugIds.Count}/{definition.ThugCount} distinct thug CharacterObjects from '{thugBaseTemplate.StringId}'.");
            }
            else
            {
                BountyLogger.Log($"ResolveTwoStageSettlementBounty: TroopId '{definition.TroopId}' (and fallback 'looter') both failed to resolve — no thugs will spawn in the tavern.");
            }

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);

            var bounty = new BountyTarget(targetHero, value, guardPartySize: definition.TroopCount, expiryDays)
            {
                TargetSettlementId = targetSettlement.StringId,
                IsTavernBounty = true,
                ThugCharacterIds = thugIds,
                Description = definition.Description,
                FactionId = targetSettlement.MapFaction?.StringId
            };
            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolveTwoStageSettlementBounty: bounty created on {targetHero.Name} at '{targetSettlement.Name}', value={value}, expiryDays={expiryDays}. Ready to spawn in the tavern immediately.");
            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {(string.IsNullOrEmpty(definition.Description) ? "A gang has taken hold of a settlement." : definition.Description)} ({value} gold, near {targetSettlement.Name})"));
        }

        /// <summary>
        /// Creates a stealth (hidden fugitive) bounty: a hero with no spawned party,
        /// anchored to a village, spawned as an unequipped LocationCharacter each time
        /// the player enters.
        /// </summary>
        private void ResolveStealthBounty(BountyDefinition definition)
        {
            if (definition?.Template == null)
            {
                BountyLogger.Log($"ResolveStealthBounty: ABORT — definition '{definition?.Id}' has no HeroTemplate.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Stealth bounty definition missing or invalid."));
                return;
            }

            Settlement targetSettlement = ResolveDefinitionSettlement(definition, "ResolveStealthBounty", s => s.IsVillage);
            if (targetSettlement == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No valid settlement configured for this bounty."));
                return;
            }

            Clan banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == targetSettlement.Culture && c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault(c => c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault();

            if (banditClan == null)
            {
                BountyLogger.Log("ResolveStealthBounty: ABORT — no bandit clan found at all.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No bandit clan found."));
                return;
            }

            var heroTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.Template.CharacterTemplateId);
            if (heroTemplate == null)
            {
                BountyLogger.Log($"ResolveStealthBounty: ABORT — CharacterTemplateId '{definition.Template.CharacterTemplateId}' did not resolve.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Could not resolve hero template."));
                return;
            }

            int age = MBRandom.RandomInt(definition.Template.MinAge, definition.Template.MaxAge);
            Hero leaderHero = HeroCreator.CreateSpecialHero(heroTemplate, targetSettlement, banditClan, null, age);
            if (leaderHero == null)
            {
                BountyLogger.Log("ResolveStealthBounty: ABORT — HeroCreator.CreateSpecialHero returned null.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Failed to create stealth bounty hero."));
                return;
            }

            var stealthName = new TextObject(definition.Template.NameText ?? "Hidden Fugitive");
            leaderHero.SetName(stealthName, stealthName);
            leaderHero.ChangeState(Hero.CharacterStates.Active);

            ApplyHeroTemplateEquipment(leaderHero, definition.Template);
            ApplyHeroTemplateBodyProperties(leaderHero, definition.Template);

            BountyLogger.Log($"ResolveStealthBounty: created stealth hero '{leaderHero.Name}' ({leaderHero.StringId}) at '{targetSettlement.Name}'.");

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);

            var bounty = new BountyTarget(leaderHero, value, guardPartySize: 0, expiryDays)
            {
                TargetSettlementId = targetSettlement.StringId,
                IsStealthBounty = true,
                Description = definition.Description,
                FactionId = targetSettlement.MapFaction?.StringId
            };

            leaderHero.IsKnownToPlayer = true;

            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolveStealthBounty: bounty created on {leaderHero.Name}, hiding in '{targetSettlement.Name}', value={value}, expiryDays={expiryDays}.");
            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {(string.IsNullOrEmpty(definition.Description) ? "Stealth bounty posted." : definition.Description)} ({value} gold, hiding in {targetSettlement.Name})"));
        }

        /// <summary>
        /// Spawns stealth and tavern bounty targets (and tavern thugs) as location
        /// characters once per mission instance, matching the player's current
        /// settlement and location.
        /// </summary>
        private void OnMissionStarted(IMission mission)
        {
            _stealthSpawnedThisMission.Clear();

            try
            {
                if (CampaignMission.Current == null || CampaignMission.Current.Location == null)
                    return;

                Settlement currentSettlement = PlayerEncounter.LocationEncounter?.Settlement;
                if (currentSettlement == null)
                    return;

                Location currentLocation = CampaignMission.Current.Location;

                foreach (var bounty in _activeBounties.Where(b =>
                             b.Status == BountyStatus.Active &&
                             b.IsStealthBounty &&
                             b.TargetSettlementId == currentSettlement.StringId).ToList())
                {
                    string spawnKey = $"{bounty.TrackerId}@{currentSettlement.StringId}:{currentLocation.StringId}";
                    if (_stealthSpawnedThisMission.Contains(spawnKey))
                        continue;

                    if (bounty.TargetHero?.CharacterObject == null)
                    {
                        BountyLogger.Log($"OnMissionStarted: stealth bounty for {bounty.TargetHero?.Name} has no resolvable CharacterObject — skipping spawn.");
                        continue;
                    }

                    LocationCharacter npc = CreateLocationCharacter(bounty.TargetHero.CharacterObject);
                    if (npc != null)
                    {
                        currentLocation.AddCharacter(npc);
                        _stealthSpawnedThisMission.Add(spawnKey);
                        BountyLogger.Log($"OnMissionStarted: spawned naked stealth target '{bounty.TargetHero.Name}' in '{currentSettlement.Name}' ({currentLocation.StringId}).");
                    }
                    else
                    {
                        BountyLogger.Log($"OnMissionStarted: CreateLocationCharacter returned null for {bounty.TargetHero.Name}.");
                    }
                }

                foreach (var bounty in _activeBounties.Where(b =>
                             b.Status == BountyStatus.Active &&
                             b.IsTavernBounty &&
                             b.TargetSettlementId == currentSettlement.StringId).ToList())
                {
                    if (!string.Equals(currentLocation.StringId, "tavern", StringComparison.OrdinalIgnoreCase))
                    {
                        BountyLogger.Log($"OnMissionStarted: tavern bounty for {bounty.TargetHero?.Name} is active, but current location is '{currentLocation.StringId}', not 'tavern' — not spawning here.");
                        continue;
                    }

                    string spawnKey = $"{bounty.TrackerId}@{currentSettlement.StringId}:{currentLocation.StringId}";
                    if (_stealthSpawnedThisMission.Contains(spawnKey))
                        continue;

                    if (bounty.TargetHero?.CharacterObject == null)
                    {
                        BountyLogger.Log($"OnMissionStarted: tavern bounty for {bounty.TargetHero?.Name} has no resolvable CharacterObject — skipping spawn.");
                        continue;
                    }

                    LocationCharacter targetNpc = CreateLocationCharacter(bounty.TargetHero.CharacterObject);
                    if (targetNpc != null)
                    {
                        currentLocation.AddCharacter(targetNpc);
                        _stealthSpawnedThisMission.Add(spawnKey);
                        BountyLogger.Log($"OnMissionStarted: spawned tavern bounty target '{bounty.TargetHero.Name}' in the tavern at '{currentSettlement.Name}'.");
                    }
                    else
                    {
                        BountyLogger.Log($"OnMissionStarted: CreateLocationCharacter returned null for tavern bounty target {bounty.TargetHero.Name}.");
                    }

                    int thugsSpawned = 0;
                    if (bounty.ThugCharacterIds != null)
                    {
                        foreach (var thugId in bounty.ThugCharacterIds)
                        {
                            var thugCharacter = MBObjectManager.Instance.GetObject<CharacterObject>(thugId);
                            if (thugCharacter == null)
                            {
                                BountyLogger.Log($"OnMissionStarted: thug CharacterObject '{thugId}' did not resolve — skipping this thug.");
                                continue;
                            }

                            LocationCharacter thugNpc = CreateLocationCharacter(thugCharacter);
                            if (thugNpc != null)
                            {
                                currentLocation.AddCharacter(thugNpc);
                                thugsSpawned++;
                            }
                        }
                    }

                    BountyLogger.Log($"OnMissionStarted: spawned {thugsSpawned}/{bounty.ThugCharacterIds?.Count ?? 0} thugs alongside {bounty.TargetHero.Name} in the tavern.");
                }
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"OnMissionStarted: EXCEPTION — {ex}");
            }
        }

        /// <summary>
        /// Builds a LocationCharacter for the given character template, ready to add
        /// to a mission's current location.
        /// </summary>
        private static LocationCharacter CreateLocationCharacter(CharacterObject character)
        {
            int minAge, maxAge;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(character, out minAge, out maxAge, "");

            Monster monster = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(character.Race, "_settlement");

            AgentData agentData = new AgentData(new SimpleAgentOrigin(character, -1, null, default))
                .Monster(monster)
                .Age(MBRandom.RandomInt(minAge, maxAge));

            return new LocationCharacter(
                agentData,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                "sp_lordshall_hero",
                true,
                LocationCharacter.CharacterRelations.Neutral,
                null,
                true,
                false,
                null,
                false,
                false,
                true
            );
        }

        /// <summary>
        /// Creates a gang-in-village bounty: a hero anchored to a village, whose gang
        /// party is spawned later on settlement entry.
        /// </summary>
        private void ResolveSettlementGangBounty(BountyDefinition definition)
        {
            if (definition?.Template == null)
            {
                BountyLogger.Log($"ResolveSettlementGangBounty: ABORT — definition '{definition?.Id}' has no HeroTemplate.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Gang bounty definition missing or invalid."));
                return;
            }

            Settlement targetSettlement = ResolveDefinitionSettlement(definition, "ResolveSettlementGangBounty", s => s.IsVillage);
            if (targetSettlement == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No valid settlement configured for this bounty."));
                return;
            }

            Clan banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == targetSettlement.Culture && c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault(c => c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault();

            if (banditClan == null)
            {
                BountyLogger.Log("ResolveSettlementGangBounty: ABORT — no bandit clan found at all.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] No bandit clan found."));
                return;
            }

            var heroTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(definition.Template.CharacterTemplateId);
            if (heroTemplate == null)
            {
                BountyLogger.Log($"ResolveSettlementGangBounty: ABORT — CharacterTemplateId '{definition.Template.CharacterTemplateId}' did not resolve.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Could not resolve hero template."));
                return;
            }

            int age = MBRandom.RandomInt(definition.Template.MinAge, definition.Template.MaxAge);
            Hero leaderHero = HeroCreator.CreateSpecialHero(heroTemplate, targetSettlement, banditClan, null, age);
            if (leaderHero == null)
            {
                BountyLogger.Log("ResolveSettlementGangBounty: ABORT — HeroCreator.CreateSpecialHero returned null.");
                InformationManager.DisplayMessage(new InformationMessage("[BountyHunting] Failed to create gang leader hero."));
                return;
            }

            var gangLeaderName = new TextObject(definition.Template.NameText ?? "{CLAN_NAME} Gang Leader")
                .SetTextVariable("CLAN_NAME", banditClan.Name);
            leaderHero.SetName(gangLeaderName, gangLeaderName);
            leaderHero.ChangeState(Hero.CharacterStates.Active);

            ApplyHeroTemplateEquipment(leaderHero, definition.Template);
            ApplyHeroTemplateBodyProperties(leaderHero, definition.Template);

            BountyLogger.Log($"ResolveSettlementGangBounty: created gang leader hero '{leaderHero.Name}' ({leaderHero.StringId}) at '{targetSettlement.Name}'.");

            int value = MBRandom.RandomInt(definition.MinValue, definition.MaxValue);
            int expiryDays = MBRandom.RandomInt(definition.MinExpiryDays, definition.MaxExpiryDays);
            const int gangTroopCount = 12;

            var bounty = new BountyTarget(leaderHero, value, guardPartySize: gangTroopCount, expiryDays)
            {
                TargetSettlementId = targetSettlement.StringId,
                Description = definition.Description,
                FactionId = targetSettlement.MapFaction?.StringId
            };

            leaderHero.IsKnownToPlayer = true;

            _activeBounties.Add(bounty);

            BountyLogger.Log($"ResolveSettlementGangBounty: bounty created on {leaderHero.Name}, anchored to '{targetSettlement.Name}' (party NOT yet spawned), value={value}, expiryDays={expiryDays}.");
            InformationManager.DisplayMessage(new InformationMessage(
                $"[BountyHunting] {(string.IsNullOrEmpty(definition.Description) ? "Gang bounty posted." : definition.Description)} ({leaderHero.Name}, {value} gold, near {targetSettlement.Name})"));
        }

        /// <summary>
        /// Spawns the gang bounty's party at the target settlement's gate, if it
        /// doesn't already have an active one.
        /// </summary>
        private void EnsureGangPartySpawnedNow(BountyTarget bounty, Settlement settlement)
        {
            if (!string.IsNullOrEmpty(bounty.GangPartyId))
            {
                var existing = FindMobilePartyByStringId(bounty.GangPartyId);
                if (existing != null && existing.IsActive)
                {
                    BountyLogger.Log($"EnsureGangPartySpawnedNow: bounty for {bounty.TargetHero?.Name} already has an active party '{bounty.GangPartyId}' — skipping spawn.");
                    return;
                }
            }

            Clan banditClan = Clan.BanditFactions.FirstOrDefault(c => c.Culture == settlement.Culture && c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault(c => c.StringId != "looters")
                            ?? Clan.BanditFactions.FirstOrDefault();

            if (banditClan == null)
            {
                BountyLogger.Log($"EnsureGangPartySpawnedNow: ABORT for {bounty.TargetHero?.Name} — no bandit clan found at spawn time.");
                return;
            }

            var partyName = new TextObject("{=BountyGangPartyName}{LEADER_NAME}'s Gang")
                .SetTextVariable("LEADER_NAME", bounty.TargetHero?.Name);

            var gangParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(
                settlement.GatePosition,
                1f,
                null,
                partyName,
                banditClan,
                TroopRoster.CreateDummyTroopRoster(),
                TroopRoster.CreateDummyTroopRoster(),
                null);

            BountyLogger.Log($"EnsureGangPartySpawnedNow: spawned gang party '{gangParty.StringId}' at '{settlement.Name}' gate for {bounty.TargetHero?.Name}.");

            const string troopId = "looter";
            int troopCount = bounty.GuardPartySize > 0 ? bounty.GuardPartySize : 12;
            var troop = MBObjectManager.Instance.GetObject<CharacterObject>(troopId);
            if (troop != null)
            {
                gangParty.MemberRoster.AddToCounts(troop, troopCount);
                BountyLogger.Log($"EnsureGangPartySpawnedNow: added {troopCount}x '{troopId}' to gang roster.");
            }
            else
            {
                BountyLogger.Log($"EnsureGangPartySpawnedNow: troopId '{troopId}' did not resolve — gang party has no troops.");
            }

            if (bounty.TargetHero != null)
            {
                gangParty.MemberRoster.AddToCounts(bounty.TargetHero.CharacterObject, 1, false, 0, 0, true, -1);
                AddHeroToPartyAction.Apply(bounty.TargetHero, gangParty);
                gangParty.ChangePartyLeader(bounty.TargetHero);
                gangParty.ActualClan = banditClan;

                BountyLogger.Log($"EnsureGangPartySpawnedNow: {bounty.TargetHero.Name} assigned as leader of gang party '{gangParty.StringId}'. " +
                                  $"party.MapFaction='{gangParty.MapFaction?.StringId ?? "NULL"}', " +
                                  $"party.ActualClan='{gangParty.ActualClan?.StringId ?? "NULL"}'.");
            }
            else
            {
                BountyLogger.Log("EnsureGangPartySpawnedNow: bounty.TargetHero is null — gang party spawned with no leader.");
            }

            gangParty.SetPartyUsedByQuest(true);
            gangParty.Ai.DisableAi();
            BountyLogger.Log($"EnsureGangPartySpawnedNow: called SetPartyUsedByQuest(true) and Ai.DisableAi() on '{gangParty.StringId}'.");

            BountyLogger.Log($"EnsureGangPartySpawnedNow: FINAL roster check — MemberRoster.TotalManCount={gangParty.MemberRoster.TotalManCount}, TotalHeroes={gangParty.MemberRoster.TotalHeroes}, TotalRegulars={gangParty.MemberRoster.TotalRegulars}, party.LeaderHero='{gangParty.LeaderHero?.Name}'.");

            bounty.GangPartyId = gangParty.StringId;
        }

        /// <summary>
        /// Spawns any gang party whose target settlement the player has just entered.
        /// Also rolls a chance for each pending hideout-boss bounty to resolve now,
        /// using this settlement as the reference position for "nearest hideout" —
        /// a failed roll leaves it queued to try again on the next settlement
        /// entered, rather than always resolving on the very first one.
        /// </summary>
        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (hero != Hero.MainHero || party != MobileParty.MainParty || settlement == null) return;

            foreach (var bounty in _activeBounties.Where(b =>
                         b.Status == BountyStatus.Active &&
                         b.IsSettlementAnchored &&
                         !b.IsStealthBounty &&
                         !b.IsHideoutBounty &&
                         !b.IsTavernBounty &&
                         !b.IsPatrolBounty &&
                         b.TargetSettlementId == settlement.StringId).ToList())
            {
                EnsureGangPartySpawnedNow(bounty, settlement);
            }

            if (_pendingHideoutDefinitionIds.Count > 0)
            {
                foreach (var definitionId in _pendingHideoutDefinitionIds.ToList())
                {
                    var definition = BountyDefinitionRegistry.GetById(definitionId);
                    if (definition == null)
                    {
                        BountyLogger.Log($"OnSettlementEntered: pending hideout definition '{definitionId}' no longer resolves in the registry — dropping it from the queue.");
                        _pendingHideoutDefinitionIds.Remove(definitionId);
                        continue;
                    }

                    if (MBRandom.RandomFloat > HideoutResolutionChancePerSettlementEntry)
                    {
                        BountyLogger.Log($"OnSettlementEntered: chance roll failed for pending hideout definition '{definitionId}' at '{settlement.Name}' — still queued, will retry on the next settlement entered.");
                        continue;
                    }

                    ResolveHideoutBossBountyAt(definition, settlement.GatePosition, $"entered settlement '{settlement.Name}' ({settlement.StringId})");
                    _pendingHideoutDefinitionIds.Remove(definitionId);
                }
            }
        }

        /// <summary>
        /// Despawns any unresolved gang party for the settlement the player is
        /// leaving, so it doesn't linger exposed on the map.
        /// </summary>
        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party?.LeaderHero != Hero.MainHero || settlement == null) return;

            foreach (var bounty in _activeBounties.Where(b =>
                         b.Status == BountyStatus.Active &&
                         b.IsSettlementAnchored &&
                         !b.IsStealthBounty &&
                         !b.IsPatrolBounty &&
                         b.TargetSettlementId == settlement.StringId &&
                         !string.IsNullOrEmpty(b.GangPartyId)).ToList())
            {
                if (_gangPartiesEngagingBattle.Contains(bounty.GangPartyId))
                {
                    BountyLogger.Log($"OnSettlementLeft: SKIPPED despawn for '{bounty.GangPartyId}' ({bounty.TargetHero?.Name}) — battle currently being initiated against this party.");
                    continue;
                }

                var gangParty = FindMobilePartyByStringId(bounty.GangPartyId);
                if (gangParty != null && gangParty.IsActive)
                {
                    BountyLogger.Log($"OnSettlementLeft: despawning unresolved gang party '{bounty.GangPartyId}' for {bounty.TargetHero?.Name} as player leaves '{settlement.Name}'.");
                    _gangPartiesBeingCleanedUp.Add(bounty.GangPartyId);
                    DestroyPartyAction.Apply(null, gangParty);
                }
                bounty.GangPartyId = null;
            }
        }

        /// <summary>
        /// Looks up a live MobileParty by StringId among all current campaign
        /// parties.
        /// </summary>
        private static MobileParty FindMobilePartyByStringId(string stringId)
        {
            if (string.IsNullOrEmpty(stringId)) return null;

            var list = Campaign.Current?.MobileParties;
            if (list != null)
                return list.FirstOrDefault(p => p != null && p.StringId == stringId);

            return null;
        }

        /// <summary>
        /// Returns every bounty currently captured and awaiting turn-in.
        /// </summary>
        public IReadOnlyList<BountyTarget> GetCapturedAwaitingTurnIn()
        {
            return _activeBounties.Where(b => b.Status == BountyStatus.Captured).ToList();
        }

        /// <summary>
        /// Looks up a bounty by its tracker id.
        /// </summary>
        public BountyTarget FindBountyByTrackerId(string trackerId)
        {
            if (string.IsNullOrEmpty(trackerId)) return null;
            return _activeBounties.FirstOrDefault(b => b.TrackerId == trackerId);
        }

        /// <summary>
        /// Returns whether the given hero currently has an active bounty on them.
        /// </summary>
        public bool HasActiveBountyOn(Hero hero)
        {
            if (hero == null) return false;
            return _activeBounties.Any(b => b.Status == BountyStatus.Active && b.TargetHero == hero);
        }

        /// <summary>
        /// Returns and removes the next hero pending a death-veto-triggered forced
        /// capture, or null if none are pending or a bounty conversation is already
        /// in progress.
        /// </summary>
        public Hero TakeNextPendingDeathVetoCapture()
        {
            if (_bountyConversationInProgress)
            {
                return null;
            }

            foreach (var hero in _pendingForcedCaptures.ToList())
            {
                bool hasActiveBounty = _activeBounties.Any(b => b.Status == BountyStatus.Active && b.TargetHero == hero);
                if (!hasActiveBounty)
                {
                    _pendingForcedCaptures.Remove(hero);
                    continue;
                }

                _pendingForcedCaptures.Remove(hero);
                return hero;
            }

            return null;
        }

        /// <summary>
        /// Force-captures a bounty target if not already a prisoner, then opens the
        /// mod's own captured-lord conversation in place of vanilla's.
        /// </summary>
        public void HandleBountyTargetCapturedInBattle(Hero hero)
        {
            if (hero == null) return;

            BountyLogger.Log($"[Harmony/DoCaptureHeroes] Intercepting vanilla captured-lord dialogue for bounty target {hero.Name}. IsPrisoner={hero.IsPrisoner}");

            if (!hero.IsPrisoner)
            {
                try
                {
                    ForceCaptureHero(hero);
                }
                catch (Exception ex)
                {
                    BountyLogger.Log($"[Harmony/DoCaptureHeroes] ForceCaptureHero THREW for {hero.Name}: {ex}");
                }
            }
            else
            {
                BountyLogger.Log($"[Harmony/DoCaptureHeroes] {hero.Name} was already a prisoner — vanilla capture handled it, just showing dialogue.");
            }

            _pendingBountyDialogueHero = hero;
            _bountyConversationInProgress = true;

            try
            {
                CampaignMapConversation.OpenConversation(
                    new ConversationCharacterData(Hero.MainHero.CharacterObject, null, false, false, false, false, false, false),
                    new ConversationCharacterData(hero.CharacterObject, null, false, false, false, false, false, false));
                BountyLogger.Log($"[Harmony/DoCaptureHeroes] OpenConversation called successfully for {hero.Name}.");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"[Harmony/DoCaptureHeroes] OpenConversation THREW for {hero.Name}: {ex}");
                _bountyConversationInProgress = false;
                return;
            }

            Campaign.Current.ConversationManager.ConversationEndOneShot += delegate ()
            {
                _bountyConversationInProgress = false;
                BountyLogger.Log($"[Harmony/DoCaptureHeroes] Bounty conversation with {hero.Name} ended — clear to process next capture.");
            };
        }

        /// <summary>
        /// Processes battle captures at the earliest point after a player battle
        /// ends.
        /// </summary>
        private void OnPlayerBattleEnd(MapEvent mapEvent)
        {
            BountyLogger.Log($"OnPlayerBattleEnd fired. WinningSide={mapEvent.WinningSide}, DefeatedSide={mapEvent.DefeatedSide}");
            ProcessBattleCaptures(mapEvent, "OnPlayerBattleEnd");
        }

        /// <summary>
        /// Backstop battle-capture handling for any map event not already covered by
        /// OnPlayerBattleEnd or the DoCaptureHeroes patch, plus hideout and gang
        /// bounty resolution.
        /// </summary>
        private void OnMapEventEnded(MapEvent mapEvent)
        {
            BountyLogger.Log($"OnMapEventEnded fired. WinningSide={mapEvent.WinningSide}, DefeatedSide={mapEvent.DefeatedSide}");

            if (mapEvent.IsHideoutBattle && mapEvent.MapEventSettlement != null)
            {
                ResolveHideoutBountyOutcome(mapEvent);
            }

            ProcessGangBountyOutcome(mapEvent);

            ProcessBattleCaptures(mapEvent, "OnMapEventEnded");
        }

        /// <summary>
        /// Force-captures a gang bounty's target if their party was on the defeated
        /// side of a battle the player won, matching by party id rather than hero
        /// leadership.
        /// </summary>
        private void ProcessGangBountyOutcome(MapEvent mapEvent)
        {
            if (mapEvent.WinningSide == BattleSideEnum.None || mapEvent.DefeatedSide == BattleSideEnum.None)
                return;

            var winningSide = mapEvent.GetMapEventSide(mapEvent.WinningSide);
            if (!winningSide.IsMainPartyAmongParties())
                return;

            var defeatedSide = mapEvent.GetMapEventSide(mapEvent.DefeatedSide);
            var defeatedPartyIds = defeatedSide.Parties
                .Select(p => p.Party?.MobileParty?.StringId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            foreach (var bounty in _activeBounties.Where(b =>
                         b.Status == BountyStatus.Active &&
                         !b.IsTavernBounty &&
                         !b.IsHideoutBounty &&
                         !string.IsNullOrEmpty(b.GangPartyId) &&
                         defeatedPartyIds.Contains(b.GangPartyId)).ToList())
            {
                BountyLogger.Log($"ProcessGangBountyOutcome: party '{bounty.GangPartyId}' for {bounty.TargetHero?.Name} was on the defeated side and player won — capturing directly (party-ID match, bypassing LeaderHero).");

                if (bounty.TargetHero != null && !bounty.TargetHero.IsPrisoner)
                {
                    ForceCaptureHero(bounty.TargetHero);
                }

                bounty.Status = BountyStatus.Captured;
            }
        }

        /// <summary>
        /// Resolves a hideout-boss bounty's outcome when the hideout's own battle
        /// ends, capturing the target on a win or removing the bounty on a loss.
        /// </summary>
        private void ResolveHideoutBountyOutcome(MapEvent mapEvent)
        {
            string settlementId = mapEvent.MapEventSettlement.StringId;

            var bounty = _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                b.IsHideoutBounty &&
                b.TargetSettlementId == settlementId);

            if (bounty == null) return;

            bool playerWon = mapEvent.WinningSide != BattleSideEnum.None
                           && mapEvent.GetMapEventSide(mapEvent.WinningSide).IsMainPartyAmongParties();

            if (playerWon)
            {
                BountyLogger.Log($"ResolveHideoutBountyOutcome: player WON the hideout raid at '{settlementId}' — capturing '{bounty.TargetHero.Name}'.");

                if (!bounty.TargetHero.IsPrisoner)
                {
                    ForceCaptureHero(bounty.TargetHero);
                }

                bounty.Status = BountyStatus.Captured;

                var remainingParty = FindMobilePartyByStringId(bounty.GangPartyId);
                if (remainingParty != null && remainingParty.IsActive)
                {
                    if (!string.IsNullOrEmpty(bounty.GangPartyId))
                    {
                        _gangPartiesBeingCleanedUp.Add(bounty.GangPartyId);
                    }
                    DestroyPartyAction.Apply(null, remainingParty);
                    BountyLogger.Log($"ResolveHideoutBountyOutcome: despawned remaining party '{bounty.GangPartyId}' after capture.");
                }
            }
            else
            {
                BountyLogger.Log($"ResolveHideoutBountyOutcome: player LOST the hideout raid at '{settlementId}' — '{bounty.TargetHero.Name}' disappears, bounty removed.");

                InformationManager.DisplayMessage(new InformationMessage(
                    $"[BountyHunting] The bounty on {bounty.TargetHero?.Name} has been cancelled."));

                var party = FindMobilePartyByStringId(bounty.GangPartyId);
                if (party != null && party.IsActive)
                {
                    if (!string.IsNullOrEmpty(bounty.GangPartyId))
                    {
                        _gangPartiesBeingCleanedUp.Add(bounty.GangPartyId);
                    }
                    DestroyPartyAction.Apply(null, party);
                }

                _activeBounties.Remove(bounty);
            }
        }

        /// <summary>
        /// Captures a hideout-boss bounty's target when its hideout is cleared by any
        /// means, including auto-resolved "Send Troops".
        /// </summary>
        private void OnHideoutDeactivated(Settlement settlement)
        {
            if (settlement == null) return;

            var bounty = _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                b.IsHideoutBounty &&
                b.TargetSettlementId == settlement.StringId);

            if (bounty == null) return;

            BountyLogger.Log($"OnHideoutDeactivated: hideout '{settlement.StringId}' cleared — capturing hideout bounty target '{bounty.TargetHero?.Name}' (covers Send Troops and personal-raid resolution alike).");

            if (bounty.TargetHero != null && !bounty.TargetHero.IsPrisoner)
            {
                ForceCaptureHero(bounty.TargetHero);
            }

            bounty.Status = BountyStatus.Captured;

            var party = FindMobilePartyByStringId(bounty.GangPartyId);
            if (party != null && party.IsActive)
            {
                if (!string.IsNullOrEmpty(bounty.GangPartyId))
                {
                    _gangPartiesBeingCleanedUp.Add(bounty.GangPartyId);
                }
                DestroyPartyAction.Apply(null, party);
                BountyLogger.Log($"OnHideoutDeactivated: despawned lingering hideout bounty party '{bounty.GangPartyId}'.");
            }
        }

        /// <summary>
        /// Re-issues SetMovePatrolAroundPoint for every active PatrolParty bounty's
        /// live party once per in-game hour, aiming at a freshly randomized point
        /// within the bounty's PatrolRadius each time — not the same fixed gate
        /// position repeatedly. AiPatrollingBehaviorSafetyPatch swallows an exception
        /// thrown by AiHourlyTick every single hour for this non-PatrolPartyComponent
        /// party, so nothing else is picking new patrol waypoints on this party's
        /// behalf; re-issuing the same static point each time (the earlier version
        /// of this method) produced no actual wandering — confirmed directly:
        /// observed as the party moving briefly then snapping back to the same spot
        /// every hour, since there was nothing different to move toward.
        /// </summary>
        private void OnHourlyTick()
        {
            foreach (var bounty in _activeBounties.Where(b => b.Status == BountyStatus.Active && b.IsPatrolBounty))
            {
                var party = FindMobilePartyByStringId(bounty.GangPartyId);
                if (party == null || !party.IsActive)
                {
                    continue;
                }

                var patrolCenter = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
                if (patrolCenter == null)
                {
                    BountyLogger.Log($"OnHourlyTick: patrol bounty on '{bounty.TargetHero?.Name}' has TargetSettlementId '{bounty.TargetSettlementId}' that no longer resolves — skipping re-issue this hour.");
                    continue;
                }

                float radius = bounty.PatrolRadius > 0f ? bounty.PatrolRadius : 15f;
                float angle = MBRandom.RandomFloat * (float)(Math.PI * 2.0);
                float distance = MBRandom.RandomFloat * radius;
                float offsetX = (float)Math.Cos(angle) * distance;
                float offsetY = (float)Math.Sin(angle) * distance;

                var centerPos = patrolCenter.GatePosition;
                var randomPatrolPoint = new CampaignVec2(new Vec2(centerPos.X + offsetX, centerPos.Y + offsetY), true);

                party.SetMovePatrolAroundPoint(randomPatrolPoint, MobileParty.NavigationType.Default);

                BountyLogger.Log($"OnHourlyTick: re-issued patrol point for '{bounty.TargetHero?.Name}' near '{patrolCenter.Name}' — offset ({offsetX:0.0}, {offsetY:0.0}) within radius {radius}.");
            }
        }

        /// <summary>
        /// Force-captures every active bounty target found on the defeated side of a
        /// won battle, or queued via a death veto, and opens the capture dialogue for
        /// each.
        /// </summary>
        private void ProcessBattleCaptures(MapEvent mapEvent, string source)
        {
            if (mapEvent.WinningSide == BattleSideEnum.None || mapEvent.DefeatedSide == BattleSideEnum.None)
            {
                BountyLogger.Log($"{source}: no clear winner/defeated side — skipping.");
                _pendingForcedCaptures.Clear();
                return;
            }

            var winningSide = mapEvent.GetMapEventSide(mapEvent.WinningSide);
            bool mainPartyWon = winningSide.IsMainPartyAmongParties();
            BountyLogger.Log($"{source}: MainPartyAmongWinners={mainPartyWon}");

            if (!mainPartyWon)
            {
                BountyLogger.Log($"{source}: player was not among the winning side — skipping capture logic.");
                _pendingForcedCaptures.Clear();
                return;
            }

            var defeatedSide = mapEvent.GetMapEventSide(mapEvent.DefeatedSide);
            BountyLogger.Log($"{source}: defeated side has {defeatedSide.Parties.Count} parties.");

            foreach (var p in defeatedSide.Parties)
            {
                BountyLogger.Log($"  Defeated party: {p.Party.Name}, LeaderHero={p.Party.MobileParty?.LeaderHero?.Name?.ToString() ?? "none"}");
            }

            var heroesNeedingForceCapture = new HashSet<Hero>();
            var heroesNeedingDialogue = new HashSet<Hero>();

            foreach (var bounty in _activeBounties.Where(b => b.Status == BountyStatus.Active).ToList())
            {
                var hero = bounty.TargetHero;
                BountyLogger.Log($"Checking bounty {hero?.Name}: IsPrisoner={hero?.IsPrisoner}");

                if (hero == null) continue;

                bool wasOnDefeatedSide = defeatedSide.Parties.Any(p => p.Party.MobileParty?.LeaderHero == hero);
                BountyLogger.Log($"  wasOnDefeatedSide for {hero.Name} = {wasOnDefeatedSide}");

                if (!wasOnDefeatedSide) continue;

                heroesNeedingDialogue.Add(hero);
                if (!hero.IsPrisoner)
                {
                    heroesNeedingForceCapture.Add(hero);
                }
            }

            foreach (var hero in _pendingForcedCaptures)
            {
                bool hasActiveBounty = _activeBounties.Any(b => b.Status == BountyStatus.Active && b.TargetHero == hero);
                if (!hasActiveBounty) continue;

                BountyLogger.Log($"  {hero.Name} queued via death-veto (IsPrisoner={hero.IsPrisoner}).");
                heroesNeedingDialogue.Add(hero);
                if (!hero.IsPrisoner)
                {
                    heroesNeedingForceCapture.Add(hero);
                }
            }

            foreach (var hero in heroesNeedingForceCapture)
            {
                BountyLogger.Log($"{source}: forcing bounty target {hero.Name} into prison roster.");

                try
                {
                    ForceCaptureHero(hero);
                }
                catch (Exception ex)
                {
                    BountyLogger.Log($"ForceCaptureHero THREW an exception for {hero.Name}: {ex}");
                }
            }

            foreach (var hero in heroesNeedingDialogue)
            {
                _pendingBountyDialogueHero = hero;

                try
                {
                    CampaignMapConversation.OpenConversation(
                        new ConversationCharacterData(Hero.MainHero.CharacterObject, null, false, false, false, false, false, false),
                        new ConversationCharacterData(hero.CharacterObject, null, false, false, false, false, false, false));
                    BountyLogger.Log($"OpenConversation called successfully for {hero.Name}.");
                }
                catch (Exception ex)
                {
                    BountyLogger.Log($"OpenConversation THREW an exception for {hero.Name}: {ex}");
                }
            }

            _pendingForcedCaptures.Clear();
        }

        /// <summary>
        /// Manually removes a hero from their party and places them in the player's
        /// prison roster, firing the vanilla prisoner-taken event.
        /// </summary>
        private void ForceCaptureHero(Hero hero)
        {
            if (hero.PartyBelongedTo != null)
            {
                if (hero.PartyBelongedTo.LeaderHero == hero)
                {
                    hero.PartyBelongedTo.RemovePartyLeader();
                }
                hero.PartyBelongedTo.MemberRoster.RemoveTroop(hero.CharacterObject, 1, default, 0);
            }

            hero.CaptivityStartTime = CampaignTime.Now;
            hero.ChangeState(Hero.CharacterStates.Prisoner);

            if (MobileParty.MainParty.Party.PrisonRoster != null)
            {
                MobileParty.MainParty.Party.PrisonRoster.AddToCounts(hero.CharacterObject, 1);
            }

            BountyLogger.Log($"ForceCaptureHero: {hero.Name} manually forced into MainParty prison roster. IsPrisoner now = {hero.IsPrisoner}");

            CampaignEventDispatcher.Instance.OnHeroPrisonerTaken(PartyBase.MainParty, hero);
        }

        /// <summary>
        /// Captures a stealth or tavern bounty target defeated in an in-mission
        /// fight, and queues the capture dialogue.
        /// </summary>
        internal void ForceCaptureHeroFromStealthFight(Hero hero)
        {
            if (hero == null) return;

            ForceCaptureHero(hero);
            _pendingBountyDialogueHero = hero;

            BountyLogger.Log($"ForceCaptureHeroFromStealthFight: {hero.Name} captured, dialogue queued via _pendingBountyDialogueHero.");
        }

        /// <summary>
        /// Removes a stealth or tavern bounty when the player is defeated in its
        /// fight.
        /// </summary>
        internal void RemoveStealthBountyOnPlayerDefeat(BountyTarget bounty)
        {
            if (bounty == null) return;
            RemoveUnresolvableBounty(bounty, "you were defeated.");
        }

        /// <summary>
        /// Registers the mod's custom conversation lines: captured-bounty dialogue,
        /// stealth bounty dialogue, and tavern bounty dialogue.
        /// </summary>
        private void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddDialogLine(
                "bh_bounty_captured_start",
                "start",
                "bh_bounty_captured_response",
                "{=BountyCapturedLine}So, you've caught me. Don't think this is over.",
                () => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == _pendingBountyDialogueHero,
                null);

            starter.AddPlayerLine(
                "bh_bounty_captured_player_response",
                "bh_bounty_captured_response",
                "close_window",
                "{=BountyPlayerResponse}You're worth a fair sum. Try to run, and it won't end well for you.",
                null,
                () => { _pendingBountyDialogueHero = null; });

            starter.AddDialogLine(
                "bh_stealth_bounty_start",
                "start",
                "bh_stealth_bounty_response",
                "{=BountyStealthLine}Please, don't hurt me. I'm... I'm not who you think I am.",
                () => GetActiveStealthBountyForConversation() != null,
                null);

            starter.AddPlayerLine(
                "bh_stealth_bounty_attack",
                "bh_stealth_bounty_response",
                "close_window",
                "{=BountyStealthAttack}You're exactly who I think you are. Draw your weapon.",
                null,
                ExecuteStealthBountyAttack);

            starter.AddPlayerLine(
                "bh_stealth_bounty_leave",
                "bh_stealth_bounty_response",
                "close_window",
                "{=BountyStealthLeave}...Never mind. Carry on.",
                null,
                null);

            starter.AddDialogLine(
                "bh_twostage_bounty_start",
                "start",
                "bh_twostage_bounty_response",
                "{=BountyTwoStageLine}You've got no business here. Turn around while you still can.",
                () => GetActiveTwoStageBountyForConversation() != null,
                null);

            starter.AddPlayerLine(
                "bh_twostage_bounty_attack",
                "bh_twostage_bounty_response",
                "close_window",
                "{=BountyTwoStageAttack}Your reign here is over. Draw your weapon.",
                null,
                ExecuteTwoStageTavernAttack);

            starter.AddPlayerLine(
                "bh_twostage_bounty_leave",
                "bh_twostage_bounty_response",
                "close_window",
                "{=BountyTwoStageLeave}...Not yet. I'll return.",
                null,
                null);
        }

        /// <summary>
        /// Returns the active stealth bounty matching the current conversation hero,
        /// if any.
        /// </summary>
        private BountyTarget GetActiveStealthBountyForConversation()
        {
            var conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero == null) return null;

            return _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                b.IsStealthBounty &&
                b.TargetHero == conversationHero);
        }

        /// <summary>
        /// Returns the active tavern bounty matching the current conversation hero,
        /// if any.
        /// </summary>
        private BountyTarget GetActiveTwoStageBountyForConversation()
        {
            var conversationHero = Hero.OneToOneConversationHero;
            if (conversationHero == null) return null;

            return _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                b.IsTavernBounty &&
                b.TargetHero == conversationHero);
        }

        /// <summary>
        /// Flips the current mission into combat mode against the stealth bounty
        /// target, reassigning their team, setting mutual hostility, and alarming
        /// them.
        /// </summary>
        private void ExecuteStealthBountyAttack()
        {
            var bounty = GetActiveStealthBountyForConversation();
            if (bounty?.TargetHero?.CharacterObject == null)
            {
                BountyLogger.Log("ExecuteStealthBountyAttack: no matching stealth bounty found for current conversation.");
                return;
            }

            try
            {
                if (Mission.Current == null)
                {
                    BountyLogger.Log("ExecuteStealthBountyAttack: ABORT — Mission.Current is null.");
                    return;
                }

                Agent targetAgent = Mission.Current.Agents.FirstOrDefault(a => a.Character == bounty.TargetHero.CharacterObject);
                if (targetAgent == null)
                {
                    BountyLogger.Log($"ExecuteStealthBountyAttack: ABORT — could not find an Agent for {bounty.TargetHero.Name} in the current mission.");
                    return;
                }

                BountyLogger.Log($"ExecuteStealthBountyAttack: flipping the CURRENT mission into combat mode against {bounty.TargetHero.Name} — no new mission opened, no scene change.");

                Mission.Current.SetMissionMode(MissionMode.Battle, false);

                var playerTeam = Mission.Current.PlayerTeam;

                if (Mission.Current.DefenderTeam != null && Mission.Current.DefenderTeam.IsValid)
                {
                    targetAgent.SetTeam(Mission.Current.DefenderTeam, true);
                    BountyLogger.Log("ExecuteStealthBountyAttack: reassigned target agent to Mission.Current.DefenderTeam.");
                }
                else
                {
                    BountyLogger.Log("ExecuteStealthBountyAttack: Mission.Current.DefenderTeam is null or invalid — could not reassign.");
                }

                var targetTeam = targetAgent.Team;

                BountyLogger.Log($"ExecuteStealthBountyAttack: playerTeam null={playerTeam == null}, IsValid={playerTeam?.IsValid}; targetTeam null={targetTeam == null}, IsValid={targetTeam?.IsValid}; same team={playerTeam == targetTeam}.");

                if (playerTeam != null && targetTeam != null && playerTeam != targetTeam
                    && playerTeam.IsValid && targetTeam.IsValid)
                {
                    playerTeam.SetIsEnemyOf(targetTeam, true);
                    targetTeam.SetIsEnemyOf(playerTeam, true);
                    BountyLogger.Log($"ExecuteStealthBountyAttack: set mutual enemy relationship between player team and {bounty.TargetHero.Name}'s team.");
                }
                else
                {
                    BountyLogger.Log($"ExecuteStealthBountyAttack: could NOT set enemy relationship — see validity flags logged above.");
                }

                AgentFlag agentFlags = targetAgent.GetAgentFlags();
                targetAgent.SetAgentFlags(agentFlags | AgentFlag.CanGetAlarmed);

                AlarmedBehaviorGroup.AlarmAgent(targetAgent);
                BountyLogger.Log("ExecuteStealthBountyAttack: AlarmedBehaviorGroup.AlarmAgent(targetAgent) called.");

                targetAgent.SetAlarmState(Agent.AIStateFlag.Alarmed);

                Mission.Current.AddMissionBehavior(new StealthBountyFightOutcomeBehavior(this, bounty, targetAgent));
                BountyLogger.Log("ExecuteStealthBountyAttack: StealthBountyFightOutcomeBehavior registered.");

                BountyLogger.Log("ExecuteStealthBountyAttack: mission mode set to Battle and target agent alarmed successfully.");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"ExecuteStealthBountyAttack: EXCEPTION — {ex}");
            }
        }

        /// <summary>
        /// Mission behavior that watches for the stealth bounty fight's outcome,
        /// capturing the target or removing the bounty depending on who goes down.
        /// </summary>
        private class StealthBountyFightOutcomeBehavior : MissionBehavior
        {
            private readonly BountyHunterBehavior _owner;
            private readonly BountyTarget _bounty;
            private readonly Agent _targetAgent;
            private bool _resolved;

            /// <summary>
            /// Creates the outcome watcher for a given owner, bounty, and target
            /// agent.
            /// </summary>
            public StealthBountyFightOutcomeBehavior(BountyHunterBehavior owner, BountyTarget bounty, Agent targetAgent)
            {
                _owner = owner;
                _bounty = bounty;
                _targetAgent = targetAgent;
            }

            public override MissionBehaviorType BehaviorType => MissionBehaviorType.Logic;

            /// <summary>
            /// Captures the target if they go down, or removes the bounty if the
            /// player does.
            /// </summary>
            public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
            {
                if (_resolved) return;

                if (affectedAgent == _targetAgent)
                {
                    _resolved = true;
                    BountyLogger.Log($"StealthBountyFightOutcomeBehavior: target agent for {_bounty.TargetHero?.Name} removed (agentState={agentState}) — treating as defeated, capturing.");

                    _owner.ForceCaptureHeroFromStealthFight(_bounty.TargetHero);
                }
                else if (affectedAgent == Agent.Main)
                {
                    _resolved = true;
                    BountyLogger.Log($"StealthBountyFightOutcomeBehavior: player agent removed (agentState={agentState}) — bounty lost, removing.");
                    _owner.RemoveStealthBountyOnPlayerDefeat(_bounty);
                }
            }
        }

        /// <summary>
        /// Flips the current mission into combat mode against the tavern bounty
        /// target and its thugs, reassigning teams, setting mutual hostility, and
        /// alarming every agent.
        /// </summary>
        private void ExecuteTwoStageTavernAttack()
        {
            var bounty = GetActiveTwoStageBountyForConversation();
            if (bounty?.TargetHero?.CharacterObject == null)
            {
                BountyLogger.Log("ExecuteTwoStageTavernAttack: no matching two-stage bounty found for current conversation.");
                return;
            }

            try
            {
                if (Mission.Current == null)
                {
                    BountyLogger.Log("ExecuteTwoStageTavernAttack: ABORT — Mission.Current is null.");
                    return;
                }

                Agent targetAgent = Mission.Current.Agents.FirstOrDefault(a => a.Character == bounty.TargetHero.CharacterObject);
                if (targetAgent == null)
                {
                    BountyLogger.Log($"ExecuteTwoStageTavernAttack: ABORT — could not find an Agent for {bounty.TargetHero.Name} in the current mission.");
                    return;
                }

                var thugAgents = new List<Agent>();
                if (bounty.ThugCharacterIds != null)
                {
                    foreach (var thugId in bounty.ThugCharacterIds)
                    {
                        var thugCharacter = MBObjectManager.Instance.GetObject<CharacterObject>(thugId);
                        if (thugCharacter == null) continue;

                        var thugAgent = Mission.Current.Agents.FirstOrDefault(a => a.Character == thugCharacter);
                        if (thugAgent != null)
                        {
                            thugAgents.Add(thugAgent);
                        }
                    }
                }

                BountyLogger.Log($"ExecuteTwoStageTavernAttack: flipping the CURRENT mission into combat mode against {bounty.TargetHero.Name} plus {thugAgents.Count} thug(s) — no new mission opened, no scene change.");

                Mission.Current.SetMissionMode(MissionMode.Battle, false);

                var playerTeam = Mission.Current.PlayerTeam;

                if (Mission.Current.DefenderTeam != null && Mission.Current.DefenderTeam.IsValid)
                {
                    targetAgent.SetTeam(Mission.Current.DefenderTeam, true);
                    foreach (var thugAgent in thugAgents)
                    {
                        thugAgent.SetTeam(Mission.Current.DefenderTeam, true);
                    }
                    BountyLogger.Log($"ExecuteTwoStageTavernAttack: reassigned target + {thugAgents.Count} thug(s) to Mission.Current.DefenderTeam.");
                }
                else
                {
                    BountyLogger.Log("ExecuteTwoStageTavernAttack: Mission.Current.DefenderTeam is null or invalid — could not reassign.");
                }

                var targetTeam = targetAgent.Team;

                BountyLogger.Log($"ExecuteTwoStageTavernAttack: playerTeam null={playerTeam == null}, IsValid={playerTeam?.IsValid}; targetTeam null={targetTeam == null}, IsValid={targetTeam?.IsValid}; same team={playerTeam == targetTeam}.");

                if (playerTeam != null && targetTeam != null && playerTeam != targetTeam
                    && playerTeam.IsValid && targetTeam.IsValid)
                {
                    playerTeam.SetIsEnemyOf(targetTeam, true);
                    targetTeam.SetIsEnemyOf(playerTeam, true);
                    BountyLogger.Log($"ExecuteTwoStageTavernAttack: set mutual enemy relationship between player team and {bounty.TargetHero.Name}'s team (covers all thugs sharing that team).");
                }
                else
                {
                    BountyLogger.Log("ExecuteTwoStageTavernAttack: could NOT set enemy relationship — see validity flags logged above.");
                }

                void AlarmOne(Agent agent, string label)
                {
                    AgentFlag flags = agent.GetAgentFlags();
                    agent.SetAgentFlags(flags | AgentFlag.CanGetAlarmed);
                    AlarmedBehaviorGroup.AlarmAgent(agent);
                    agent.SetAlarmState(Agent.AIStateFlag.Alarmed);
                    BountyLogger.Log($"ExecuteTwoStageTavernAttack: alarmed {label}.");
                }

                AlarmOne(targetAgent, $"target ({bounty.TargetHero.Name})");
                foreach (var thugAgent in thugAgents)
                {
                    AlarmOne(thugAgent, "thug");
                }

                Mission.Current.AddMissionBehavior(new TwoStageFightOutcomeBehavior(this, bounty, targetAgent));
                BountyLogger.Log("ExecuteTwoStageTavernAttack: TwoStageFightOutcomeBehavior registered.");

                BountyLogger.Log("ExecuteTwoStageTavernAttack: mission mode set to Battle and all agents alarmed successfully.");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"ExecuteTwoStageTavernAttack: EXCEPTION — {ex}");
            }
        }

        /// <summary>
        /// Mission behavior that watches for the tavern bounty fight's outcome,
        /// capturing the target or removing the bounty depending on who goes down.
        /// Thugs are unwatched ordinary combatants.
        /// </summary>
        private class TwoStageFightOutcomeBehavior : MissionBehavior
        {
            private readonly BountyHunterBehavior _owner;
            private readonly BountyTarget _bounty;
            private readonly Agent _targetAgent;
            private bool _resolved;

            /// <summary>
            /// Creates the outcome watcher for a given owner, bounty, and target
            /// agent.
            /// </summary>
            public TwoStageFightOutcomeBehavior(BountyHunterBehavior owner, BountyTarget bounty, Agent targetAgent)
            {
                _owner = owner;
                _bounty = bounty;
                _targetAgent = targetAgent;
            }

            public override MissionBehaviorType BehaviorType => MissionBehaviorType.Logic;

            /// <summary>
            /// Captures the target if they go down, or removes the bounty if the
            /// player does.
            /// </summary>
            public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
            {
                if (_resolved) return;

                if (affectedAgent == _targetAgent)
                {
                    _resolved = true;
                    BountyLogger.Log($"TwoStageFightOutcomeBehavior: target agent for {_bounty.TargetHero?.Name} removed (agentState={agentState}) — treating as defeated, capturing.");
                    _owner.ForceCaptureHeroFromStealthFight(_bounty.TargetHero);
                }
                else if (affectedAgent == Agent.Main)
                {
                    _resolved = true;
                    BountyLogger.Log($"TwoStageFightOutcomeBehavior: player agent removed (agentState={agentState}) — bounty lost, removing.");
                    _owner.RemoveStealthBountyOnPlayerDefeat(_bounty);
                }
            }
        }

        /// <summary>
        /// Flips a captured bounty target's status to Captured, stops tracking it,
        /// and notifies its quest.
        /// </summary>
        private void OnHeroPrisonerTaken(PartyBase capturer, Hero prisoner)
        {
            var matchingBounty = _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active && b.TargetHero == prisoner);

            if (matchingBounty == null) return;

            matchingBounty.Status = BountyStatus.Captured;
            UntrackBounty(matchingBounty);
            matchingBounty.AssociatedQuest?.NotifyBountyCaptured();

            bool takenByPlayer = capturer?.MobileParty == MobileParty.MainParty;

            BountyLogger.Log($"Bounty target captured: {prisoner.Name}. TakenByPlayer={takenByPlayer}, Capturer={capturer?.Name}.");

            if (takenByPlayer)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"You've captured {prisoner.Name}!",
                    Colors.Green));
            }
        }

        /// <summary>
        /// Vetoes death for any active bounty target, queuing them for forced
        /// capture instead.
        /// </summary>
        private void OnCanHeroDie(Hero hero, KillCharacterAction.KillCharacterActionDetail detail, ref bool result)
        {
            bool isActiveBountyTarget = _activeBounties.Any(b => b.Status == BountyStatus.Active && b.TargetHero == hero);

            if (isActiveBountyTarget)
            {
                result = false;
                _pendingForcedCaptures.Add(hero);
                BountyLogger.Log($"CanHeroDie: vetoed death for bounty target {hero.Name} (detail={detail}); queued for forced capture.");
            }
        }

        /// <summary>
        /// Forces any active bounty target to be capturable as a prisoner.
        /// </summary>
        private void OnCanHeroBecomePrisoner(Hero hero, ref bool result)
        {
            bool isActiveBountyTarget = _activeBounties.Any(b => b.Status == BountyStatus.Active && b.TargetHero == hero);

            if (isActiveBountyTarget)
            {
                result = true;
                BountyLogger.Log($"CanHeroBecomePrisoner: forced capture for bounty target {hero.Name}.");
            }
        }

        /// <summary>
        /// Marks a bounty as killed and stops tracking it.
        /// </summary>
        public void MarkKilled(BountyTarget bounty)
        {
            bounty.Status = BountyStatus.Killed;
            UntrackBounty(bounty);
            BountyLogger.Log($"Bounty marked KILLED: {bounty.TargetHero?.Name}.");
        }

        /// <summary>
        /// Returns whether the given hero is currently in the main party's prison
        /// roster.
        /// </summary>
        private bool IsHeroInMainPartyPrisonRoster(Hero hero)
        {
            var roster = MobileParty.MainParty.PrisonRoster;
            return roster.GetTroopRoster().Any(element =>
                element.Character.IsHero && element.Character.HeroObject == hero && element.Number > 0);
        }

        /// <summary>
        /// Pays out a captured bounty's reward, releases the prisoner, completes its
        /// quest, and removes the bounty.
        /// </summary>
        public void TurnInBounty(BountyTarget bounty)
        {
            if (!IsHeroInMainPartyPrisonRoster(bounty.TargetHero))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"You don't have {bounty.TargetHero.Name} in your prison roster — bring them here first."));
                BountyLogger.Log($"TurnInBounty failed — {bounty.TargetHero.Name} not found in main party prison roster.");
                return;
            }

            MobileParty.MainParty.PrisonRoster.AddToCounts(bounty.TargetHero.CharacterObject, -1);

            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, bounty.BountyValue, false);

            BountyLogger.Log($"Bounty turned in: {bounty.TargetHero.Name}. Paid {bounty.BountyValue} gold.");
            InformationManager.DisplayMessage(new InformationMessage(
                $"Bounty collected: {bounty.BountyValue} gold for {bounty.TargetHero.Name}.",
                Colors.Green));

            EndCaptivityAction.ApplyByReleasedByChoice(bounty.TargetHero, null);

            bounty.AssociatedQuest?.CompleteBountyQuest(true);

            _activeBounties.Remove(bounty);
        }

        /// <summary>
        /// Accepts a bounty as a tracked contract, creating a map marker and the
        /// associated quest.
        /// </summary>
        public void TrackBounty(BountyTarget bounty)
        {
            if (bounty != null && bounty.IsSettlementAnchored && !bounty.IsPatrolBounty)
            {
                var settlement = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
                if (settlement == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"Could not resolve the settlement for {bounty.TargetHero?.Name}'s bounty — cannot track right now."));
                    BountyLogger.Log($"TrackBounty failed — settlement '{bounty.TargetSettlementId}' did not resolve for {bounty.TargetHero?.Name}.");
                    return;
                }

                var settlementMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(
                    settlement.Party.Banner,
                    new TextObject("{=BountyMarkerSettlement}Bounty: {HERO_NAME} (near {SETTLEMENT_NAME})")
                        .SetTextVariable("HERO_NAME", bounty.TargetHero?.Name)
                        .SetTextVariable("SETTLEMENT_NAME", settlement.Name),
                    settlement.GatePosition.AsVec3(),
                    true,
                    bounty.TrackerId);

                bounty.IsTracked = true;
                EnsureBountyQuestCreated(bounty);

                BountyLogger.Log($"Now tracking settlement-anchored bounty: {bounty.TargetHero?.Name} at {settlementMarker.Position} ({settlement.Name}).");
                InformationManager.DisplayMessage(new InformationMessage($"Now tracking {bounty.TargetHero?.Name} near {settlement.Name}."));
                return;
            }

            var party = bounty?.TargetHero?.PartyBelongedTo;
            if (party == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    $"{bounty?.TargetHero?.Name} is not currently with a visible party — cannot track right now."));
                BountyLogger.Log($"TrackBounty failed — no visible party for {bounty?.TargetHero?.Name}.");
                return;
            }

            var marker = Campaign.Current.MapMarkerManager.CreateMapMarker(
                party.Banner,
                new TextObject("{=BountyMarker}Bounty: {HERO_NAME}").SetTextVariable("HERO_NAME", bounty.TargetHero.Name),
                party.GetPositionAsVec3(),
                true,
                bounty.TrackerId);

            bounty.IsTracked = true;
            EnsureBountyQuestCreated(bounty);

            BountyLogger.Log($"Now tracking bounty via MapMarker: {bounty.TargetHero.Name} at {marker.Position}.");
            InformationManager.DisplayMessage(new InformationMessage($"Now tracking {bounty.TargetHero.Name} on the map."));
        }

        /// <summary>
        /// Creates and starts the BountyQuest wrapper for a bounty the first time it
        /// is accepted, resolving a nominal quest giver.
        /// </summary>
        private void EnsureBountyQuestCreated(BountyTarget bounty)
        {
            if (bounty == null || bounty.AssociatedQuest != null) return;

            Hero questGiver = null;
            if (bounty.IsSettlementAnchored)
            {
                var settlement = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
                questGiver = settlement?.OwnerClan?.Leader;
            }
            questGiver ??= Hero.MainHero;

            try
            {
                bounty.AssociatedQuest = new BountyQuest(bounty, questGiver, CampaignTime.DaysFromNow(MaxExpiryDays));

                bounty.AssociatedQuest.StartQuest();

                BountyLogger.Log($"EnsureBountyQuestCreated: created BountyQuest for {bounty.TargetHero?.Name} (giver='{questGiver?.Name}').");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"EnsureBountyQuestCreated: EXCEPTION creating BountyQuest for {bounty.TargetHero?.Name} — {ex}");
            }
        }

        /// <summary>
        /// Removes a bounty's map marker and clears its tracked flag.
        /// </summary>
        public void UntrackBounty(BountyTarget bounty)
        {
            if (bounty == null || !bounty.IsTracked) return;

            Campaign.Current.MapMarkerManager.RemoveAllMapMarkersByQuestId(bounty.TrackerId);
            bounty.IsTracked = false;

            BountyLogger.Log($"Stopped tracking bounty: {bounty.TargetHero?.Name}.");
        }

        /// <summary>
        /// Registers the bounty board menu option and any other bounty-related game
        /// menu options.
        /// </summary>
        private void AddGameMenus(CampaignGameStarter starter)
        {
            starter.AddGameMenuOption(
                "town",
                "bh_bounty_board",
                "Bounty Board",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    return Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown;
                },
                args =>
                {
                    try
                    {
                        BountyLogger.Log("[BountyBoard] Menu clicked — about to call CreateState<BountyBoardState>().");
                        var bountyBoardState = Game.Current.GameStateManager.CreateState<UI.BountyBoardState>();
                        BountyLogger.Log("[BountyBoard] CreateState returned successfully. About to assign Behavior.");

                        bountyBoardState.Behavior = this;
                        BountyLogger.Log("[BountyBoard] Behavior assigned. About to call PushState.");

                        Game.Current.GameStateManager.PushState(bountyBoardState, 0);
                        BountyLogger.Log("[BountyBoard] PushState returned successfully.");
                    }
                    catch (Exception ex)
                    {
                        BountyLogger.Log($"[BountyBoard] Failed to open bounty board screen: {ex}");
                    }
                },
                false, 4);

            starter.AddGameMenuOption(
                "village",
                "bh_gang_attack",
                "Attack bandits",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
                    return TryGetActiveGangBountyForCurrentVillage(out _);
                },
                args =>
                {
                    if (TryGetActiveGangBountyForCurrentVillage(out var bounty))
                    {
                        StartBattleWithGangParty(bounty);
                    }
                },
                false, 10);

            starter.AddGameMenu(
                "bh_bounty_board_menu",
                "Select a bounty to view details, track it, or turn in a captured target.",
                InitBountyBoardMenu);

            for (int i = 0; i < MaxDisplayedBountySlots; i++)
            {
                int slotIndex = i;
                starter.AddGameMenuOption(
                    "bh_bounty_board_menu",
                    $"bh_bounty_slot_{slotIndex}",
                    "{BOUNTY_SLOT_TEXT_" + slotIndex + "}",
                    args =>
                    {
                        var combined = GetActiveBounties().Concat(GetCapturedAwaitingTurnIn()).ToList();
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                        return slotIndex < combined.Count;
                    },
                    args => OnBountySlotSelected(slotIndex));
            }

            starter.AddGameMenuOption(
                "bh_bounty_board_menu",
                "bh_back",
                "Back",
                args =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                args => GameMenu.SwitchToMenu("town"));
        }

        /// <summary>
        /// Returns whether an active, spawned gang bounty exists for the village
        /// currently being visited.
        /// </summary>
        private bool TryGetActiveGangBountyForCurrentVillage(out BountyTarget bounty)
        {
            bounty = null;

            if (Settlement.CurrentSettlement == null || !Settlement.CurrentSettlement.IsVillage)
                return false;

            bounty = _activeBounties.FirstOrDefault(b =>
                b.Status == BountyStatus.Active &&
                b.IsSettlementAnchored &&
                !b.IsStealthBounty &&
                !b.IsPatrolBounty &&
                b.TargetSettlementId == Settlement.CurrentSettlement.StringId &&
                !string.IsNullOrEmpty(b.GangPartyId));

            if (bounty == null) return false;

            var party = FindMobilePartyByStringId(bounty.GangPartyId);
            return party != null && party.IsActive;
        }

        /// <summary>
        /// Forces a raid battle between the player and the village's gang bounty
        /// party, stripping any extra village-affiliated defenders so it's a
        /// player-only fight.
        /// </summary>
        private void StartBattleWithGangParty(BountyTarget bounty)
        {
            if (bounty == null || string.IsNullOrEmpty(bounty.GangPartyId)) return;

            var party = FindMobilePartyByStringId(bounty.GangPartyId);
            if (party == null || !party.IsActive)
            {
                BountyLogger.Log($"StartBattleWithGangParty: ABORT — party '{bounty.GangPartyId}' for {bounty.TargetHero?.Name} is null or inactive.");
                return;
            }

            var settlement = Settlement.All.FirstOrDefault(s => s.StringId == bounty.TargetSettlementId);
            if (settlement == null)
            {
                BountyLogger.Log($"StartBattleWithGangParty: ABORT — settlement '{bounty.TargetSettlementId}' did not resolve.");
                return;
            }

            _gangPartiesEngagingBattle.Add(bounty.GangPartyId);
            try
            {
                BountyLogger.Log($"StartBattleWithGangParty: entered. party='{party.StringId}', party.MapFaction='{party.MapFaction?.StringId ?? "NULL"}', party.LeaderHero='{party.LeaderHero?.Name}', party.IsActive={party.IsActive}, party.MemberRoster.TotalManCount={party.MemberRoster.TotalManCount}.");

                var gangFaction = party.MapFaction;
                var playerFaction = Hero.MainHero.MapFaction;

                BountyLogger.Log($"StartBattleWithGangParty: about to check/declare war. gangFaction='{gangFaction?.StringId ?? "NULL"}', playerFaction='{playerFaction?.StringId ?? "NULL"}'.");

                if (gangFaction != null && playerFaction != null && gangFaction != playerFaction)
                {
                    BountyLogger.Log($"StartBattleWithGangParty: calling DeclareWarAction.ApplyByPlayerHostility('{gangFaction.StringId}', '{playerFaction.StringId}')...");
                    DeclareWarAction.ApplyByPlayerHostility(gangFaction, playerFaction);
                    BountyLogger.Log("StartBattleWithGangParty: DeclareWarAction.ApplyByPlayerHostility returned successfully.");
                }
                else
                {
                    BountyLogger.Log("StartBattleWithGangParty: skipped DeclareWarAction (one faction null, or factions are the same).");
                }

                BountyLogger.Log($"StartBattleWithGangParty: about to call RestartPlayerEncounter(defender=settlement '{settlement.Name}', attacker=gang '{bounty.GangPartyId}').");

                PlayerEncounter.RestartPlayerEncounter(settlement.Party, party.Party, forcePlayerOutFromSettlement: false);
                BountyLogger.Log("StartBattleWithGangParty: RestartPlayerEncounter returned successfully. About to call StartBattle.");

                PlayerEncounter.StartBattle();
                BountyLogger.Log("StartBattleWithGangParty: StartBattle returned successfully.");

                var mapEvent = PlayerEncounter.Battle;
                if (mapEvent?.DefenderSide != null)
                {
                    var settlementLeaderParty = mapEvent.DefenderSide.LeaderParty;

                    var nonPlayerDefenders = mapEvent.DefenderSide.Parties
                        .Where(mp => mp.Party != PartyBase.MainParty && mp.Party != settlementLeaderParty)
                        .ToList();

                    foreach (var mapEventParty in nonPlayerDefenders)
                    {
                        BountyLogger.Log($"StartBattleWithGangParty: removing non-player defender '{mapEventParty.Party?.Name}' from raid defender side (player-only fight requested).");
                        mapEventParty.Party.MapEventSide = null;
                    }

                    if (nonPlayerDefenders.Count == 0)
                    {
                        BountyLogger.Log("StartBattleWithGangParty: no extra non-player defenders to remove (settlement's own leader party always preserved).");
                    }
                }
                else
                {
                    BountyLogger.Log("StartBattleWithGangParty: PlayerEncounter.Battle or DefenderSide was null — could not check for non-player defenders.");
                }

                BountyLogger.Log("StartBattleWithGangParty: about to call JoinBattle(Defender) to attach the player's party to the raid.");

                PlayerEncounter.JoinBattle(BattleSideEnum.Defender);
                BountyLogger.Log("StartBattleWithGangParty: JoinBattle returned successfully. About to call Update.");

                PlayerEncounter.Update();
                BountyLogger.Log("StartBattleWithGangParty: Update returned successfully. All steps completed.");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"StartBattleWithGangParty: EXCEPTION — {ex}");
            }
            finally
            {
                _gangPartiesEngagingBattle.Remove(bounty.GangPartyId);
            }
        }

        /// <summary>
        /// Populates the bounty board menu's display text for each visible slot.
        /// </summary>
        private void InitBountyBoardMenu(MenuCallbackArgs args)
        {
            var combined = GetActiveBounties().Concat(GetCapturedAwaitingTurnIn()).ToList();
            BountyLogger.Log($"Bounty board opened — {combined.Count} bounties displayed (active + captured).");

            for (int i = 0; i < MaxDisplayedBountySlots; i++)
            {
                string text;
                if (i < combined.Count)
                {
                    var bounty = combined[i];

                    if (bounty.Status == BountyStatus.Captured)
                    {
                        text = $"{bounty.TargetHero.Name} — CAPTURED, awaiting turn-in ({bounty.BountyValue} gold)";
                    }
                    else
                    {
                        string guardNote = bounty.GuardPartySize > 0
                            ? $" (guarded by ~{bounty.GuardPartySize} men)"
                            : " (unguarded)";
                        string trackedNote = bounty.IsTracked ? " [TRACKING]" : "";

                        text = $"{bounty.TargetHero.Name} — {bounty.BountyValue} gold{guardNote}, " +
                               $"expires in {bounty.ExpiryDate.RemainingDaysFromNow:0} days{trackedNote}";
                    }
                }
                else
                {
                    text = "";
                }

                MBTextManager.SetTextVariable($"BOUNTY_SLOT_TEXT_{i}", text);
            }
        }

        /// <summary>
        /// Shows the detail inquiry for the bounty in the given menu slot.
        /// </summary>
        private void OnBountySlotSelected(int slotIndex)
        {
            var combined = GetActiveBounties().Concat(GetCapturedAwaitingTurnIn()).ToList();
            if (slotIndex >= combined.Count) return;

            var bounty = combined[slotIndex];
            ShowBountyDetail(bounty);
        }

        /// <summary>
        /// Shows an inquiry with a bounty's details and its context-appropriate
        /// action (turn in, track, or stop tracking).
        /// </summary>
        private void ShowBountyDetail(BountyTarget bounty)
        {
            string encyclopediaLink = bounty.TargetHero.EncyclopediaLinkWithName.ToString();

            if (bounty.Status == BountyStatus.Captured)
            {
                bool inRoster = IsHeroInMainPartyPrisonRoster(bounty.TargetHero);

                string capturedMessage = $"{encyclopediaLink}\n\n" +
                                          $"Bounty: {bounty.BountyValue} gold\n" +
                                          (inRoster
                                              ? "This target is in your prison roster — turn them in for the reward."
                                              : "This target was captured elsewhere and is not currently in your prison roster.");

                InformationManager.ShowInquiry(new InquiryData(
                    "Bounty Details",
                    capturedMessage,
                    inRoster, true,
                    "Turn In",
                    "Close",
                    () =>
                    {
                        TurnInBounty(bounty);
                        GameMenu.SwitchToMenu("bh_bounty_board_menu");
                    },
                    () => GameMenu.SwitchToMenu("bh_bounty_board_menu")));
                return;
            }

            string message = $"{encyclopediaLink}\n\n" +
                              $"Bounty: {bounty.BountyValue} gold\n" +
                              $"Guard size: {bounty.GuardPartySize}\n" +
                              $"Expires in {bounty.ExpiryDate.RemainingDaysFromNow:0} days\n" +
                              (bounty.IsTracked ? "Currently tracking this target." : "Not currently tracked.");

            InformationManager.ShowInquiry(new InquiryData(
                "Bounty Details",
                message,
                true, true,
                bounty.IsTracked ? "Stop Tracking" : "Track on Map",
                "Close",
                () =>
                {
                    if (bounty.IsTracked)
                        UntrackBounty(bounty);
                    else
                        TrackBounty(bounty);

                    GameMenu.SwitchToMenu("bh_bounty_board_menu");
                },
                () => GameMenu.SwitchToMenu("bh_bounty_board_menu")));
        }
    }
}