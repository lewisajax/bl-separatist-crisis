using SeparatistCrisis.Tactics.VisualOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.VisualOrders;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders;
using TaleWorlds.MountAndBlade.View.VisualOrders.Orders.ToggleOrders;
using TaleWorlds.MountAndBlade.View.VisualOrders.OrderSets;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.FormOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.MovementOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual.Default.Orders.ToggleOrders;

namespace SeparatistCrisis.Tactics
{
    public class SCVisualOrderProvider: VisualOrderProvider
    {
        // The DefaultVisualOrderProvider recreates the order sets every time we open the order menu.
        // It doesn't cache it's orders. That might be for good reason though, we'll see.
        private MBReadOnlyList<VisualOrderSet>? _cachedOrderSets;

        public MBReadOnlyList<VisualOrderSet>? CachedOrderSets
        {
            get { return this._cachedOrderSets; }
            protected set 
            { 
                if (this._cachedOrderSets != value) 
                    this._cachedOrderSets = value; 
            }
        }

        public override bool IsAvailable()
        {
            return Mission.Current != null && !Mission.Current.IsFriendlyMission;
        }

        public override MBReadOnlyList<VisualOrderSet> GetOrders()
        {
            if (CachedOrderSets == null)
            {
                this.CachedOrderSets = this.GetDefaultOrders();
            }

            return CachedOrderSets;
        }

        protected MBReadOnlyList<VisualOrderSet> GetDefaultOrders()
        {
            DefaultVisualOrderProvider defaultProvider = new DefaultVisualOrderProvider();
            MBReadOnlyList<VisualOrderSet> orderSets = defaultProvider.GetOrders();

            for (int i = 0; i < orderSets.Count; i++)
            {
                if (orderSets[i].StringId == "order_type_toggle")
                {
                    MBReadOnlyList<VisualOrder> orders = orderSets[i].Orders;

                    // Remove the 'Return' order
                    if (orders.Count > 0)
                        orderSets[i].RemoveOrder(orders[orders.Count - 1]);

                    orderSets[i].AddOrder(new UseCoverVisualOrder("order_movement_charge"));
                    orderSets[i].AddOrder(new ReturnVisualOrder());
                }
            }

            return orderSets;
        }

        public void Dispose()
        {
            this.CachedOrderSets = null;
        }
    }
}
