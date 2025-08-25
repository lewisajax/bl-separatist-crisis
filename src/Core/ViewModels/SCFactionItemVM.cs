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
        // Token: 0x1700007E RID: 126
        // (get) Token: 0x060001A1 RID: 417 RVA: 0x0000A36B File Offset: 0x0000856B
        // (set) Token: 0x060001A2 RID: 418 RVA: 0x0000A373 File Offset: 0x00008573
        public BasicCultureObject Faction { get; private set; }

        // Token: 0x060001A3 RID: 419 RVA: 0x0000A37C File Offset: 0x0000857C
        public SCFactionItemVM(BasicCultureObject faction) : base(faction.Name.ToString())
        {
            this.Faction = faction;
        }
    }
}
