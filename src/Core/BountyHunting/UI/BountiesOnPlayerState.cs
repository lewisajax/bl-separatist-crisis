using TaleWorlds.Core;

namespace SeparatistCrisis.BountyHunting.UI
{
    public class BountiesOnPlayerState : GameState
    {
        public BountyHunterBehavior Behavior { get; set; }

        protected override void OnInitialize() => base.OnInitialize();
        protected override void OnFinalize() => base.OnFinalize();
    }
}