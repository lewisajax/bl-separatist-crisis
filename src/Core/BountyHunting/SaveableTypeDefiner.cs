using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace SeparatistCrisis.BountyHunting
{
    /// <summary>
    /// Registers this mod's custom classes, enums, and container types with the
    /// save system. Local ids share one keyspace across DefineClassTypes,
    /// DefineEnumTypes, and DefineContainerDefinitions and must all be globally
    /// unique within this definer.
    /// </summary>
    public class BountyHuntingSaveDefiner : SaveableTypeDefiner
    {
        public BountyHuntingSaveDefiner() : base(559001)
        {
        }

        /// <summary>
        /// Registers custom classes that need to be saved.
        /// </summary>
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(BountyTarget), 1);
            AddClassDefinition(typeof(BountyQuest), 3);
        }

        /// <summary>
        /// Registers custom enums that need to be saved.
        /// </summary>
        protected override void DefineEnumTypes()
        {
            AddEnumDefinition(typeof(BountyStatus), 2);
        }

        /// <summary>
        /// Registers container types (generic collections) used by saved fields.
        /// </summary>
        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<BountyTarget>));
            ConstructContainerDefinition(typeof(List<string>));
        }
    }
}
