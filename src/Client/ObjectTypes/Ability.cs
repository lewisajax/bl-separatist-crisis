using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.ObjectTypes
{
    public class Ability: MBObjectBase
    {
        public Ability() : this(string.Empty) { }

        public Ability(string Name) { }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            bool isInitialized = base.IsInitialized;
            base.Deserialize(objectManager, node);

            if (node == null)
                return;
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
    }
}
