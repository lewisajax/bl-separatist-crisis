using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.BountyHunting.UI
{
    [GameStateScreen(typeof(BountiesOnPlayerState))]
    public class BountiesOnPlayerScreen : ScreenBase, IGameStateListener
    {
        private readonly BountiesOnPlayerState _state;
        private GauntletLayer _gauntletLayer;
        private BountiesOnPlayerVM _dataSource;

        public BountiesOnPlayerScreen(BountiesOnPlayerState state)
        {
            _state = state;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _dataSource = new BountiesOnPlayerVM(_state.Behavior, CloseScreen);

            _gauntletLayer = new GauntletLayer("BountiesOnPlayer", 1, false);
            _gauntletLayer.IsFocusLayer = true;
            _gauntletLayer.LoadMovie("BountiesOnPlayer", _dataSource);

            AddLayer(_gauntletLayer);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            RemoveLayer(_gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            _dataSource?.Tick(dt);
        }

        private void CloseScreen() => Game.Current.GameStateManager.PopState();

        void IGameStateListener.OnActivate() { }
        void IGameStateListener.OnDeactivate() { }
        void IGameStateListener.OnInitialize() { }
        void IGameStateListener.OnFinalize() { }
    }
}