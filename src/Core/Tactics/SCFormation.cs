using SeparatistCrisis.Tactics.OrderStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics
{
    public class SCFormation
    {
        private Formation _formation;

        public Formation Formation
        {
            get { return this._formation; }
            protected set 
            { 
                if (value != null && this._formation != value)
                    this._formation = value; 
            }
        }

        public CoverOrder CoverOrder { get; protected set; }

        public SCFormation(Formation formation) 
        {
            this._formation= formation;
        }

        public void SetCoverOrder(CoverOrder order)
        {
            if (this.CoverOrder != order)
            {
                this.CoverOrder = order;
                this.Formation.ApplyActionOnEachUnit(delegate (Agent agent)
                {
                    // This is where we can start looking for the closest cover area maybe
                    // agent.SetFiringOrder(order.OrderEnum);
                }, null);
            }
        }
    }
}
