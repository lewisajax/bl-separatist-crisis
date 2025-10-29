using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.PartyVisuals
{
    public class DefaultShipAssignments
    {
        public static DefaultShipAssignments Instance { get; } = new DefaultShipAssignments();

        public bool HasDeserializedDefaults { get; set; }

        public Dictionary<CultureObject, CultureDefaults> ShipDefaults { get; private set; } = new Dictionary<CultureObject, CultureDefaults>();

        public KingdomDefaults? GetAssignment(Kingdom kingdom)
        {
            if (kingdom != null)
                if (this.ShipDefaults.TryGetValue(kingdom.Culture, out CultureDefaults? cultureDefaults))
                    if (cultureDefaults.Children.TryGetValue(kingdom, out IFactionAssignment? kingdomDefaults))
                        return kingdomDefaults as KingdomDefaults;

            return null;
        }

        public ClanDefaults? GetAssignment(Clan clan)
        {
            if (clan?.Kingdom != null)
            {
                if (clan.Kingdom?.Culture == null)
                    return null;

                KingdomDefaults? kinDefaults = null;
                if (this.ShipDefaults.TryGetValue(clan.Kingdom.Culture, out CultureDefaults? cultureDefaults))
                    if (cultureDefaults.Children.TryGetValue(clan.Kingdom, out IFactionAssignment? kingdomDefaults))
                        kinDefaults = kingdomDefaults as KingdomDefaults;

                return kinDefaults != null ? kinDefaults[clan] : null;
            }
            else
            {
                if (clan?.Culture == null)
                    return null;

                if (this.ShipDefaults.TryGetValue(clan.Culture, out CultureDefaults? cultureDefaults))
                    if (cultureDefaults.Children.TryGetValue(clan, out IFactionAssignment? clanDefaults))
                        return (ClanDefaults)cultureDefaults[clan];

                return null;
            }
        }

        public AssignedSpaceShip? GetAssignment(CharacterObject character)
        {
            if (character != null && character.HeroObject.Clan != null)
                return GetAssignment(character.HeroObject.Clan)?[character];

            return null;
        }

        public void DeserializeDefaults<TObject, TAssignment>(MBObjectManager objectManager, XmlNode parentNode, DefaultAssignment<TObject, TAssignment> assignment)
        {
            if (parentNode == null) return;

            foreach (XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name != assignment?.NodeName)
                    continue;

                char[] entitySplit = new char[] { '.' };

                XmlAttributeCollection? attributes = node?.Attributes;
                if (attributes != null)
                {
                    if (assignment is CultureDefaults)
                    {
                        this.DeserializeCultureDefaults(attributes, assignment as CultureDefaults);
                        continue;
                    }

                    if (assignment is KingdomDefaults)
                    {
                        this.DeserializeKingdomDefaults(attributes, assignment as KingdomDefaults);
                        continue;
                    }

                    if (assignment is ClanDefaults)
                    {
                        this.DeserializeClanDefaults(attributes, assignment as ClanDefaults);
                        continue;
                    }
                }
            }
        }

        public void DeserializeCultureDefaults(XmlAttributeCollection attributes, CultureDefaults assignment)
        {
            char[] entitySplit = new char[] { '.' };
            XmlAttribute? cultureAttribute = attributes["culture"];
            XmlAttribute? lordAttribute = attributes["lord_default"];
            XmlAttribute? patrolAttribute = attributes["patrol_default"];

            CultureObject culture = MBObjectManager.Instance.GetObject<CultureObject>(cultureAttribute.Value);
            string lordShipName = lordAttribute.Value.Contains('.') ? lordAttribute.Value.Split(entitySplit)[1] : lordAttribute.Value;
            string patrolShipName = patrolAttribute.Value.Contains('.') ? patrolAttribute.Value.Split(entitySplit)[1] : patrolAttribute.Value;
            SpaceShip? lordShip = MBObjectManager.Instance.GetObject<SpaceShip>(lordShipName);
            SpaceShip? patrolShip = MBObjectManager.Instance.GetObject<SpaceShip>(patrolShipName);

            if (culture != null && lordShip != null && patrolShip != null)
            {
                // If there's already a basic culture assigment in the dictionary from parsing AssignedSpaceShips, then we use that instead of the assignment passed to this function
                bool assignmentExists = this.ShipDefaults.ContainsKey(culture);
                CultureDefaults cultureDefaults = assignmentExists ? this.ShipDefaults[culture] : assignment;
                cultureDefaults.Culture = culture;
                cultureDefaults.LordDefault = lordShip;
                cultureDefaults.PatrolDefault = patrolShip;
                if (!assignmentExists)
                    this.ShipDefaults.Add(culture, cultureDefaults);
            }
        }

        public void DeserializeKingdomDefaults(XmlAttributeCollection attributes, KingdomDefaults assignment)
        {
            char[] entitySplit = new char[] { '.' };
            XmlAttribute? kingdomAttribute = attributes["kingdom"];
            XmlAttribute? lordAttribute = attributes["lord_default"];

            Kingdom kingdom = MBObjectManager.Instance.GetObject<Kingdom>(kingdomAttribute.Value);
            string lordShipName = lordAttribute.Value.Contains('.') ? lordAttribute.Value.Split(entitySplit)[1] : lordAttribute.Value;
            SpaceShip? lordShip = MBObjectManager.Instance.GetObject<SpaceShip>(lordShipName);

            if (kingdom != null && lordShip != null)
            {
                KingdomDefaults kingdomDefaults = assignment;
                kingdomDefaults.Kingdom = kingdom;
                kingdomDefaults.LordDefault = lordShip;

                // If there's a kingdom default. If not, then we create a basic culture assignment to add to the dictionary
                // All cultures should have default ships but not all kingdoms need defaults
                if (this.ShipDefaults.TryGetValue(kingdom.Culture, out CultureDefaults? cultureDefaults))
                {
                    if (cultureDefaults != null && cultureDefaults.Children.ContainsKey(kingdom))
                    {
                        KingdomDefaults existingKinAss = (KingdomDefaults)cultureDefaults[kingdom];
                        existingKinAss.Kingdom = kingdom;
                        existingKinAss.LordDefault = lordShip;
                    }
                    else
                    {
                        cultureDefaults?.Children.Add(kingdom, kingdomDefaults);
                    }
                }
                else
                {
                    CultureDefaults newCulDefaults = new CultureDefaults();
                    newCulDefaults.Culture = kingdom.Culture;
                    newCulDefaults.Children.Add(kingdom, kingdomDefaults);
                    this.ShipDefaults.Add(kingdom.Culture, newCulDefaults);
                }
            }
        }

        public void DeserializeClanDefaults(XmlAttributeCollection attributes, ClanDefaults assignment)
        {
            char[] entitySplit = new char[] { '.' };
            XmlAttribute? clanAttribute = attributes["clan"];
            XmlAttribute? lordAttribute = attributes["lord_default"];

            Clan clan = MBObjectManager.Instance.GetObject<Clan>(clanAttribute.Value);
            string lordShipName = lordAttribute.Value.Contains('.') ? lordAttribute.Value.Split(entitySplit)[1] : lordAttribute.Value;
            SpaceShip? lordShip = MBObjectManager.Instance.GetObject<SpaceShip>(lordShipName);

            if (clan != null && lordShip != null)
            {
                ClanDefaults clanDefaults = assignment;
                clanDefaults.Clan = clan;
                clanDefaults.LordDefault = lordShip;

                // If clan is independent, use the clan's culture else use the clan's kingdom's culture
                CultureObject culture = clan.MapFaction is Clan ? clan.Culture : clan.Kingdom.Culture;

                // Check if there's an existing culture assignment
                if (!this.ShipDefaults.ContainsKey(culture))
                {
                    CultureDefaults newCulDefaults = new CultureDefaults();
                    newCulDefaults.Culture = culture;
                    this.ShipDefaults.Add(culture, newCulDefaults);
                }

                // Check if the clan is not independent and that there's not an existing kingdom assignment
                if (clan.Kingdom != null && !this.ShipDefaults[culture].Children.ContainsKey(clan.Kingdom))
                {
                    KingdomDefaults newKinDefaults = new KingdomDefaults();
                    newKinDefaults.Kingdom = clan.Kingdom;
                    newKinDefaults.Children.Add(clan, clanDefaults);
                    this.ShipDefaults[culture].Children.Add(clan.Kingdom, newKinDefaults);
                }

                // Independent clans
                if (clan.Kingdom == null && !this.ShipDefaults[culture].Children.ContainsKey(clan))
                {
                    this.ShipDefaults[culture].Children.Add(clan, clanDefaults);
                }
                else if (clan.Kingdom != null)
                {
                    KingdomDefaults kingdomDefaults = (KingdomDefaults)this.ShipDefaults[culture][clan.Kingdom];
                    if (!kingdomDefaults.Children.ContainsKey(clan))
                    {
                        kingdomDefaults.Children.Add(clan, clanDefaults);
                    }
                    else
                    {
                        ClanDefaults existingClanAss = kingdomDefaults[clan];
                        existingClanAss.Clan = clan;
                        existingClanAss.LordDefault = lordShip;
                    }
                }
            }
        }
    }

    public interface IAssignment
    {
        public abstract string NodeName { get; }
        public SpaceShip? LordDefault { get; set; }
        public SpaceShip? PatrolDefault { get; set; }
    }

    public interface IFactionAssignment { }

    public abstract class DefaultAssignment<TObject, TAssignment> : IAssignment
    {
        public TAssignment this[TObject obj]
        {
            get => this.Children[obj];
        }

        public abstract string NodeName { get; set; }
        public virtual SpaceShip? LordDefault { get; set; }

        public virtual SpaceShip? PatrolDefault { get; set; }

        public Dictionary<TObject, TAssignment> Children { get; } = new Dictionary<TObject, TAssignment>();

        public virtual TAssignment GetChild(TObject obj)
        {
            return this.Children[obj];
        }
    }

    public class CultureDefaults : DefaultAssignment<IFaction, IFactionAssignment>
    {
        public override string NodeName
        {
            get => "CultureSpaceShip";
            set => throw new NotImplementedException();
        }

        public CultureObject Culture { get; set; } = null!;

        public override SpaceShip? LordDefault { get; set; }

        public override SpaceShip? PatrolDefault { get; set; }
    }

    // There will be an issue if a clan moves to a different kingdom.
    // If the clan has it's own defaults, it would fall back to their new kingdom's defaults if we don't update the assignment dict
    public class KingdomDefaults : DefaultAssignment<Clan, ClanDefaults>, IFactionAssignment
    {
        public override string NodeName
        {
            get => "KingdomSpaceShip";
            set => throw new NotImplementedException();
        }
        public Kingdom Kingdom { get; set; } = null!;

        public override SpaceShip? LordDefault { get; set; }
    }

    public class ClanDefaults : DefaultAssignment<CharacterObject, AssignedSpaceShip>, IFactionAssignment
    {
        public override string NodeName
        {
            get => "ClanSpaceShip";
            set => throw new NotImplementedException();
        }

        public Clan Clan { get; set; } = null!;

        public override SpaceShip? LordDefault { get; set; }
    }
}
