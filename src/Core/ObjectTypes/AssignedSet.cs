using SeparatistCrisis.SetOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.ObjectTypes
{
    public class AssignedSet : MBObjectBase
    {
        private BasicCharacterObject _character = null!;

        public BasicCharacterObject Character
        {
            get
            {
                return this._character;
            }
        }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            SetAssignments assignments = SetAssignments.Instance;

            // We will most likely set these up to be their own MBObjects later on
            if (!assignments.HasDeserializedAssignments)
            {
                XmlNode? lordsNode = node?.ParentNode?.SelectSingleNode(".//AssignedHeroSets");
                XmlNode? clansNode = node?.ParentNode?.SelectSingleNode(".//AssignedClanSets");

                if (lordsNode != null)
                    assignments.Deserialize(objectManager, lordsNode);

                if (clansNode != null) // && Game.Current.GameType.GetType() == typeof(Campaign)
                    assignments.Deserialize(objectManager, clansNode);

                assignments.HasDeserializedAssignments = true;
            }

            base.Deserialize(objectManager, node);
            XmlAttributeCollection? attributes = node?.Attributes;
            if (attributes != null)
            {
                XmlAttribute? characterAttribute = attributes["character_id"];

                if (characterAttribute == null)
                {
                    Debug.FailedAssert("The character attribute is null", "C:\\SeparatistCrisis\\ObjectTypes\\AssignedSet.cs", "Deserialize");
                    MBObjectManager.Instance.UnregisterObject(this);
                    return;
                }

                BasicCharacterObject character = MBObjectManager.Instance.GetObject<BasicCharacterObject>(characterAttribute.Value);
                this._character = character;
            }
        }

        public static MBReadOnlyList<AssignedSet> All
        {
            get
            {
                return MBObjectManager.Instance.GetObjectTypeList<AssignedSet>();
            }
        }

        public static AssignedSet? Find(string idString)
        {
            return MBObjectManager.Instance.GetObject<AssignedSet>(idString);
        }

        public static AssignedSet? Find(BasicCharacterObject charObj)
        {
            return MBObjectManager.Instance.GetObject<AssignedSet>((x) => x.Character == charObj);
        }
    }
}
