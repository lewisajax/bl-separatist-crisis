using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics.OrderStructs
{
    public struct CoverOrder : IEquatable<CoverOrder>
    {
        private CoverOrder(SCOrderType orderEnum)
        {
            this.OrderEnum = orderEnum;
        }

        public SCOrderType OrderType
        {
            get
            {
                if (this.OrderEnum != SCOrderType.LeaveCover)
                {
                    return SCOrderType.UseCover;
                }
                return SCOrderType.LeaveCover;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CoverOrder)
            {
                CoverOrder f = (CoverOrder)obj;
                return f == this;
            }
            return false;
        }

        public bool Equals(CoverOrder other)
        {
            CoverOrder f = other;
            return f == this;
        }

        public override int GetHashCode()
        {
            return (int)this.OrderEnum;
        }

        public static bool operator !=(CoverOrder f1, CoverOrder f2)
        {
            return f1.OrderEnum != f2.OrderEnum;
        }

        public static bool operator ==(CoverOrder f1, CoverOrder f2)
        {
            return f1.OrderEnum == f2.OrderEnum;
        }

        public readonly SCOrderType OrderEnum;

        public static readonly CoverOrder CoverOrderUseCover = new CoverOrder(SCOrderType.UseCover);

        public static readonly CoverOrder CoverOrderLeaveCover = new CoverOrder(SCOrderType.LeaveCover);
    }
}
