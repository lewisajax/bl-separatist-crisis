using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;

namespace SeparatistCrisis.Tactics.VisualOrders
{
    public class UseCoverVisualOrder: VisualOrder
    {
        public UseCoverVisualOrder(string iconId) : base(iconId)
        {
        }

        public override TextObject GetName(OrderController orderController)
        {
            return new TextObject("{=Dxmq32qW}Use Cover", null);
        }

        public override void ExecuteOrder(OrderController orderController, VisualOrderExecutionParameters executionParameters)
        {
            if (executionParameters.HasFormation)
            {
                orderController.SetOrderWithFormation(OrderType.Charge, executionParameters.Formation);
                return;
            }
            orderController.SetOrder(OrderType.Charge);
        }

        public override bool IsTargeted()
        {
            return true;
        }

        protected override bool? OnGetFormationHasOrder(Formation formation)
        {
            OrderType activeMovementOrderOf = OrderController.GetActiveMovementOrderOf(formation);
            return new bool?(activeMovementOrderOf == OrderType.Charge || activeMovementOrderOf == OrderType.ChargeWithTarget);
        }
    }
}
