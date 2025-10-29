using SeparatistCrisis.PartyVisuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.ObjectTypes
{
    public class AssignedSpaceShip : MBObjectBase
    {
        public CharacterObject _character;
        public SpaceShip _ship;

        public CharacterObject Character
        {
            get
            {
                return this._character;
            }
        }

        public SpaceShip Ship
        {
            get
            {
                return this._ship;
            }
        }

        private static SpaceShip? GetDefaultShip(PartyBase party, IAssignment defaults)
        {
            if (defaults.PatrolDefault != null)
            {
                if (party.MobileParty.IsVillager || party.MobileParty.IsPatrolParty || party.MobileParty.IsCaravan)
                    return defaults.PatrolDefault;
            }

            return null;
        }

        /* PartyBase.LeaderHero is null
        .General is null
        .Owner is the faction leader of this village's kingdom/clan??
        .MapFaction would be the kingdom or the independent clan

        We can't get the village party's clan directly from PartyBase, only the independent clan/kingdom and the culture
        If we want the clan, we need to go into PartyBase.MobileParty.HomeSettlement.OwnerClan */
        public static SpaceShip? GetShip(PartyBase party)
        {
            DefaultShipAssignments Assignments = DefaultShipAssignments.Instance;
            SpaceShip? ship = null;

            Clan? clan = party?.MobileParty?.HomeSettlement?.OwnerClan;

            if (clan == null)
                return null;

            ClanDefaults? clanDefaults = Assignments.GetAssignment(clan);
            if (clanDefaults != null)
            {
                ship = GetDefaultShip(party, clanDefaults);
                if (ship != null)
                    return ship;
            }

            IFaction faction = party.MapFaction;
            KingdomDefaults? kingdomDefaults = faction is Kingdom ? Assignments.GetAssignment(faction as Kingdom) : null;
            if (kingdomDefaults != null)
            {
                ship = GetDefaultShip(party, kingdomDefaults);
                if (ship != null)
                    return ship;
            }

            CultureDefaults? cultureDefaults = null;
            if (clan.Kingdom != null && Assignments.ShipDefaults.TryGetValue(clan.Kingdom.Culture, out CultureDefaults? culDef1))
                cultureDefaults = culDef1;
            else if (clan.Kingdom == null && Assignments.ShipDefaults.TryGetValue(clan.Culture, out CultureDefaults? culDef2))
                cultureDefaults = culDef2;

            if (cultureDefaults != null)
            {
                ship = GetDefaultShip(party, cultureDefaults);
                if (ship != null)
                    return ship;
            }

            // Somewhere else needs to decide on a default ship model
            return null;
        }

        public static SpaceShip? GetShip(CharacterObject character)
        {
            DefaultShipAssignments Assignments = DefaultShipAssignments.Instance;

            AssignedSpaceShip? assignedShip = Assignments.GetAssignment(character);
            if (assignedShip != null)
                return assignedShip.Ship;

            // If the clan prop is null on the hero object, then we have bigger problems
            Clan? clan = character?.HeroObject?.Clan;

            if (clan == null)
                return null;

            ClanDefaults? clanDefaults = clan != null ? Assignments.GetAssignment(clan) : null;
            if (clanDefaults != null && clanDefaults.LordDefault != null)
                return clanDefaults.LordDefault;

            KingdomDefaults? kingdomDefaults = clan.Kingdom != null ? Assignments.GetAssignment(clan.Kingdom) : null;
            if (kingdomDefaults != null && kingdomDefaults.LordDefault != null)
                return kingdomDefaults.LordDefault;

            CultureDefaults? cultureDefaults = null;
            if (clan.Kingdom != null && Assignments.ShipDefaults.TryGetValue(clan.Kingdom.Culture, out CultureDefaults? culDef1))
                cultureDefaults = culDef1;
            else if (clan.Kingdom == null && Assignments.ShipDefaults.TryGetValue(clan.Culture, out CultureDefaults? culDef2))
                cultureDefaults = culDef2;

            if (cultureDefaults != null && cultureDefaults.LordDefault != null)
                return cultureDefaults.LordDefault;

            // Somewhere else needs to decide on a default ship model
            return null;
        }

        // It might be worth loading the xml ourselves instead of going through MBObjectManager but atm it's convenient to use TW's xml parser
        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            DefaultShipAssignments Assignments =  DefaultShipAssignments.Instance;

            // I can't be arsed to set up different object types and files for these
            if (!Assignments.HasDeserializedDefaults)
            {
                XmlNode? culturesNode = node?.ParentNode?.SelectSingleNode(".//CultureSpaceShips");
                XmlNode? kingdomsNode = node?.ParentNode?.SelectSingleNode(".//KingdomSpaceShips");
                XmlNode? clansNode = node?.ParentNode?.SelectSingleNode(".//ClanSpaceShips");

                if (culturesNode != null)
                    Assignments.DeserializeDefaults<IFaction, IFactionAssignment>(objectManager, culturesNode, new CultureDefaults());

                if (kingdomsNode != null)
                    Assignments.DeserializeDefaults<Clan, ClanDefaults>(objectManager, kingdomsNode, new KingdomDefaults());

                if (clansNode != null)
                    Assignments.DeserializeDefaults<CharacterObject, AssignedSpaceShip>(objectManager, clansNode, new ClanDefaults());

                Assignments.HasDeserializedDefaults = true;
            }

            // Big ol' clusterfuck
            base.Deserialize(objectManager, node);
            XmlAttributeCollection? attributes = node?.Attributes;
            if (attributes != null)
            {
                XmlAttribute? characterAttribute = attributes["character"];
                XmlAttribute? shipAttribute = attributes["ship"];

                if (characterAttribute == null || shipAttribute == null)
                {
                    Debug.FailedAssert("The character or ship attribute is null", "C:\\SeparatistCrisis\\ObjectTypes\\AssignedSpaceShip.cs", "Deserialize");
                    MBObjectManager.Instance.UnregisterObject(this);
                    return;
                }

                CharacterObject character = MBObjectManager.Instance.GetObject<CharacterObject>(characterAttribute.Value);
                string shipName = shipAttribute.Value.Contains('.') ? shipAttribute.Value.Split(new char[] { '.' })[1] : shipAttribute.Value;
                SpaceShip ship = MBObjectManager.Instance.GetObject<SpaceShip>(shipName);

                this._character = character;
                this._ship = ship;

                Clan clan = character.HeroObject.Clan;
                CultureObject culture = clan.MapFaction is Clan ? clan.Culture : clan.Kingdom.Culture;

                // If there's no culture defaults then we make one
                if (!Assignments.ShipDefaults.TryGetValue(culture, out CultureDefaults? cultureDefaults))
                {
                    CultureDefaults newCulDefaults = new CultureDefaults();
                    newCulDefaults.Culture = culture;
                    cultureDefaults = newCulDefaults;
                    Assignments.ShipDefaults.Add(culture, cultureDefaults);
                }

                // If the clan is a part of a kingdom and there's no kingdom defaults
                if (clan.Kingdom != null && !cultureDefaults.Children.ContainsKey(clan.Kingdom))
                {
                    KingdomDefaults newKinDefaults = new KingdomDefaults();
                    newKinDefaults.Kingdom = clan.Kingdom;
                    cultureDefaults.Children.Add(clan.Kingdom, newKinDefaults);
                }

                // If the clan is a part of the kingdom but there's no clan assignment in the kingdom's
                if (clan.Kingdom != null && !((KingdomDefaults)cultureDefaults[clan.Kingdom]).Children.ContainsKey(clan))
                {
                    ClanDefaults newClanDefaults = new ClanDefaults();
                    newClanDefaults.Clan = clan;
                    newClanDefaults.Children.Add(character, this);
                    ((KingdomDefaults)cultureDefaults[clan.Kingdom]).Children.Add(clan, newClanDefaults);
                }
                else if (clan.Kingdom != null)
                {
                    // If the clan defaults is already in the kingdom's defaults, then just add it to the source
                    ClanDefaults clanDefaults = ((KingdomDefaults)Assignments.ShipDefaults[culture][clan.Kingdom])[clan];
                    clanDefaults.Children.Add(character, this);
                }

                // Independent clans
                if (clan.Kingdom == null && !cultureDefaults.Children.ContainsKey(clan))
                {
                    ClanDefaults newClanDefaults = new ClanDefaults();
                    newClanDefaults.Clan = clan;
                    newClanDefaults.Children.Add(character, this);
                    cultureDefaults.Children.Add(clan, newClanDefaults);
                }
                else if (clan.Kingdom == null)
                {
                    ClanDefaults clanDefaults = (ClanDefaults)Assignments.ShipDefaults[culture][clan];
                    clanDefaults.Children.Add(character, this);
                }
            }
        }
    }
}
