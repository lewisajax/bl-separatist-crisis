using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeparatistCrisis.Abilities
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AbilityAttribute : Attribute
    {
        public string StringId { get; private set; }

        public AbilityAttribute(string stringId)
        {
            StringId = stringId;
        }
    } 
}
