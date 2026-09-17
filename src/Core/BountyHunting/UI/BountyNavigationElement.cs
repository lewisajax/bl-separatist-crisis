// BountyNavigationElement.cs
using SandBox.View.Map.Navigation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// Map nav-bar entry for the player's own bounty status, injected via Harmony
    /// postfix on MapNavigationHandler.OnCreateElements. Mirrors
    /// QuestsNavigationElement's shape. Opens BountiesOnPlayerState — the "Bounties
    /// on you" screen — separate from the town-menu bounty board (BountyBoardState).
    /// </summary>
    public class BountyNavigationElement : MapNavigationElementBase
    {
        public override string StringId => "bounty";

        public override bool IsActive => _game.GameStateManager.ActiveState is BountiesOnPlayerState;

        public override bool IsLockingNavigation => false;

        public override bool HasAlert => false; // wire to "you have an active bounty" later if wanted

        public BountyNavigationElement(MapNavigationHandler handler) : base(handler)
        {
        }

        protected override NavigationPermissionItem GetPermission()
        {
            if (IsActive)
            {
                return new NavigationPermissionItem(false, null);
            }
            return new NavigationPermissionItem(true, null);
        }

        protected override TextObject GetTooltip()
        {
            return new TextObject("{=BountyNavTooltip}Bounties on you");
        }

        protected override TextObject GetAlertTooltip()
        {
            return TextObject.GetEmpty();
        }

        public override void OpenView()
        {
            var state = _game.GameStateManager.CreateState<BountiesOnPlayerState>();
            state.Behavior = Campaign.Current.GetCampaignBehavior<BountyHunterBehavior>();
            _game.GameStateManager.PushState(state, 0);
        }

        public override void OpenView(params object[] parameters)
        {
            OpenView();
        }

        public override void GoToLink()
        {
        }
    }
}