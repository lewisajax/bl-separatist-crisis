using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// Gauntlet screen paired with BountyBoardState. Constructs the view model and
    /// Gauntlet layer, loads the BountyBoard prefab, and manages input focus for the
    /// bounty board UI.
    /// </summary>
    [GameStateScreen(typeof(BountyBoardState))]
    public class BountyBoardScreen : ScreenBase, IGameStateListener
    {
        private readonly BountyBoardState _state;
        private GauntletLayer _gauntletLayer;
        private BountyBoardVM _dataSource;

        public BountyBoardScreen(BountyBoardState state)
        {
            _state = state;
        }

        /// <summary>
        /// Constructs the view model and Gauntlet layer, loads the prefab, and sets
        /// up input handling.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            BountyLogger.Log("[BountyBoardScreen] OnInitialize entered. About to construct BountyBoardVM.");

            _dataSource = new BountyBoardVM(_state.Behavior, CloseScreen);
            BountyLogger.Log("[BountyBoardScreen] BountyBoardVM constructed. About to construct GauntletLayer.");

            _gauntletLayer = new GauntletLayer("BountyBoard", 1, false);
            BountyLogger.Log("[BountyBoardScreen] GauntletLayer constructed. About to set IsFocusLayer.");

            _gauntletLayer.IsFocusLayer = true;
            BountyLogger.Log("[BountyBoardScreen] IsFocusLayer set. About to call LoadMovie.");

            _gauntletLayer.LoadMovie("BountyBoard", _dataSource);
            BountyLogger.Log("[BountyBoardScreen] LoadMovie returned successfully. About to call AddLayer.");

            AddLayer(_gauntletLayer);
            BountyLogger.Log("[BountyBoardScreen] AddLayer returned successfully. About to call SetInputRestrictions.");

            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            BountyLogger.Log("[BountyBoardScreen] SetInputRestrictions returned successfully. OnInitialize complete.");
        }

        /// <summary>
        /// Resets input restrictions and removes the Gauntlet layer.
        /// </summary>
        protected override void OnFinalize()
        {
            base.OnFinalize();

            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            RemoveLayer(_gauntletLayer);
            _dataSource = null;
            _gauntletLayer = null;
        }

        private bool _loggedFirstFrame = false;

        /// <summary>
        /// Ticks the view model each frame and logs once when the first frame is
        /// reached.
        /// </summary>
        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);

            if (!_loggedFirstFrame)
            {
                _loggedFirstFrame = true;
                BountyLogger.Log("[BountyBoardScreen] First OnFrameTick reached.");
            }

            _dataSource?.Tick(dt);
        }

        /// <summary>
        /// Pops this screen's game state, closing the bounty board.
        /// </summary>
        private void CloseScreen()
        {
            Game.Current.GameStateManager.PopState();
        }

        void IGameStateListener.OnActivate() { }
        void IGameStateListener.OnDeactivate() { }
        void IGameStateListener.OnInitialize() { }
        void IGameStateListener.OnFinalize() { }
    }
}