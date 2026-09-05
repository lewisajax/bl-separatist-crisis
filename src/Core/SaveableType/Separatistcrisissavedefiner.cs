using SeparatistCrisis.Components;
using TaleWorlds.SaveSystem;

namespace SeparatistCrisis
{
    public class SeparatistCrisisSaveDefiner : SaveableTypeDefiner
    {
        public SeparatistCrisisSaveDefiner() : base(700001)
        {
        }

        /// <summary>
        /// Registers custom classes that need to be saved.
        /// </summary>
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(SettlementGroupComponent), 1);
        }
    }
}