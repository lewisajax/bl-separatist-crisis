using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Menu;
using SeparatistCrisis.Abilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.ObjectTypes
{
    public class Ability: MBObjectBase
    {
        private static Dictionary<string, Type>? _abilityTypes;

        public string Name { get; private set; } = null!;

        public Type? Action { get; private set; }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            bool isInitialized = base.IsInitialized;
            base.Deserialize(objectManager, node);

            if (Ability._abilityTypes == null)
            {
                Ability._abilityTypes = Ability.CollectAbilityTypes();
            }

            if (node == null)
                return;

            if (node.Attributes == null || node.Attributes["name"] == null)
                throw new ArgumentNullException("node");

            this.Name = node.Attributes["name"].Value;

            if (Ability._abilityTypes.TryGetValue(this.StringId, out Type? value))
            {
                if (value != null)
                {
                    this.Action = value;
                }
            }
        }

        public static MBReadOnlyList<Ability> All
        {
            get
            {
                return MBObjectManager.Instance.GetObjectTypeList<Ability>();
            }
        }

        public static Ability Find(string idString)
        {
            return MBObjectManager.Instance.GetObject<Ability>(idString);
        }

        public static Ability FindFirst(Func<Ability, bool> predicate)
        {
            return Ability.All.FirstOrDefault(predicate);
        }

        public static IEnumerable<Ability> FindAll(Func<Ability, bool> predicate)
        {
            return Ability.All.Where(predicate);
        }

        private static Dictionary<string, Type> CollectAbilityTypes()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            Assembly currAssembly = typeof(AbilityAttribute).Assembly;
            Assembly[] refAssemblies = currAssembly.GetReferencingAssembliesSafe(null); // TW extension for Assembly
            Assembly[] assemblies = refAssemblies.Append(currAssembly).ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type type in assemblies[i].GetTypesSafe(null))
                {
                    if (!typeof(BaseAbility).IsAssignableFrom(type))
                        continue;

                    object[] customAttributesSafe = type.GetCustomAttributesSafe(typeof(AbilityAttribute), false);

                    if (customAttributesSafe != null && customAttributesSafe.Length == 1 && customAttributesSafe[0] is AbilityAttribute)
                    {
                        AbilityAttribute? abiAttr = customAttributesSafe[0] as AbilityAttribute;

                        if (abiAttr != null)
                        {
                            if (type != null)
                                types[abiAttr.StringId] = type;
                        }
                    }
                }
            }

            return types;
        }
    }
}
