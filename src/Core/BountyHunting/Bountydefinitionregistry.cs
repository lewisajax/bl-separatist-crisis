using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace SeparatistCrisis.BountyHunting
{
    /// <summary>
    /// Loads BountyDefinitions from the module's BountyDefinitions.xml, falling back
    /// to a small set of hardcoded defaults if the file is missing, unresolvable, or
    /// fails to parse.
    /// </summary>
    public static class BountyDefinitionRegistry
    {
        private static List<BountyDefinition> _definitions = new List<BountyDefinition>();

        public static IReadOnlyList<BountyDefinition> Definitions => _definitions;

        /// <summary>
        /// Returns the loaded definition with the given id, or null if none matches.
        /// </summary>
        public static BountyDefinition GetById(string id)
        {
            return _definitions.FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// Loads and parses BountyDefinitions.xml from the module's ModuleData folder,
        /// falling back to hardcoded defaults on any failure.
        /// </summary>
        public static void LoadDefinitions()
        {
            _definitions = new List<BountyDefinition>();

            string path;
            try
            {
                path = Path.Combine(ModuleHelper.GetModuleFullPath(SeparatistCrisis.SubModule.Name), "ModuleData", "BountyDefinitions.xml");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"BountyDefinitionRegistry.LoadDefinitions: ABORT — could not resolve module path via ModuleHelper: {ex}. Falling back to hardcoded defaults.");
                LoadHardcodedDefaults();
                return;
            }

            if (!File.Exists(path))
            {
                BountyLogger.Log($"BountyDefinitionRegistry.LoadDefinitions: file not found at '{path}'. Falling back to hardcoded defaults.");
                LoadHardcodedDefaults();
                return;
            }

            try
            {
                var doc = XDocument.Load(path);
                foreach (var el in doc.Root?.Elements("BountyDefinition") ?? Enumerable.Empty<XElement>())
                {
                    var def = ParseDefinition(el);
                    if (def != null)
                    {
                        _definitions.Add(def);
                    }
                }

                BountyLogger.Log($"BountyDefinitionRegistry.LoadDefinitions: loaded {_definitions.Count} definition(s) from '{path}'.");
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"BountyDefinitionRegistry.LoadDefinitions: EXCEPTION parsing '{path}': {ex}. Falling back to hardcoded defaults.");
                LoadHardcodedDefaults();
                return;
            }

            if (_definitions.Count == 0)
            {
                BountyLogger.Log("BountyDefinitionRegistry.LoadDefinitions: file parsed but produced zero definitions. Falling back to hardcoded defaults.");
                LoadHardcodedDefaults();
            }
        }

        /// <summary>
        /// Parses a single &lt;BountyDefinition&gt; element into a BountyDefinition,
        /// or null if it is missing a required field or fails to parse.
        /// </summary>
        private static BountyDefinition ParseDefinition(XElement el)
        {
            try
            {
                var def = new BountyDefinition
                {
                    Id = (string)el.Attribute("id"),
                    Name = (string)el.Element("Name") ?? (string)el.Attribute("id"),
                    Description = (string)el.Element("Description") ?? ""
                };

                if (string.IsNullOrEmpty(def.Id))
                {
                    BountyLogger.Log("BountyDefinitionRegistry.ParseDefinition: skipped a <BountyDefinition> with no 'id' attribute.");
                    return null;
                }

                var spawnTypeStr = (string)el.Attribute("spawnType") ?? (string)el.Element("SpawnType");
                if (!Enum.TryParse(spawnTypeStr, out BountySpawnType spawnType))
                {
                    BountyLogger.Log($"BountyDefinitionRegistry.ParseDefinition: '{def.Id}' has invalid/missing spawnType '{spawnTypeStr}' — skipped.");
                    return null;
                }
                def.SpawnType = spawnType;

                var valueRange = el.Element("ValueRange");
                if (valueRange != null)
                {
                    def.MinValue = (int?)valueRange.Attribute("min") ?? def.MinValue;
                    def.MaxValue = (int?)valueRange.Attribute("max") ?? def.MaxValue;
                }

                var expiryRange = el.Element("ExpiryDaysRange");
                if (expiryRange != null)
                {
                    def.MinExpiryDays = (int?)expiryRange.Attribute("min") ?? def.MinExpiryDays;
                    def.MaxExpiryDays = (int?)expiryRange.Attribute("max") ?? def.MaxExpiryDays;
                }

                var settlementsEl = el.Element("Settlements");
                if (settlementsEl != null)
                {
                    foreach (var sEl in settlementsEl.Elements("SettlementId"))
                    {
                        var id = (string)sEl;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            def.SettlementIds.Add(id.Trim());
                        }
                    }
                }

                var templateEl = el.Element("HeroTemplate");
                if (templateEl != null)
                {
                    def.Template = ParseHeroTemplate(templateEl);
                }

                def.ExistingLordId = (string)el.Element("ExistingLordId");
                if (string.IsNullOrWhiteSpace(def.ExistingLordId)) def.ExistingLordId = null;

                def.FactionId = (string)el.Element("FactionId");
                if (string.IsNullOrWhiteSpace(def.FactionId)) def.FactionId = null;

                def.BountyFactionId = (string)el.Element("BountyFactionId");
                if (string.IsNullOrWhiteSpace(def.BountyFactionId)) def.BountyFactionId = null;

                def.PatrolCenterSettlementId = (string)el.Element("PatrolCenterSettlementId");
                def.PatrolRadius = (float?)el.Element("PatrolRadius") ?? def.PatrolRadius;

                var troopEl = el.Element("Troop");
                if (troopEl != null)
                {
                    var troopId = (string)troopEl.Attribute("id");
                    if (!string.IsNullOrWhiteSpace(troopId)) def.TroopId = troopId.Trim();
                    def.TroopCount = (int?)troopEl.Attribute("count") ?? def.TroopCount;
                }

                def.ThugCount = (int?)el.Element("ThugCount") ?? def.ThugCount;

                return def;
            }
            catch (Exception ex)
            {
                BountyLogger.Log($"BountyDefinitionRegistry.ParseDefinition: EXCEPTION parsing one <BountyDefinition> element: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Maps a vanilla-style "ItemN" equipment slot name to the real EquipmentIndex
        /// enum name "WeaponN"; other slot names are returned unchanged.
        /// </summary>
        private static string NormalizeSlotName(string rawSlotName)
        {
            if (string.IsNullOrEmpty(rawSlotName)) return rawSlotName;

            if (rawSlotName.StartsWith("Item", StringComparison.OrdinalIgnoreCase)
                && rawSlotName.Length > 4
                && int.TryParse(rawSlotName.Substring(4), out int weaponIndex))
            {
                return $"Weapon{weaponIndex}";
            }

            return rawSlotName;
        }

        /// <summary>
        /// Strips vanilla's "Item." cross-reference prefix from an item id, if
        /// present.
        /// </summary>
        private static string NormalizeItemId(string rawItemId)
        {
            if (string.IsNullOrEmpty(rawItemId)) return rawItemId;

            const string prefix = "Item.";
            return rawItemId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? rawItemId.Substring(prefix.Length)
                : rawItemId;
        }

        /// <summary>
        /// Parses a &lt;HeroTemplate&gt; element into a HeroTemplate, including its
        /// equipment slots (supporting both the nested &lt;Slot&gt; shape and
        /// vanilla's sibling-element shape) and optional body properties.
        /// </summary>
        private static HeroTemplate ParseHeroTemplate(XElement templateEl)
        {
            var template = new HeroTemplate();

            var charTemplate = (string)templateEl.Element("CharacterTemplateId");
            if (!string.IsNullOrWhiteSpace(charTemplate)) template.CharacterTemplateId = charTemplate.Trim();

            template.NameText = (string)templateEl.Element("NameText") ?? template.NameText;

            var ageEl = templateEl.Element("AgeRange");
            if (ageEl != null)
            {
                template.MinAge = (int?)ageEl.Attribute("min") ?? template.MinAge;
                template.MaxAge = (int?)ageEl.Attribute("max") ?? template.MaxAge;
            }

            var nestedEquipEl = templateEl.Element("Equipment");
            if (nestedEquipEl != null && nestedEquipEl.Elements("Slot").Any())
            {
                foreach (var slotEl in nestedEquipEl.Elements("Slot"))
                {
                    var slotName = (string)slotEl.Attribute("name");
                    var itemId = (string)slotEl.Attribute("item");
                    if (!string.IsNullOrEmpty(slotName) && !string.IsNullOrEmpty(itemId))
                    {
                        template.EquipmentSlots[NormalizeSlotName(slotName)] = NormalizeItemId(itemId);
                    }
                }
            }

            foreach (var equipEl in templateEl.Elements("Equipment"))
            {
                var slotName = (string)equipEl.Attribute("slot");
                var itemId = (string)equipEl.Attribute("id");
                if (!string.IsNullOrEmpty(slotName) && !string.IsNullOrEmpty(itemId))
                {
                    template.EquipmentSlots[NormalizeSlotName(slotName)] = NormalizeItemId(itemId);
                }
            }

            var bodyPropsEl = templateEl.Element("BodyProperties");
            if (bodyPropsEl != null)
            {
                string weight = (string)bodyPropsEl.Attribute("weight");
                string build = (string)bodyPropsEl.Attribute("build");
                string key = (string)bodyPropsEl.Attribute("key");

                if (!string.IsNullOrEmpty(weight) && !string.IsNullOrEmpty(build) && !string.IsNullOrEmpty(key))
                {
                    template.HasBodyProperties = true;
                    template.BodyPropertiesWeight = float.TryParse(weight, out var w) ? w : 0.5f;
                    template.BodyPropertiesBuild = float.TryParse(build, out var b) ? b : 0.5f;
                    template.BodyPropertiesKeyHex = key.Trim();
                }
                else
                {
                    BountyLogger.Log("BountyDefinitionRegistry.ParseHeroTemplate: <BodyProperties> element is missing one of weight/build/key — ignored.");
                }
            }

            return template;
        }

        /// <summary>
        /// Populates a small set of hardcoded fallback definitions, used when the XML
        /// file is missing, unresolvable, or fails to parse.
        /// </summary>
        private static void LoadHardcodedDefaults()
        {
            _definitions = new List<BountyDefinition>
            {
                new BountyDefinition
                {
                    Id = "wandering_lord_bounty",
                    Name = "Wanted Criminal",
                    SpawnType = BountySpawnType.Wandering,
                    MinValue = 500, MaxValue = 5000,
                    MinExpiryDays = 10, MaxExpiryDays = 30
                },
                new BountyDefinition
                {
                    Id = "settlement_gang_bounty",
                    Name = "Gang Leader",
                    SpawnType = BountySpawnType.SettlementGang,
                    MinValue = 500, MaxValue = 5000,
                    MinExpiryDays = 10, MaxExpiryDays = 30,
                    Template = new HeroTemplate
                    {
                        CharacterTemplateId = "looter",
                        NameText = "Gang Leader",
                        MinAge = 25, MaxAge = 45
                    }
                },
                new BountyDefinition
                {
                    Id = "stealth_fugitive_bounty",
                    Name = "Hidden Fugitive",
                    SpawnType = BountySpawnType.SettlementStealth,
                    MinValue = 500, MaxValue = 5000,
                    MinExpiryDays = 10, MaxExpiryDays = 30,
                    Template = new HeroTemplate
                    {
                        CharacterTemplateId = "looter",
                        NameText = "Hidden Fugitive",
                        MinAge = 25, MaxAge = 45,
                        EquipmentSlots = new Dictionary<string, string> { { "Weapon0", "sword_1_t2" } }
                    }
                }
            };

            BountyLogger.Log($"BountyDefinitionRegistry.LoadHardcodedDefaults: loaded {_definitions.Count} built-in default definition(s).");
        }
    }
}
