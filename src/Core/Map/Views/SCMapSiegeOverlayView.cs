using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SandBox.ViewModelCollection.MapSiege;
using SeparatistCrisis.Map.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.Map.Views
{
    [OverrideView(typeof(MapSiegeOverlayView))]
    public class SCMapSiegeOverlayView: MapView
    {
        private GauntletLayer _layerAsGauntletLayer;

        private SCMapSiegeVM _dataSource;

        private GauntletMovieIdentifier _movie;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            GauntletMapBasicView mapView = base.MapScreen.GetMapView<GauntletMapBasicView>();
            base.Layer = mapView.GauntletNameplateLayer;
            this._layerAsGauntletLayer = (base.Layer as GauntletLayer);
            SettlementVisual settlementVisual = SettlementVisualManager.Current.GetSettlementVisual(PlayerSiege.PlayerSiegeEvent.BesiegedSettlement);
            this._dataSource = new SCMapSiegeVM(base.MapScreen.MapCameraView.Camera, settlementVisual.GetAttackerBatteringRamSiegeEngineFrames(), settlementVisual.GetAttackerRangedSiegeEngineFrames(), settlementVisual.GetAttackerTowerSiegeEngineFrames(), settlementVisual.GetDefenderRangedSiegeEngineFrames(), settlementVisual.GetBreachableWallFrames());
            CampaignEvents.SiegeEngineBuiltEvent.AddNonSerializedListener(this, new Action<SiegeEvent, BattleSideEnum, SiegeEngineType>(this.OnSiegeEngineBuilt));
            this._movie = this._layerAsGauntletLayer.LoadMovie("MapSiegeOverlay", this._dataSource);
        }

        protected override void OnMapScreenUpdate(float dt)
        {
            base.OnMapScreenUpdate(dt);
            SCMapSiegeVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.Update(base.MapScreen.MapCameraView.CameraDistance);
        }

        protected override void OnFinalize()
        {
            this._layerAsGauntletLayer.ReleaseMovie(this._movie);
            this._movie = null;
            this._dataSource = null;
            base.Layer = null;
            this._layerAsGauntletLayer = null;
            CampaignEvents.SiegeEngineBuiltEvent.ClearListeners(this);
            base.OnFinalize();
        }

        protected override void OnMapConversationStart()
        {
            base.OnMapConversationStart();
            if (this._layerAsGauntletLayer != null)
            {
                ScreenManager.SetSuspendLayer(this._layerAsGauntletLayer, true);
            }
        }

        protected override void OnMapConversationOver()
        {
            base.OnMapConversationOver();
            if (this._layerAsGauntletLayer != null)
            {
                ScreenManager.SetSuspendLayer(this._layerAsGauntletLayer, false);
            }
        }

        protected override void OnSiegeEngineClick(MatrixFrame siegeEngineFrame)
        {
            base.OnSiegeEngineClick(siegeEngineFrame);
            UISoundsHelper.PlayUISound("event:/ui/panels/siege/engine_click");
            SCMapSiegeVM dataSource = this._dataSource;
            if (dataSource != null && dataSource.ProductionController.IsEnabled && this._dataSource.ProductionController.LatestSelectedPOI.MapSceneLocationFrame.NearlyEquals(siegeEngineFrame, 1E-05f))
            {
                this._dataSource.ProductionController.ExecuteDisable();
                return;
            }
            SCMapSiegeVM dataSource2 = this._dataSource;
            if (dataSource2 != null)
            {
                dataSource2.OnSelectionFromScene(siegeEngineFrame);
            }
            base.MapState.OnSiegeEngineClick(siegeEngineFrame);
        }

        protected override void OnMapTerrainClick()
        {
            base.OnMapTerrainClick();
            SCMapSiegeVM dataSource = this._dataSource;
            if (dataSource == null)
            {
                return;
            }
            dataSource.ProductionController.ExecuteDisable();
        }

        private void OnSiegeEngineBuilt(SiegeEvent siegeEvent, BattleSideEnum side, SiegeEngineType siegeEngineType)
        {
            if (siegeEvent.IsPlayerSiegeEvent && side == PlayerSiege.PlayerSide)
            {
                UISoundsHelper.PlayUISound("event:/ui/panels/siege/engine_build_complete");
            }
        }
    }
}
