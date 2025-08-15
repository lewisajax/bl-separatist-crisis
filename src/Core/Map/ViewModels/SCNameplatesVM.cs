using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using SeparatistCrisis.ObjectTypes;
using SeparatistCrisis.Components;

namespace SeparatistCrisis.ViewModels
{
    public class SCNameplatesVM: ViewModel
    {
        private readonly Camera _mapCamera;

        private Vec3 _cachedCameraPosition;

        private readonly TWParallel.ParallelForAuxPredicate UpdateNameplateAuxMTPredicate;

        private readonly Action<Vec2> _fastMoveCameraToPosition;

        private IEnumerable<Tuple<Settlement, GameEntity>> _allHideouts;

        private IEnumerable<Tuple<Settlement, GameEntity>> _allRetreats;

        private IEnumerable<Tuple<Settlement, GameEntity?>> _allGroups;

        private MBBindingList<SCNameplateVM> _nameplates;

        [DataSourceProperty]
        public MBBindingList<SCNameplateVM> Nameplates
        {
            get
            {
                return this._nameplates;
            }
            set
            {
                if (this._nameplates != value)
                {
                    this._nameplates = value;
                    base.OnPropertyChangedWithValue<MBBindingList<SCNameplateVM>>(value, "Nameplates");
                }
            }
        }

        public SCNameplatesVM(Camera mapCamera, Action<Vec2> fastMoveCameraToPosition)
        {
            this.Nameplates = new MBBindingList<SCNameplateVM>();
            this._mapCamera = mapCamera;
            this._fastMoveCameraToPosition = fastMoveCameraToPosition;
            CampaignEvents.PartyVisibilityChangedEvent.AddNonSerializedListener(this, new Action<PartyBase>(this.OnPartyBaseVisibilityChange));
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction, DeclareWarAction.DeclareWarDetail>(this.OnWarDeclared));
            CampaignEvents.MakePeace.AddNonSerializedListener(this, new Action<IFaction, IFaction, MakePeaceAction.MakePeaceDetail>(this.OnPeaceDeclared));
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangeKingdom));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(this.OnSettlementOwnerChanged));
            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, new Action<SiegeEvent>(this.OnSiegeEventStartedOnSettlement));
            CampaignEvents.OnSiegeEventEndedEvent.AddNonSerializedListener(this, new Action<SiegeEvent>(this.OnSiegeEventEndedOnSettlement));
            CampaignEvents.RebelliousClanDisbandedAtSettlement.AddNonSerializedListener(this, new Action<Settlement, Clan>(this.OnRebelliousClanDisbandedAtSettlement));
            this.UpdateNameplateAuxMTPredicate = new TWParallel.ParallelForAuxPredicate(this.UpdateNameplateAuxMT);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Nameplates.ApplyActionOnAllItems(delegate (SCNameplateVM x)
            {
                x.RefreshValues();
            });
        }

        public void Initialize(IEnumerable<Tuple<Settlement, GameEntity?>> settlements)
        {
            IEnumerable<Tuple<Settlement, GameEntity>> enumerable = from x in settlements
                                                                    where !x.Item1.IsHideout &&
                                                                    !(x.Item1.SettlementComponent is RetirementSettlementComponent) &&
                                                                    !(x.Item1.SettlementComponent is SettlementGroupComponent)
                                                                    select x;
            this._allHideouts = from x in settlements
                                where x.Item1.IsHideout && !(x.Item1.SettlementComponent is RetirementSettlementComponent)
                                select x;
            this._allRetreats = from x in settlements
                                where !x.Item1.IsHideout && x.Item1.SettlementComponent is RetirementSettlementComponent
                                select x;

            this._allGroups = from x in settlements where x.Item1.SettlementComponent is SettlementGroupComponent select x;

            foreach (Tuple<Settlement, GameEntity> tuple in enumerable)
            {
                SCNameplateVM item = new SCNameplateVM(tuple.Item1, tuple.Item2, this._mapCamera, this._fastMoveCameraToPosition);
                this.Nameplates.Add(item);
            }

            foreach (Tuple<Settlement, GameEntity?> tuple in this._allGroups)
            {
                SettlementGroupComponent groupSettlementComponent;
                if ((groupSettlementComponent = (SettlementGroupComponent)tuple.Item1.SettlementComponent) != null)
                {
                    if (groupSettlementComponent.IsSpotted)
                    {
                        bool isSystem = true;
                        SCNameplateVM item = new SCNameplateVM(tuple.Item1, tuple.Item2, this._mapCamera, this._fastMoveCameraToPosition, isSystem);
                        this.Nameplates.Add(item);
                    }
                }
            }

            foreach (Tuple<Settlement, GameEntity> tuple2 in this._allHideouts)
            {
                if (tuple2.Item1.Hideout.IsSpotted)
                {
                    SCNameplateVM item2 = new SCNameplateVM(tuple2.Item1, tuple2.Item2, this._mapCamera, this._fastMoveCameraToPosition);
                    this.Nameplates.Add(item2);
                }
            }

            foreach (Tuple<Settlement, GameEntity> tuple3 in this._allRetreats)
            {
                RetirementSettlementComponent retirementSettlementComponent;
                if ((retirementSettlementComponent = (tuple3.Item1.SettlementComponent as RetirementSettlementComponent)) != null)
                {
                    if (retirementSettlementComponent.IsSpotted)
                    {
                        SCNameplateVM item3 = new SCNameplateVM(tuple3.Item1, tuple3.Item2, this._mapCamera, this._fastMoveCameraToPosition);
                        this.Nameplates.Add(item3);
                    }
                }
                else
                {
                    Debug.FailedAssert("A settlement which is IsRetreat doesn't have a retirement component.", "C:\\Develop\\MB3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SettlementNameplatesVM.cs", "Initialize", 83);
                }
            }

            foreach (SCNameplateVM settlementNameplateVM in this.Nameplates)
            {
                Settlement settlement = settlementNameplateVM.Settlement;
                if (((settlement != null) ? settlement.SiegeEvent : null) != null)
                {
                    SCNameplateVM settlementNameplateVM2 = settlementNameplateVM;
                    Settlement settlement2 = settlementNameplateVM.Settlement;
                    settlementNameplateVM2.OnSiegeEventStartedOnSettlement((settlement2 != null) ? settlement2.SiegeEvent : null);
                }
                else if (settlementNameplateVM.Settlement.IsTown || settlementNameplateVM.Settlement.IsCastle)
                {
                    Clan ownerClan = settlementNameplateVM.Settlement.OwnerClan;
                    if (ownerClan != null && ownerClan.IsRebelClan)
                    {
                        settlementNameplateVM.OnRebelliousClanFormed(settlementNameplateVM.Settlement.OwnerClan);
                    }
                }
            }
            this.RefreshRelationsOfNameplates();
        }

        private void UpdateNameplateAuxMT(int startInclusive, int endExclusive)
        {
            for (int i = startInclusive; i < endExclusive; i++)
            {
                this.Nameplates[i].UpdateNameplateMT(this._cachedCameraPosition);
            }
        }

        public void Update()
        {
            this._cachedCameraPosition = this._mapCamera.Position;
            TWParallel.For(0, this.Nameplates.Count, this.UpdateNameplateAuxMTPredicate, 16);
            for (int i = 0; i < this.Nameplates.Count; i++)
            {

                this.Nameplates[i].RefreshBindValues();
            }
        }

        private void OnSiegeEventStartedOnSettlement(SiegeEvent siegeEvent)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == siegeEvent.BesiegedSettlement);
            if (settlementNameplateVM == null)
            {
                return;
            }
            settlementNameplateVM.OnSiegeEventStartedOnSettlement(siegeEvent);
        }

        private void OnSiegeEventEndedOnSettlement(SiegeEvent siegeEvent)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == siegeEvent.BesiegedSettlement);
            if (settlementNameplateVM == null)
            {
                return;
            }
            settlementNameplateVM.OnSiegeEventEndedOnSettlement(siegeEvent);
        }

        private void OnMapEventStartedOnSettlement(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == mapEvent.MapEventSettlement);
            if (settlementNameplateVM == null)
            {
                return;
            }
            settlementNameplateVM.OnMapEventStartedOnSettlement(mapEvent);
        }

        private void OnMapEventEndedOnSettlement(MapEvent mapEvent)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == mapEvent.MapEventSettlement);
            if (settlementNameplateVM == null)
            {
                return;
            }
            settlementNameplateVM.OnMapEventEndedOnSettlement();
        }

        private void OnPartyBaseVisibilityChange(PartyBase party)
        {
            if (party.IsSettlement)
            {
                Tuple<Settlement, GameEntity>? desiredSettlementTuple = null;
                if (party.Settlement.IsHideout)
                {
                    desiredSettlementTuple = this._allHideouts.SingleOrDefault((Tuple<Settlement, GameEntity> h) => h.Item1.Hideout == party.Settlement.Hideout);
                }
                else if (party.Settlement.SettlementComponent is RetirementSettlementComponent)
                {
                    desiredSettlementTuple = this._allRetreats.SingleOrDefault((Tuple<Settlement, GameEntity> h) => h.Item1.SettlementComponent as RetirementSettlementComponent == party.Settlement.SettlementComponent as RetirementSettlementComponent);
                }
                else
                {
                    Debug.FailedAssert("We don't support hiding non retreat or non hideout settlements.", "C:\\Develop\\MB3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SettlementNameplatesVM.cs", "OnPartyBaseVisibilityChange", 176);
                }
                if (desiredSettlementTuple != null)
                {
                    SCNameplateVM? settlementNameplateVM = this.Nameplates.SingleOrDefault((SCNameplateVM n) => n.Settlement == desiredSettlementTuple.Item1);
                    if (party.IsVisible && settlementNameplateVM == null)
                    {
                        SCNameplateVM settlementNameplateVM2 = new SCNameplateVM(desiredSettlementTuple.Item1, desiredSettlementTuple.Item2, this._mapCamera, this._fastMoveCameraToPosition);
                        this.Nameplates.Add(settlementNameplateVM2);
                        settlementNameplateVM2.RefreshRelationStatus();
                        return;
                    }
                    if (!party.IsVisible && settlementNameplateVM != null)
                    {
                        this.Nameplates.Remove(settlementNameplateVM);
                    }
                }
            }
        }

        private void OnPeaceDeclared(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
        {
            this.OnPeaceOrWarDeclared(faction1, faction2);
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail arg3)
        {
            this.OnPeaceOrWarDeclared(faction1, faction2);
        }

        private void OnPeaceOrWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (faction1 == Hero.MainHero.MapFaction || faction1 == Hero.MainHero.Clan || faction2 == Hero.MainHero.MapFaction || faction2 == Hero.MainHero.Clan)
            {
                this.RefreshRelationsOfNameplates();
            }
        }

        private void OnClanChangeKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            this.RefreshRelationsOfNameplates();
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero previousOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.SingleOrDefault((SCNameplateVM n) => n.Settlement == settlement);
            if (settlementNameplateVM != null)
            {
                settlementNameplateVM.RefreshDynamicProperties(true);
            }
            if (settlementNameplateVM != null)
            {
                settlementNameplateVM.RefreshRelationStatus();
            }
            using (List<Village>.Enumerator enumerator = settlement.BoundVillages.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Village village = enumerator.Current;
                    SCNameplateVM? settlementNameplateVM2 = this.Nameplates.SingleOrDefault((SCNameplateVM n) => n.Settlement.IsVillage && n.Settlement.Village == village);
                    if (settlementNameplateVM2 != null)
                    {
                        settlementNameplateVM2.RefreshDynamicProperties(true);
                    }
                    if (settlementNameplateVM2 != null)
                    {
                        settlementNameplateVM2.RefreshRelationStatus();
                    }
                }
            }
            if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByRebellion)
            {
                if (previousOwner != null && previousOwner.IsRebel)
                {
                    SCNameplateVM? settlementNameplateVM3 = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == settlement);
                    if (settlementNameplateVM3 == null)
                    {
                        return;
                    }
                    settlementNameplateVM3.OnRebelliousClanDisbanded(previousOwner.Clan);
                }
                return;
            }
            SCNameplateVM? settlementNameplateVM4 = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == settlement);
            if (settlementNameplateVM4 == null)
            {
                return;
            }
            settlementNameplateVM4.OnRebelliousClanFormed(newOwner.Clan);
        }

        private void OnRebelliousClanDisbandedAtSettlement(Settlement settlement, Clan clan)
        {
            SCNameplateVM? settlementNameplateVM = this.Nameplates.FirstOrDefault((SCNameplateVM n) => n.Settlement == settlement);
            if (settlementNameplateVM == null)
            {
                return;
            }
            settlementNameplateVM.OnRebelliousClanDisbanded(clan);
        }

        private void RefreshRelationsOfNameplates()
        {
            foreach (SCNameplateVM settlementNameplateVM in this.Nameplates)
            {
                settlementNameplateVM.RefreshRelationStatus();
            }
        }

        private void RefreshDynamicPropertiesOfNameplates()
        {
            foreach (SCNameplateVM settlementNameplateVM in this.Nameplates)
            {
                settlementNameplateVM.RefreshDynamicProperties(false);
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEvents.PartyVisibilityChangedEvent.ClearListeners(this);
            CampaignEvents.WarDeclared.ClearListeners(this);
            CampaignEvents.MakePeace.ClearListeners(this);
            CampaignEvents.OnClanChangedKingdomEvent.ClearListeners(this);
            CampaignEvents.OnSettlementOwnerChangedEvent.ClearListeners(this);
            CampaignEvents.OnSiegeEventStartedEvent.ClearListeners(this);
            CampaignEvents.OnSiegeEventEndedEvent.ClearListeners(this);
            CampaignEvents.RebelliousClanDisbandedAtSettlement.ClearListeners(this);
            this.Nameplates.ApplyActionOnAllItems(delegate (SCNameplateVM n)
            {
                n.OnFinalize();
            });
        }
    }
}
