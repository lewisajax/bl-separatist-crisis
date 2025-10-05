using SandBox.ViewModelCollection.Nameplate;
using SeparatistCrisis.Components;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.ViewModels
{
    public class SCNameplatesVM : ViewModel
    {
        private readonly Camera _mapCamera;
        private Vec3 _cachedCameraPosition;
        private readonly TWParallel.ParallelForAuxPredicate UpdateNameplateAuxMTPredicate;
        private readonly Action<CampaignVec2> _fastMoveCameraToPosition;
        private IEnumerable<Tuple<Settlement, GameEntity>> _allHideouts;
        private IEnumerable<Tuple<Settlement, GameEntity>> _allRetreats;
        private IEnumerable<Tuple<Settlement, GameEntity>> _allRegularSettlements;
        private IEnumerable<Tuple<Settlement, GameEntity?>> _allGroups;
        private MBBindingList<SCNameplateVM> _nameplates;

        [DataSourceProperty]
        public MBBindingList<SCNameplateVM> Nameplates
        {
            get => this._nameplates;
            set
            {
                if (this._nameplates == value)
                    return;
                this._nameplates = value;
                this.OnPropertyChangedWithValue<MBBindingList<SCNameplateVM>>(value, nameof(Nameplates));
            }
        }

        public SCNameplatesVM(Camera mapCamera, Action<CampaignVec2> fastMoveCameraToPosition)
        {
            this.Nameplates = new MBBindingList<SCNameplateVM>();
            this._mapCamera = mapCamera;
            this._fastMoveCameraToPosition = fastMoveCameraToPosition;
            CampaignEvents.PartyVisibilityChangedEvent.AddNonSerializedListener((object)this, new Action<PartyBase>(this.OnPartyBaseVisibilityChange));
            CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, new Action<IFaction, IFaction, DeclareWarAction.DeclareWarDetail>(this.OnWarDeclared));
            CampaignEvents.MakePeace.AddNonSerializedListener((object)this, new Action<IFaction, IFaction, MakePeaceAction.MakePeaceDetail>(this.OnPeaceDeclared));
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangeKingdom));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(this.OnSettlementOwnerChanged));
            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener((object)this, new Action<SiegeEvent>(this.OnSiegeEventStartedOnSettlement));
            CampaignEvents.OnSiegeEventEndedEvent.AddNonSerializedListener((object)this, new Action<SiegeEvent>(this.OnSiegeEventEndedOnSettlement));
            CampaignEvents.RebelliousClanDisbandedAtSettlement.AddNonSerializedListener((object)this, new Action<Settlement, Clan>(this.OnRebelliousClanDisbandedAtSettlement));
            this.UpdateNameplateAuxMTPredicate = new TWParallel.ParallelForAuxPredicate(this.UpdateNameplateAuxMT);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Nameplates.ApplyActionOnAllItems((Action<SCNameplateVM>)(x => x.RefreshValues()));
        }

        public void Initialize(
          IEnumerable<Tuple<Settlement, GameEntity>> settlements)
        {
            this._allRegularSettlements = settlements.Where<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(x => !x.Item1.IsHideout && !(x.Item1.SettlementComponent is RetirementSettlementComponent)));
            this._allHideouts = settlements.Where<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(x => x.Item1.IsHideout && !(x.Item1.SettlementComponent is RetirementSettlementComponent)));
            this._allRetreats = settlements.Where<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(x => !x.Item1.IsHideout && x.Item1.SettlementComponent is RetirementSettlementComponent));
            this._allGroups = settlements.Where<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(x => !x.Item1.IsHideout && x.Item1.SettlementComponent is SettlementGroupComponent));

            // Normal Settlements
            foreach (Tuple<Settlement, GameEntity> settlement in this._allRegularSettlements)
            {
                if (settlement.Item1.IsVisible)
                    this.Nameplates.Add(new SCNameplateVM(settlement.Item1, settlement.Item2, this._mapCamera, this._fastMoveCameraToPosition));
            }

            // Groups
            foreach (Tuple<Settlement, GameEntity> group in this._allGroups)
            {
                if (group.Item1.SettlementComponent is SettlementGroupComponent settlementComponent)
                {
                    if (settlementComponent.IsSpotted)
                        this.Nameplates.Add(new SCNameplateVM(group.Item1, group.Item2, this._mapCamera, this._fastMoveCameraToPosition, true));
                }
                else
                    Debug.FailedAssert("A group settlement is not comparing to SettlementGroupComponent", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SCNameplatesVM.cs", nameof(Initialize), 87);
            }

            // Hideouts
            foreach (Tuple<Settlement, GameEntity> hideout in this._allHideouts)
            {
                if (hideout.Item1.Hideout.IsSpotted)
                    this.Nameplates.Add(new SCNameplateVM(hideout.Item1, hideout.Item2, this._mapCamera, this._fastMoveCameraToPosition));
            }

            // Retreat
            foreach (Tuple<Settlement, GameEntity> retreat in this._allRetreats)
            {
                if (retreat.Item1.SettlementComponent is RetirementSettlementComponent settlementComponent)
                {
                    if (settlementComponent.IsSpotted)
                        this.Nameplates.Add(new SCNameplateVM(retreat.Item1, retreat.Item2, this._mapCamera, this._fastMoveCameraToPosition));
                }
                else
                    Debug.FailedAssert("A seetlement which is IsRetreat doesn't have a retirement component.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SCNameplatesVM.cs", nameof(Initialize), 87);
            }

            foreach (SCNameplateVM nameplate in (Collection<SCNameplateVM>)this.Nameplates)
            {
                if (nameplate.Settlement?.SiegeEvent != null)
                    nameplate.OnSiegeEventStartedOnSettlement(nameplate.Settlement?.SiegeEvent);
                else if (nameplate.Settlement.IsTown || nameplate.Settlement.IsCastle)
                {
                    Clan ownerClan = nameplate.Settlement.OwnerClan;
                    if ((ownerClan != null ? (ownerClan.IsRebelClan ? 1 : 0) : 0) != 0)
                        nameplate.OnRebelliousClanFormed(nameplate.Settlement.OwnerClan);
                }
            }

            this.RefreshRelationsOfNameplates();
        }

        private void UpdateNameplateAuxMT(int startInclusive, int endExclusive)
        {
            for (int index = startInclusive; index < endExclusive; ++index)
                this.Nameplates[index].UpdateNameplateMT(this._cachedCameraPosition);
        }

        public void Update()
        {
            this._cachedCameraPosition = this._mapCamera.Position;
            TWParallel.For(0, this.Nameplates.Count, this.UpdateNameplateAuxMTPredicate);
            for (int index = 0; index < this.Nameplates.Count; ++index)
                this.Nameplates[index].RefreshBindValues();
        }

        private void OnSiegeEventStartedOnSettlement(SiegeEvent siegeEvent)
        {
            this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == siegeEvent.BesiegedSettlement))?.OnSiegeEventStartedOnSettlement(siegeEvent);
        }

        private void OnSiegeEventEndedOnSettlement(SiegeEvent siegeEvent)
        {
            this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == siegeEvent.BesiegedSettlement))?.OnSiegeEventEndedOnSettlement(siegeEvent);
        }

        private void OnMapEventStartedOnSettlement(
          MapEvent mapEvent,
          PartyBase attackerParty,
          PartyBase defenderParty)
        {
            this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == mapEvent.MapEventSettlement))?.OnMapEventStartedOnSettlement(mapEvent);
        }

        private void OnMapEventEndedOnSettlement(MapEvent mapEvent)
        {
            this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == mapEvent.MapEventSettlement))?.OnMapEventEndedOnSettlement();
        }

        private void OnPartyBaseVisibilityChange(PartyBase party)
        {
            if (!party.IsSettlement)
                return;
            Tuple<Settlement, GameEntity> desiredSettlementTuple = (Tuple<Settlement, GameEntity>)null;
            desiredSettlementTuple = !party.Settlement.IsHideout ? (!(party.Settlement.SettlementComponent is RetirementSettlementComponent) ? this._allRegularSettlements.SingleOrDefault<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(h => h.Item1 == party.Settlement)) : this._allRetreats.SingleOrDefault<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(h => h.Item1.SettlementComponent as RetirementSettlementComponent == party.Settlement.SettlementComponent as RetirementSettlementComponent))) : this._allHideouts.SingleOrDefault<Tuple<Settlement, GameEntity>>((Func<Tuple<Settlement, GameEntity>, bool>)(h => h.Item1.Hideout == party.Settlement.Hideout));
            if (desiredSettlementTuple == null)
                return;
            SCNameplateVM settlementNameplateVm1 = this.Nameplates.SingleOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == desiredSettlementTuple.Item1));
            if (party.IsVisible && settlementNameplateVm1 == null)
            {
                SCNameplateVM settlementNameplateVm2 = new SCNameplateVM(desiredSettlementTuple.Item1, desiredSettlementTuple.Item2, this._mapCamera, this._fastMoveCameraToPosition);
                this.Nameplates.Add(settlementNameplateVm2);
                settlementNameplateVm2.RefreshRelationStatus();
            }
            else
            {
                if (party.IsVisible || settlementNameplateVm1 == null)
                    return;
                this.Nameplates.Remove(settlementNameplateVm1);
            }
        }

        private void OnPeaceDeclared(
          IFaction faction1,
          IFaction faction2,
          MakePeaceAction.MakePeaceDetail detail)
        {
            this.OnPeaceOrWarDeclared(faction1, faction2);
        }

        private void OnWarDeclared(
          IFaction faction1,
          IFaction faction2,
          DeclareWarAction.DeclareWarDetail arg3)
        {
            this.OnPeaceOrWarDeclared(faction1, faction2);
        }

        private void OnPeaceOrWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (faction1 != Hero.MainHero.MapFaction && faction1 != Hero.MainHero.Clan && faction2 != Hero.MainHero.MapFaction && faction2 != Hero.MainHero.Clan)
                return;
            this.RefreshRelationsOfNameplates();
        }

        private void OnClanChangeKingdom(
          Clan clan,
          Kingdom oldKingdom,
          Kingdom newKingdom,
          ChangeKingdomAction.ChangeKingdomActionDetail detail,
          bool showNotification)
        {
            this.RefreshRelationsOfNameplates();
        }

        private void OnSettlementOwnerChanged(
          Settlement settlement,
          bool openToClaim,
          Hero newOwner,
          Hero previousOwner,
          Hero capturerHero,
          ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            SCNameplateVM settlementNameplateVm1 = this.Nameplates.SingleOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == settlement));
            settlementNameplateVm1?.RefreshDynamicProperties(true);
            settlementNameplateVm1?.RefreshRelationStatus();
            foreach (Village boundVillage in (List<Village>)settlement.BoundVillages)
            {
                Village village = boundVillage;
                SCNameplateVM settlementNameplateVm2 = this.Nameplates.SingleOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement.IsVillage && n.Settlement.Village == village));
                settlementNameplateVm2?.RefreshDynamicProperties(true);
                settlementNameplateVm2?.RefreshRelationStatus();
            }
            if (detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByRebellion)
            {
                this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == settlement))?.OnRebelliousClanFormed(newOwner.Clan);
            }
            else
            {
                if (previousOwner == null || !previousOwner.IsRebel)
                    return;
                this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == settlement))?.OnRebelliousClanDisbanded(previousOwner.Clan);
            }
        }

        private void OnRebelliousClanDisbandedAtSettlement(Settlement settlement, Clan clan)
        {
            this.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == settlement))?.OnRebelliousClanDisbanded(clan);
        }

        private void RefreshRelationsOfNameplates()
        {
            foreach (NameplateVM nameplate in (Collection<SCNameplateVM>)this.Nameplates)
                nameplate.RefreshRelationStatus();
        }

        private void RefreshDynamicPropertiesOfNameplates()
        {
            foreach (NameplateVM nameplate in (Collection<SCNameplateVM>)this.Nameplates)
                nameplate.RefreshDynamicProperties(false);
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEventDispatcher.Instance.RemoveListeners((object)this);
            this.Nameplates.ApplyActionOnAllItems((Action<SCNameplateVM>)(n => n.OnFinalize()));
        }
    }
}
