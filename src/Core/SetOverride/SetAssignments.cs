using NetworkMessages.FromClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.SetOverride
{
    public class SetAssignments
    {
        public static SetAssignments Instance { get; } = new SetAssignments();

        public bool HasDeserializedAssignments { get; set; }

        // If we want the player to add custom set assignments in-game, we'll probably want to scrap this and go with an MBObject to make saving the data easier.
        // We'll probably move from using singletons if we start writing tests but for now this makes it easier to prototype stuff
        public Dictionary<BasicCharacterObject, Tuple<BasicCultureObject, int>[]> Lords { get; private set; } = new Dictionary<BasicCharacterObject, Tuple<BasicCultureObject, int>[]>();
        public Dictionary<IFaction, Tuple<BasicCultureObject, int>[]> Clans { get; private set; } = new Dictionary<IFaction, Tuple<BasicCultureObject, int>[]>();

        public int GetSetIndex(BasicCharacterObject npc, BasicCultureObject culture)
        {
            if (this.Lords.TryGetValue(npc, out Tuple<BasicCultureObject, int>[]? sets))
            {
                if (sets != null)
                {
                    for (int i = 0; i < sets.Length; i++)
                    {
                        (BasicCultureObject cul, int ind) = sets[i];
                        if (cul == culture) return ind;
                    }
                }
            }

            return 0;
        }

        public int GetSetIndex(IFaction fac, BasicCultureObject culture)
        {
            if (this.Clans.TryGetValue(fac, out Tuple<BasicCultureObject, int>[]? sets))
            {
                if (sets != null)
                {
                    for (int i = 0; i < sets.Length; i++)
                    {
                        (BasicCultureObject cul, int ind) = sets[i];
                        if (cul == culture) return ind;
                    }
                }
            }

            return 0;
        }

        public void Deserialize(MBObjectManager objectManager, XmlNode parentNode)
        {
            if (parentNode == null) return;

            foreach (XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name != "AssignedHeroSet" && node.Name != "AssignedClanSet")
                    continue;

                XmlAttributeCollection? attributes = node?.Attributes;
                if (attributes != null)
                {
                    if (node.Name == "AssignedHeroSet")
                    {
                        this.DeserializeLords(attributes, node);
                        continue;
                    }

                    // We don't have access to the clans in custom game
                    if (Game.Current.GameType.GetType() == typeof(Campaign))
                    {
                        if (node.Name == "AssignedClanSet")
                        {
                            this.DeserializeClans(attributes, node);
                            continue;
                        }
                    }
                }
            }
        }

        public void DeserializeSets(List<Tuple<BasicCultureObject, int>> sets, XmlNode parentNode)
        {
            char[] entitySplit = new char[] { '.' };
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                if (node.Name != "Set")
                    continue;

                int index = 0;

                XmlAttributeCollection? attributes = node?.Attributes;
                if (attributes != null)
                {
                    XmlAttribute? indexAttribute = attributes["index"];
                    XmlAttribute? cultureAttribute = attributes["culture"];

                    if (indexAttribute == null || cultureAttribute == null)
                    {
                        Debug.FailedAssert("The index or culture attribute is null", "C:\\SeparatistCrisis\\SetOverride\\SetAssignments.cs", "DeserializeSets");
                        return;
                    }

                    if (int.TryParse(indexAttribute.Value, out int result))
                        index = result;

                    string cultureName = cultureAttribute.Value.Contains('.') ? cultureAttribute.Value.Split(entitySplit)[1] : cultureAttribute.Value;
                    BasicCultureObject? culture = MBObjectManager.Instance.GetObject<BasicCultureObject>(cultureName);

                    if (culture != null)
                        sets.Add(Tuple.Create(culture, index));
                }
            }
        }

        public void DeserializeLords(XmlAttributeCollection attributes, XmlNode node)
        {
            char[] entitySplit = new char[] { '.' };
            XmlAttribute? charAttribute = attributes["character_id"];

            if (charAttribute == null)
            {
                Debug.FailedAssert("The character attribute is null", "C:\\SeparatistCrisis\\SetOverride\\SetAssignments.cs", "DeserializeLordSets");
                return;
            }

            string charName = charAttribute.Value.Contains('.') ? charAttribute.Value.Split(entitySplit)[1] : charAttribute.Value;
            BasicCharacterObject? charObj = MBObjectManager.Instance.GetObject<BasicCharacterObject>(charName);

            if (charObj != null)
            {
                // If there's already an array of sets in the dictionary, then we use that instead of creating a new list
                bool setsExists = this.Lords.ContainsKey(charObj);
                List<Tuple<BasicCultureObject, int>> sets = setsExists ? this.Lords[charObj].ToList() : new List<Tuple<BasicCultureObject, int>>();
                this.DeserializeSets(sets, node);
                
                if (setsExists)
                {
                    this.Lords[charObj] = sets.ToArray();
                } 
                else
                {
                    this.Lords.Add(charObj, sets.ToArray());
                }
            }
        }

        public void DeserializeClans(XmlAttributeCollection attributes, XmlNode node)
        {
            char[] entitySplit = new char[] { '.' };
            XmlAttribute? clanAttribute = attributes["clan_id"];

            if (clanAttribute == null)
            {
                Debug.FailedAssert("The clan attribute is null", "C:\\SeparatistCrisis\\SetOverride\\SetAssignments.cs", "DeserializeClans");
                return;
            }

            string clanName = clanAttribute.Value.Contains('.') ? clanAttribute.Value.Split(entitySplit)[1] : clanAttribute.Value;
            Clan? clanObj = MBObjectManager.Instance.GetObject<Clan>(clanName);

            if (clanObj != null)
            {
                // If there's already an array of sets in the dictionary, then we use that instead of creating a new list
                bool setsExists = this.Clans.ContainsKey(clanObj);
                List<Tuple<BasicCultureObject, int>> sets = setsExists ? this.Clans[clanObj].ToList() : new List<Tuple<BasicCultureObject, int>>();
                this.DeserializeSets(sets, node);

                if (setsExists)
                {
                    this.Clans[clanObj] = sets.ToArray();
                }
                else
                {
                    this.Clans.Add(clanObj, sets.ToArray());
                }
            }
        }

        public void Dispose()
        {
            this.Lords.Clear();
            this.Clans.Clear();
            this.HasDeserializedAssignments = false;
        }
    }
}
