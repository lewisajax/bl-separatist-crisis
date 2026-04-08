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

namespace SeparatistCrisis.CustomBattle
{
    [GameStateScreen(typeof(CustomBattleState))]
    public class SCCustomBattleScreen : ScreenBase, IGameStateListener
    {
        private CustomBattleState _customBattleState;

        private GauntletLayer? _gauntletLayer;

        private GauntletMovieIdentifier? _gauntletMovie;

        private SCCustomBattleMenuVM? _dataSource;

        private bool _isMovieLoaded;

        private int _isFirstFrameCounter;

        public SCCustomBattleScreen(CustomBattleState customBattleState)
        {
            _customBattleState = customBattleState;
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
            _dataSource = new SCCustomBattleMenuVM(_customBattleState);
            _dataSource.SetStartInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
            _dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
            _dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
            _dataSource.SetRandomizeInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Randomize"));

            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = _dataSource.TroopTypeSelectionPopUp;

            if (troopTypeSelectionPopUp != null)
            {
                troopTypeSelectionPopUp.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
            }

            SpriteData spriteData = UIResourceManager.SpriteData;
            SpriteCategory spriteCategory = spriteData.SpriteCategories["ui_encyclopedia"];
            spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);

            this._gauntletLayer = new GauntletLayer("SCCustomBattle", 1, true);
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));

            LoadMovie();

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
            if (!this._gauntletLayer.IsFocusedOnInput())
            {
                SCTroopSelectionPopUpVM troopTypeSelectionPopUp = _dataSource.TroopTypeSelectionPopUp;
                if (troopTypeSelectionPopUp != null && troopTypeSelectionPopUp.IsOpen)
                {
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Exit"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.TroopTypeSelectionPopUp.ExecuteCancel();
                        return;
                    }
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Confirm"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.TroopTypeSelectionPopUp.ExecuteDone();
                        return;
                    }
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Reset"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.TroopTypeSelectionPopUp.ExecuteReset();
                        return;
                    }
                }
                else
                {
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Exit"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.ExecuteBack();
                        return;
                    }
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Randomize"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.ExecuteRandomize();
                        return;
                    }
                    if (this._gauntletLayer.Input.IsHotKeyReleased("Confirm"))
                    {
                        UISoundsHelper.PlayUISound("event:/ui/default");
                        this._dataSource.ExecuteStart();
                    }
                }
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
        
            SCCustomBattleMenuVM? dataSource = _dataSource;
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
            SCCustomBattleMenuVM? dataSource = _dataSource;
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
                SCCustomBattleMenuVM? dataSource = _dataSource;
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
                _gauntletMovie = _gauntletLayer.LoadMovie("CustomBattleScreen", _dataSource);
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
