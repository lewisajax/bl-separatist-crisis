using TaleWorlds.Core;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// Pushable game state representing "the bounty board screen is open". Holds no
    /// UI code itself — BountyBoardScreen creates the Gauntlet layer and view model
    /// when this state becomes active.
    /// </summary>
    public class BountyBoardState : GameState
    {
        public BountyHunterBehavior Behavior { get; set; }

        /// <summary>
        /// Called when this state is initialized.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        /// <summary>
        /// Called when this state is finalized.
        /// </summary>
        protected override void OnFinalize()
        {
            base.OnFinalize();
        }
    }
}
