using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map.DistanceCache;
using TaleWorlds.Library;

namespace SeparatistCrisis.Scenes.DistanceCache
{
    public class SCSettlementRecord : ISettlementDataHolder
    {
        public SCSettlementRecord(string settlementId, Vec2 position, Vec2 gatePosition, XmlNode node, bool hasGate, Vec2 portPosition, bool hasPort, bool isFortification)
        {
            this.SettlementId = settlementId;
            this.Position = position;
            this.GatePosition = gatePosition;
            this.Node = node;
            this.HasGate = hasGate;
            this.PortPosition = portPosition;
            this.HasPort = hasPort;
            this.IsFortification = isFortification;
        }

        public string StringId
        {
            get
            {
                return this.SettlementId;
            }
        }

        CampaignVec2 ISettlementDataHolder.GatePosition
        {
            get
            {
                return new CampaignVec2(this.GatePosition, true);
            }
        }

        CampaignVec2 ISettlementDataHolder.PortPosition
        {
            get
            {
                return new CampaignVec2(this.PortPosition, false);
            }
        }

        bool ISettlementDataHolder.IsFortification
        {
            get
            {
                return this.IsFortification;
            }
        }

        bool ISettlementDataHolder.HasPort
        {
            get
            {
                return this.HasPort;
            }
        }

        public readonly string SettlementId;

        public readonly XmlNode Node;

        public readonly Vec2 Position;

        public readonly Vec2 GatePosition;

        public readonly bool HasGate;

        public readonly Vec2 PortPosition;

        public readonly bool HasPort;

        public readonly bool IsFortification;
    }
}
