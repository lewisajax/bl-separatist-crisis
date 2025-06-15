using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Nameplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade;
using SeparatistCrisis.ViewModels;
using SeparatistCrisis.ObjectTypes;

namespace SeparatistCrisis.Views
{
    [OverrideView(typeof(MapSettlementNameplateView))]
    public class SCNameplateView : MapView, IGauntletMapEventVisualHandler
    {
        protected override void CreateLayout()
        {
            _dataSource = new SCNameplatesVM(MapScreen._mapCameraView.Camera, new Action<Vec2>(MapScreen.FastMoveCameraToPosition));
            GauntletMapBasicView mapView = MapScreen.GetMapView<GauntletMapBasicView>();
            Layer = mapView.GauntletNameplateLayer;
            _layerAsGauntletLayer = (GauntletLayer)Layer;
            _movie = _layerAsGauntletLayer.LoadMovie("SCNameplate", _dataSource);
            List<Tuple<Settlement, GameEntity?>> settlements = new List<Tuple<Settlement, GameEntity?>>();
            
            foreach (Settlement settlement in Settlement.All)
            {
                GameEntity? strategicEntity = PartyVisualManager.Current.GetVisualOfParty(settlement.Party)?.StrategicEntity;
                Tuple<Settlement, GameEntity?> item = new Tuple<Settlement, GameEntity?>(settlement, strategicEntity);
                settlements.Add(item);
            }

            CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener(this, new Action<PartyBase, PartyBase>(OnHideoutSpotted));
            _dataSource.Initialize(settlements);
            GauntletMapEventVisualCreator gauntletMapEventVisualCreator;

            if ((gauntletMapEventVisualCreator = (GauntletMapEventVisualCreator)Campaign.Current.VisualCreator.MapEventVisualCreator) != null)
            {
                gauntletMapEventVisualCreator.Handlers.Add(this);

                foreach (GauntletMapEventVisual gauntletMapEventVisual in gauntletMapEventVisualCreator.GetCurrentEvents())
                {
                    SettlementNameplateVM nameplateOfMapEvent = GetNameplateOfMapEvent(gauntletMapEventVisual);

                    if (nameplateOfMapEvent != null)
                    {
                        nameplateOfMapEvent.OnMapEventStartedOnSettlement(gauntletMapEventVisual.MapEvent);
                    }
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (DataSource != null)
            {
                foreach (SettlementNameplateVM settlementNameplateVM in DataSource.Nameplates)
                {
                    settlementNameplateVM.RefreshDynamicProperties(true);
                }
            }
        }

        protected override void OnMapScreenUpdate(float dt)
        {
            base.OnMapScreenUpdate(dt);

            // It took me DAYS to figure out why the nameplates weren't showing up
            // And it's all because I checked for null?? wtf
            // I ended up hard copying a lot of files that I didn't need, trying to source the problem
            _dataSource?.Update();
            // (this.DataSource != null) this.DataSource.Update();
        }

        protected override void OnFinalize()
        {
            GauntletMapEventVisualCreator gauntletMapEventVisualCreator;
            if ((gauntletMapEventVisualCreator = (GauntletMapEventVisualCreator)Campaign.Current.VisualCreator.MapEventVisualCreator) != null)
            {
                gauntletMapEventVisualCreator.Handlers.Remove(this);
            }
            CampaignEvents.OnHideoutSpottedEvent.ClearListeners(this);

            if (LayerAsGauntletLayer != null && DataSource != null)
            {
                LayerAsGauntletLayer.ReleaseMovie(_movie);
                DataSource.OnFinalize();
            }

            LayerAsGauntletLayer = null;
            Layer = null;
            _movie = null;
            _dataSource = null;
            base.OnFinalize();
        }

        private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
        {
            MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/ui/notification/hideout_found"), hideoutParty.Settlement.GetPosition());
        }

        private SettlementNameplateVM GetNameplateOfMapEvent(GauntletMapEventVisual mapEvent)
        {
            bool flag;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.Raid)
            {
                Settlement mapEventSettlement = mapEvent.MapEvent.MapEventSettlement;
                if (mapEventSettlement == null || !mapEventSettlement.IsUnderRaid)
                {
                    GauntletMapEventVisual mapEvent2 = mapEvent;
                    flag = mapEvent2 != null && mapEvent2.MapEvent.IsFinished;
                }
                else
                {
                    flag = true;
                }
            }
            else
            {
                flag = false;
            }
            bool flag2 = flag;
            bool flag3;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.Siege)
            {
                Settlement mapEventSettlement2 = mapEvent.MapEvent.MapEventSettlement;
                if (mapEventSettlement2 == null || !mapEventSettlement2.IsUnderSiege)
                {
                    GauntletMapEventVisual mapEvent3 = mapEvent;
                    flag3 = mapEvent3 != null && mapEvent3.MapEvent.IsFinished;
                }
                else
                {
                    flag3 = true;
                }
            }
            else
            {
                flag3 = false;
            }
            bool flag4 = flag3;
            bool flag5;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.SallyOut)
            {
                Settlement mapEventSettlement3 = mapEvent.MapEvent.MapEventSettlement;
                if (mapEventSettlement3 == null || !mapEventSettlement3.IsUnderSiege)
                {
                    GauntletMapEventVisual mapEvent4 = mapEvent;
                    flag5 = mapEvent4 != null && mapEvent4.MapEvent.IsFinished;
                }
                else
                {
                    flag5 = true;
                }
            }
            else
            {
                flag5 = false;
            }
            bool flag6 = flag5;
            if (mapEvent.MapEvent.MapEventSettlement != null && (flag4 || flag2 || flag6))
            {
                if (DataSource != null)
                    return DataSource.Nameplates.FirstOrDefault((SettlementNameplateVM n) => n.Settlement == mapEvent.MapEvent.MapEventSettlement);
            }

            return null;
        }

        void IGauntletMapEventVisualHandler.OnNewEventStarted(GauntletMapEventVisual newEvent)
        {
            SettlementNameplateVM nameplateOfMapEvent = GetNameplateOfMapEvent(newEvent);
            if (nameplateOfMapEvent == null)
            {
                return;
            }
            nameplateOfMapEvent.OnMapEventStartedOnSettlement(newEvent.MapEvent);
        }

        void IGauntletMapEventVisualHandler.OnInitialized(GauntletMapEventVisual newEvent)
        {
            SettlementNameplateVM nameplateOfMapEvent = GetNameplateOfMapEvent(newEvent);
            if (nameplateOfMapEvent == null)
            {
                return;
            }
            nameplateOfMapEvent.OnMapEventStartedOnSettlement(newEvent.MapEvent);
        }

        void IGauntletMapEventVisualHandler.OnEventEnded(GauntletMapEventVisual newEvent)
        {
            SettlementNameplateVM nameplateOfMapEvent = GetNameplateOfMapEvent(newEvent);
            if (nameplateOfMapEvent == null)
            {
                return;
            }
            nameplateOfMapEvent.OnMapEventEndedOnSettlement();
        }

        void IGauntletMapEventVisualHandler.OnEventVisibilityChanged(GauntletMapEventVisual visibilityChangedEvent)
        {
        }

        protected GauntletLayer? LayerAsGauntletLayer { get; private set; }

        private GauntletLayer? _layerAsGauntletLayer;

        protected IGauntletMovie? Movie { get; private set; }

        private IGauntletMovie? _movie;

        protected SCNameplatesVM? DataSource { get; private set; }

        private SCNameplatesVM? _dataSource;
    }
}
