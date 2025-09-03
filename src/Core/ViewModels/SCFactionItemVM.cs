using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace SeparatistCrisis.ViewModels
{
    public class SCFactionItemVM : SelectorItemVM
    {
        public BasicCultureObject Faction { get; private set; }

        public SCFactionItemVM(BasicCultureObject faction) : base(faction.Name.ToString())
        {
            this.Faction = faction;
        }
    }
}
