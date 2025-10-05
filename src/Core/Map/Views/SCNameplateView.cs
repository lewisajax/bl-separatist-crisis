using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.ViewModelCollection.Nameplate;
using SeparatistCrisis.ObjectTypes;
using SeparatistCrisis.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.Views
{
    [OverrideView(typeof(MapSettlementNameplateView))]
    public class SCNameplateView : MapView, IGauntletMapEventVisualHandler
    {
        private GauntletLayer _layerAsGauntletLayer;
        private GauntletMovieIdentifier _movie;
        private SCNameplatesVM _dataSource;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            this._dataSource = new SCNameplatesVM(this.MapScreen.MapCameraView.Camera, new Action<CampaignVec2>(this.MapScreen.FastMoveCameraToPosition));
            this.Layer = (ScreenLayer)this.MapScreen.GetMapView<GauntletMapBasicView>().GauntletNameplateLayer;
            this._layerAsGauntletLayer = this.Layer as GauntletLayer;
            this._movie = this._layerAsGauntletLayer.LoadMovie("SCNameplate", (ViewModel)this._dataSource);

            List<Tuple<Settlement, GameEntity>> settlements = new List<Tuple<Settlement, GameEntity>>();
            foreach (Settlement settlement in (List<Settlement>)Settlement.All)
            {
                GameEntity strategicEntity = SettlementVisualManager.Current.GetSettlementVisual(settlement).StrategicEntity;
                Tuple<Settlement, GameEntity> tuple = new Tuple<Settlement, GameEntity>(settlement, strategicEntity);
                settlements.Add(tuple);
            }

            CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener((object)this, new Action<PartyBase, PartyBase>(this.OnHideoutSpotted));
            this._dataSource.Initialize((IEnumerable<Tuple<Settlement, GameEntity>>)settlements);

            if (!(Campaign.Current.VisualCreator.MapEventVisualCreator is GauntletMapEventVisualCreator eventVisualCreator))
                return;

            eventVisualCreator.Handlers.Add((IGauntletMapEventVisualHandler)this);
            foreach (GauntletMapEventVisual currentEvent in eventVisualCreator.GetCurrentEvents())
                this.GetNameplateOfMapEvent(currentEvent)?.OnMapEventStartedOnSettlement(currentEvent.MapEvent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            foreach (NameplateVM nameplate in (Collection<SCNameplateVM>)this._dataSource.Nameplates)
                nameplate.RefreshDynamicProperties(true);
        }

        protected override void OnMapScreenUpdate(float dt)
        {
            base.OnMapScreenUpdate(dt);
            this._dataSource.Update();
            foreach (SCNameplateVM nameplate in (Collection<SCNameplateVM>)this._dataSource.Nameplates)
                nameplate.CanParley = this.MapScreen.SceneLayer.Input.IsGameKeyDown(5) && Campaign.Current.Models.EncounterModel.CanMainHeroDoParleyWithParty(nameplate.Settlement.Party, out TextObject _);
        }

        protected override void OnFinalize()
        {
            if (Campaign.Current.VisualCreator.MapEventVisualCreator is GauntletMapEventVisualCreator eventVisualCreator)
                eventVisualCreator.Handlers.Remove((IGauntletMapEventVisualHandler)this);
            CampaignEvents.OnHideoutSpottedEvent.ClearListeners((object)this);
            this._layerAsGauntletLayer.ReleaseMovie(this._movie);
            this._dataSource.OnFinalize();
            this._layerAsGauntletLayer = (GauntletLayer)null;
            this.Layer = (ScreenLayer)null;
            this._movie = (GauntletMovieIdentifier)null;
            this._dataSource = (SCNameplatesVM)null;
            base.OnFinalize();
        }

        private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
        {
            MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/ui/notification/hideout_found"), hideoutParty.Settlement.GetPosition());
        }

        private SCNameplateVM GetNameplateOfMapEvent(GauntletMapEventVisual mapEvent)
        {
            int num1;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.Raid)
            {
                Settlement mapEventSettlement = mapEvent.MapEvent.MapEventSettlement;
                if ((mapEventSettlement != null ? (mapEventSettlement.IsUnderRaid ? 1 : 0) : 0) == 0)
                {
                    GauntletMapEventVisual gauntletMapEventVisual = mapEvent;
                    num1 = gauntletMapEventVisual != null ? (gauntletMapEventVisual.MapEvent.IsFinalized ? 1 : 0) : 0;
                }
                else
                    num1 = 1;
            }
            else
                num1 = 0;
            bool flag1 = num1 != 0;
            int num2;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.Siege)
            {
                Settlement mapEventSettlement = mapEvent.MapEvent.MapEventSettlement;
                if ((mapEventSettlement != null ? (mapEventSettlement.IsUnderSiege ? 1 : 0) : 0) == 0)
                {
                    GauntletMapEventVisual gauntletMapEventVisual = mapEvent;
                    num2 = gauntletMapEventVisual != null ? (gauntletMapEventVisual.MapEvent.IsFinalized ? 1 : 0) : 0;
                }
                else
                    num2 = 1;
            }
            else
                num2 = 0;
            bool flag2 = num2 != 0;
            int num3;
            if (mapEvent.MapEvent.EventType == MapEvent.BattleTypes.SallyOut || mapEvent.MapEvent.EventType == MapEvent.BattleTypes.BlockadeSallyOutBattle)
            {
                Settlement mapEventSettlement = mapEvent.MapEvent.MapEventSettlement;
                if ((mapEventSettlement != null ? (mapEventSettlement.IsUnderSiege ? 1 : 0) : 0) == 0)
                {
                    GauntletMapEventVisual gauntletMapEventVisual = mapEvent;
                    num3 = gauntletMapEventVisual != null ? (gauntletMapEventVisual.MapEvent.IsFinalized ? 1 : 0) : 0;
                }
                else
                    num3 = 1;
            }
            else
                num3 = 0;
            bool flag3 = num3 != 0;
            return mapEvent.MapEvent.MapEventSettlement != null && flag2 | flag1 | flag3 ? this._dataSource.Nameplates.FirstOrDefault<SCNameplateVM>((Func<SCNameplateVM, bool>)(n => n.Settlement == mapEvent.MapEvent.MapEventSettlement)) : (SCNameplateVM)null;
        }

        void IGauntletMapEventVisualHandler.OnNewEventStarted(GauntletMapEventVisual newEvent)
        {
            this.GetNameplateOfMapEvent(newEvent)?.OnMapEventStartedOnSettlement(newEvent.MapEvent);
        }

        void IGauntletMapEventVisualHandler.OnInitialized(GauntletMapEventVisual newEvent)
        {
            this.GetNameplateOfMapEvent(newEvent)?.OnMapEventStartedOnSettlement(newEvent.MapEvent);
        }

        void IGauntletMapEventVisualHandler.OnEventEnded(GauntletMapEventVisual newEvent)
        {
            this.GetNameplateOfMapEvent(newEvent)?.OnMapEventEndedOnSettlement();
        }

        void IGauntletMapEventVisualHandler.OnEventVisibilityChanged(
          GauntletMapEventVisual visibilityChangedEvent)
        {
        }
    }
}
