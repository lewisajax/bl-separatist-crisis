using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using SeparatistCrisis.ViewModels;

namespace SeparatistCrisis.ObjectTypes
{
    public class SettlementGroup: MBObjectBase
    {
        public Settlement? MapEntity { get; set; }

        public bool AreBoundEntitiesHidden { get; set; }

        public SCNameplateVM? Nameplate { get; set; }

        // [SaveableField(129)]
        private MBList<Settlement> _boundSettlements;

        private Vec2 _position;

        private TextObject _name;

        private PartyBase? _party;

        public MBReadOnlyList<Settlement> BoundSettlements
        {
            get
            {
                return this._boundSettlements;
            }
        }

        public float Radius { get; private set; }

        public Vec2 Position2D
        {
            get
            {
                return this._position;
            }
            private set
            {
                this._position = value;
            }
        }

        public TextObject Name
        {
            get
            {
                return this._name;
            }
            private set
            {
                this._name = value;
            }
        }

        public static SettlementGroup? CurrentGroup
        {
            get
            {
                Settlement? settlement = null;

                if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.IsSettlement)
                {
                    settlement = PlayerCaptivity.CaptorParty.Settlement;
                }
                if (PlayerEncounter.EncounterSettlement != null)
                {
                    settlement = PlayerEncounter.EncounterSettlement;
                }
                if (MobileParty.MainParty.CurrentSettlement != null)
                {
                    settlement = MobileParty.MainParty.CurrentSettlement;
                }

                if (settlement != null)
                {
                    SettlementGroup group = SettlementGroup.FindFirst((n) => n.PrimarySettlement == settlement);

                    if (group != null)
                    {
                        return group;
                    }
                }

                return null;
            }
        }

        public Settlement PrimarySettlement { get; private set; }

        public IFaction MapFaction
        {
            get
            {
                return this.PrimarySettlement.MapFaction;
            }
        }

        public Clan? OwnerClan
        {
            get
            {
                if (this.PrimarySettlement.Village != null)
                {
                    return this.PrimarySettlement.Village.Bound.OwnerClan;
                }
                if (this.PrimarySettlement.Town != null)
                {
                    return this.PrimarySettlement.Town.OwnerClan;
                }

                return null;
            }
        }

        public bool IsActive { get; set; }

        public PartyBase? Party
        {
            get
            {
                return this.MapEntity?.Party;
            }
            private set
            {
                this._party = value;
            }
        }

        public string EncyclopediaLink
        {
            get
            {
                return (Campaign.Current.EncyclopediaManager.GetIdentifier(typeof(SettlementGroup)) + "-" + base.StringId) ?? "";
            }
        }

        public TextObject EncyclopediaText { get; private set; }


        public TextObject EncyclopediaLinkWithName
        {
            get
            {
                // Need to add own hypertext category
                return HyperlinkTexts.GetSettlementHyperlinkText(this.EncyclopediaLink, this.Name);
            }
        }

        public CultureObject Culture
        {
            get
            {
                return this.PrimarySettlement.Culture;
            }
        }

        public static MBReadOnlyList<SettlementGroup> All
        {
            get
            {
                return MBObjectManager.Instance.GetObjectTypeList<SettlementGroup>();
            }
        }

        public SettlementGroup() : this(new TextObject("{=!}unnamed", null), null, null)
        {
        }

        public SettlementGroup(TextObject name, Settlement? mapEntity, List<Settlement>? childNodes)
        {
            this._name = name;
            this._position = Vec2.Zero;

            if (childNodes != null)
            {
                this._boundSettlements = new MBList<Settlement>(childNodes);
            }
            else
            {
                this._boundSettlements = new MBList<Settlement>();
            }

            if (mapEntity != null)
            {
                this.MapEntity = mapEntity;
            }

            this.AreBoundEntitiesHidden = false;
            this.IsActive = true;
            this.EncyclopediaText = new TextObject("");
        }

        public static SettlementGroup Find(string idString)
        {
            return MBObjectManager.Instance.GetObject<SettlementGroup>(idString);
        }

        public static SettlementGroup? FindFirst(Func<SettlementGroup, bool> predicate)
        {
            return SettlementGroup.All.FirstOrDefault(predicate);
        }

        public static IEnumerable<SettlementGroup> FindAll(Func<SettlementGroup, bool> predicate)
        {
            return SettlementGroup.All.Where(predicate);
        }

        // Can't remember why I wanted this to be internal
        internal void AddBoundSettlementInternal(Settlement settlement)
        {
            this._boundSettlements.Add(settlement);
        }

        internal void RemoveBoundSettlementInternal(Settlement settlement)
        {
            this._boundSettlements.Remove(settlement);
        }

        public Vec3 GetPosition()
        {
            return this.GetLogicalPosition();
        }

        public Vec3 GetLogicalPosition()
        {
            float z = 0f;
            CampaignVec2 campaignVec = new CampaignVec2(this.Position2D, true);
            Campaign.Current.MapSceneWrapper.GetHeightAtPoint(campaignVec, ref z);
            return new Vec3(this.Position2D.x, this.Position2D.y, z, -1f);
        }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            bool isInitialized = base.IsInitialized;
            base.Deserialize(objectManager, node);

            this.Name = new TextObject(node.Attributes["name"].Value, null);
            this.Position2D = new Vec2((float)Convert.ToDouble(node.Attributes["posX"].Value), (float)Convert.ToDouble(node.Attributes["posY"].Value));
            this.Radius = (float)Convert.ToDouble(node.Attributes["radius"].Value);
            this.EncyclopediaText = ((node.Attributes["text"] != null) ? new TextObject(node.Attributes["text"].Value, null) : TextObject.GetEmpty());

            XmlAttribute primaryNode = node.Attributes["primaryNode"];
            if (primaryNode != null)
            {
                this.PrimarySettlement = Settlement.Find(primaryNode.Value);
            }
            else
            {
                throw new Exception();
            }

            foreach (object obj in node.ChildNodes)
            {
                XmlNode xmlNode = (XmlNode)obj;
                if (xmlNode.Name == "Children")
                {
                    foreach (object obj2 in xmlNode.ChildNodes)
                    {
                        XmlNode node2 = (XmlNode)obj2;
                        XmlAttribute boundSettlementId = node2.Attributes["id"];
                        if (boundSettlementId != null)
                        {
                            Settlement boundSettlement = Settlement.Find(boundSettlementId.Value);
                            this.AddBoundSettlementInternal(boundSettlement);
                        }
                    }
                }
            }
        }
    }
}
