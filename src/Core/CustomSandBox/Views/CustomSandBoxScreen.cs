using SeparatistCrisis.CustomSandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SeparatistCrisis.CustomSandBox
{
    [GameStateScreen(typeof(CustomSandBoxState))]
    public class CustomSandBoxScreen : ScreenBase, IGameStateListener
    {
        private CustomSandBoxState _customSandBoxState;

        private GauntletLayer? _gauntletLayer;

        private GauntletMovieIdentifier? _gauntletMovie;

        private SCCustomSandBoxMenuVM? _dataSource;

        private bool _isMovieLoaded;

        private int _isFirstFrameCounter;

        public CustomSandBoxScreen(CustomSandBoxState customSandBoxState)
        {
            _customSandBoxState = customSandBoxState;
        }

        void IGameStateListener.OnActivate()
        {
        }

        void IGameStateListener.OnDeactivate()
        {
        }

        void IGameStateListener.OnInitialize()
        {
        }

        void IGameStateListener.OnFinalize()
        {
            _dataSource?.OnFinalize();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataSource = new SCCustomSandBoxMenuVM(this._customSandBoxState);
            _dataSource.SetStartInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
            _dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
            _dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
            _dataSource.SetRandomizeInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Randomize"));

            SpriteData spriteData = UIResourceManager.SpriteData;
            SpriteCategory spriteCategory = spriteData.SpriteCategories["ui_encyclopedia"];
            spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);

            this._gauntletLayer = new GauntletLayer("SCCustomSandBox", 1, true);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));

            this.LoadMovie();

            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _dataSource.SetActiveState(true);
            AddLayer(_gauntletLayer);
            InformationManager.HideAllMessages();
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            if (this._isFirstFrameCounter >= 0)
            {
                if (this._isFirstFrameCounter == 0)
                {
                    LoadingWindow.DisableGlobalLoadingWindow();
                }
                this._isFirstFrameCounter--;
            }
        }

        protected override void OnFinalize()
        {
            UnloadMovie();
            RemoveLayer(_gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;
            base.OnFinalize();
        }

        protected override void OnActivate()
        {
            LoadMovie();
        
            SCCustomSandBoxMenuVM? dataSource = _dataSource;
            if (dataSource != null)
            {
                dataSource.SetActiveState(true);
            }

            if (_gauntletLayer != null)
            {
                _gauntletLayer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_gauntletLayer);
            }

            _isFirstFrameCounter = 2;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            UnloadMovie();
            SCCustomSandBoxMenuVM? dataSource = _dataSource;
            if (dataSource == null)
            {
                return;
            }

            dataSource.SetActiveState(false);
        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();
            if (!_isMovieLoaded)
            {
                SCCustomSandBoxMenuVM? dataSource = _dataSource;
                if (dataSource == null)
                {
                    return;
                }

                dataSource.RefreshValues();
            }
        }

        private void LoadMovie()
        {
            if (!_isMovieLoaded && _gauntletLayer != null)
            {
                _gauntletMovie = _gauntletLayer.LoadMovie("CustomSandBoxScreen", _dataSource);
                _isMovieLoaded = true;
            }
        }

        private void UnloadMovie()
        {
            if (_isMovieLoaded && _gauntletLayer != null)
            {
                _gauntletLayer.ReleaseMovie(_gauntletMovie);
                _gauntletMovie = null;
                _isMovieLoaded = false;
                _gauntletLayer.IsFocusLayer = false;
                ScreenManager.TryLoseFocus(_gauntletLayer);
            }
        }
    }
}
