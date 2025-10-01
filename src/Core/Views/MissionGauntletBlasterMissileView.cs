using SeparatistCrisis.ViewModels;
using SeparatistCrisis.Views.Placeholders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SeparatistCrisis.Views
{
    [OverrideView(typeof(MissionBlasterMissileView))]
    public class MissionGauntletBlasterMissileView : MissionView
    {
        private BlasterMissileVM? _dataSource;
        private GauntletLayer? _gauntletLayer;

        public MissionGauntletBlasterMissileView()
        {
            // this.ViewOrderPriority = 20;
            // this._dataSource = new BlasterMissileVM();
            // this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority, "GauntletLayer", false);
            // this._gauntletLayer.LoadMovie("XMLFileName", this._dataSource);
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._gauntletLayer?.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("SCGameKeyContext"));
        }

        public override void OnMissionScreenFinalize()
        {
            if (this._gauntletLayer != null)
            {
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }

            if (this._dataSource != null)
            {
                this._dataSource.OnFinalize();
                this._dataSource = null;
            }

            base.OnMissionScreenFinalize();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
        }
    }
}
