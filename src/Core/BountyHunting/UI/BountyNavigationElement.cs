using SandBox.View.Map.Navigation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace SeparatistCrisis.BountyHunting.UI
{
    /// <summary>
    /// Map nav-bar entry for the bounty board, injected via Harmony postfix on
    /// MapNavigationHandler.OnCreateElements. Mirrors QuestsNavigationElement's shape.
    /// </summary>
    public class BountyNavigationElement : MapNavigationElementBase
    {
        public override string StringId => "bounty";

        public override bool IsActive => _game.GameStateManager.ActiveState is BountyBoardState;

        public override bool IsLockingNavigation => false;

        public override bool HasAlert => false; 

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
            return new TextObject("{=BountyNavTooltip}Bounty Board");
        }

        protected override TextObject GetAlertTooltip()
        {
            return TextObject.GetEmpty();
        }

        public override void OpenView()
        {
            var state = _game.GameStateManager.CreateState<BountyBoardState>();
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