using System;
using System.Collections.Generic;

namespace SeparatistCrisis.BountyHunting
{
    public enum BountySpawnType
    {
        Wandering,
        SettlementGang,
        PatrolParty,
        HideoutBoss
    }

    /// <summary>
    /// Fully describes how to build the hero for a SettlementGang/PatrolParty/
    /// HideoutBoss bounty: base character template, name, age range, optional custom
    /// face/body, and equipment slots — all data-driven from XML.
    /// </summary>
    public class HeroTemplate
    {
        public string CharacterTemplateId = "jabba";

        public string NameText;
        public int MinAge = 25;
        public int MaxAge = 45;

        public bool HasBodyProperties;
        public float BodyPropertiesWeight;
        public float BodyPropertiesBuild;
        public string BodyPropertiesKeyHex;

        public Dictionary<string, string> EquipmentSlots = new Dictionary<string, string>();
    }

    /// <summary>
    /// Data-driven definition of one bounty type, loaded from XML: spawn mechanism,
    /// value/expiry ranges, faction scoping, and type-specific parameters (hero
    /// template, settlement list, patrol center, troop counts). FactionId is used
    /// for settlement/candidate search (which faction's territory or party this
    /// bounty is found in). TargetFactionId is an optional, separate override for
    /// which faction/clan the spawned target hero itself belongs to — only
    /// meaningful for SettlementGang, and falls back to FactionId if left unset
    /// (e.g. a Separatist gang boss hiding in Republic territory: FactionId=galactic_republic for the
    /// settlement search, TargetFactionId=separatist for the hero's own clan).
    /// BountyFactionId is a further optional override for which faction's board
    /// the bounty shows on, only meaningful for Wandering and PatrolParty, and
    /// falls back to FactionId if left unset.
    /// </summary>
    public class BountyDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public BountySpawnType SpawnType;

        public int MinValue = 500;
        public int MaxValue = 5000;
        public int MinExpiryDays = 10;
        public int MaxExpiryDays = 30;

        public List<string> SettlementIds = new List<string>();

        public HeroTemplate Template;

        public string FactionId;

        public string TargetFactionId;

        public string BountyFactionId;

        public string ExistingLordId;

        public string PatrolCenterSettlementId;
        public float PatrolRadius = 15f;
        public string TroopId = "looter";
        public int TroopCount = 10;

        /// <summary>
        /// Returns a debug-friendly string summarizing this definition's key fields.
        /// </summary>
        public override string ToString()
        {
            return $"BountyDefinition(Id={Id}, Name={Name}, SpawnType={SpawnType}, Settlements=[{string.Join(",", SettlementIds)}], Value=[{MinValue}-{MaxValue}], ExpiryDays=[{MinExpiryDays}-{MaxExpiryDays}])";
        }
    }
}