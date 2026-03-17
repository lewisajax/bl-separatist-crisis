using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics
{
    public class SCOrderController : OrderController
    {
        SCMissionCombatantsLogic _missionCombatantsLogic;


        public event OnSCOrderIssuedDelegate OnSCOrderIssued;

        public delegate void OnSCOrderIssuedDelegate(SCOrderType orderType, MBReadOnlyList<SCFormation> appliedFormations, SCOrderController orderController, params object[] delegateParams);

        public SCOrderController(Mission mission, Team team, Agent owner) : base(mission, team, owner)
        {
            // Every Mission that uses Teams should also have an SCMissionCombatantsLogic applied to it
            this._missionCombatantsLogic = (SCMissionCombatantsLogic)Mission.Current.GetMissionBehavior<MissionCombatantsLogic>();
        }

        public override void SetOrder(OrderType orderType)
        {
            base.SetOrder(orderType);
        }

        public void SetOrder(SCOrderType orderType)
        {
            switch(orderType)
            {
                case SCOrderType.UseCover:
                    break;
                case SCOrderType.LeaveCover:
                    break;
                default:
                    Debug.FailedAssert("[DEBUG]Invalid order type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\AI\\OrderController.cs", "SetOrder", 620);
                    break;
            }

            MBList<SCFormation> appliedSCFormations = new MBList<SCFormation>();
            for (int i = 0; i < this.SelectedFormations.Count; i++)
            {
                SCFormation? scFormation = this._missionCombatantsLogic.FormationDecorators.Find((x) => x.Formation == this.SelectedFormations[i]);
                if (scFormation != null)
                    appliedSCFormations.Add(scFormation);
            }

            this.AfterSetOrder(orderType);
            this.FireOnSCOrderIssued(orderType, new MBReadOnlyList<SCFormation>(appliedSCFormations), this, Array.Empty<object>());
        }

        public void SetUseCoverOrder()
        {
            using (List<Formation>.Enumerator enumerator = this.SelectedFormations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Formation formation = enumerator.Current;
                    SCFormation scFormation = this._missionCombatantsLogic.FormationDecorators.Find((x) => x.Formation == formation);
                    SCOrderController.TryCancelStopOrder(formation);
                    formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
                }
            }
        }

        public void AfterSetOrder(SCOrderType orderType)
        {

        }

        // Why make a static method private??
        public static void TryCancelStopOrder(Formation formation)
        {
            if (!GameNetwork.IsClientOrReplay && formation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Stop)
            {
                WorldPosition position = formation.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None);
                if (position.IsValid)
                {
                    formation.SetMovementOrder(MovementOrder.MovementOrderMove(position));
                }
            }
        }

        protected void FireOnSCOrderIssued(SCOrderType orderType, MBReadOnlyList<SCFormation> appliedFormations, SCOrderController orderController, params object[] delegateParams)
        {
            if (appliedFormations == null)
                return;

            if (this.OnSCOrderIssued != null)
            {
                this.OnSCOrderIssued(orderType, appliedFormations, orderController, delegateParams);
            }
        }
    }
}
