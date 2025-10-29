using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.ObjectTypes
{
    public class SpaceShip: MBObjectBase
    {
        public string _name;
        public string _mesh;
        public float _scaleMultiplier = 1f;

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string Mesh
        {
            get
            {
                return this._mesh;
            }
        }

        public float ScaleMultiplier
        {
            get
            {
                return this._scaleMultiplier;
            }
        }

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            base.Deserialize(objectManager, node);

            XmlAttributeCollection? attributes = node?.Attributes;
            if (attributes != null)
            {
                XmlAttribute? nameAttribute = attributes["name"];
                XmlAttribute? meshAttribute = attributes["mesh"];
                XmlAttribute? scaleAttribute = attributes["scale_multiplier"];

                if (nameAttribute == null || meshAttribute == null)
                {
                    Debug.FailedAssert("Name or mesh on SpaceShip node is null", "C:\\SeparatistCrisis\\ObjectTypes\\SpaceShip.cs", "Deserialize", 441);
                    MBObjectManager.Instance.UnregisterObject(this);
                    return;
                }

                this._name = nameAttribute.Value;
                this._mesh = meshAttribute.Value;

                if (scaleAttribute != null)
                    this._scaleMultiplier = float.Parse(scaleAttribute.Value);
            }
        }
    }
}
