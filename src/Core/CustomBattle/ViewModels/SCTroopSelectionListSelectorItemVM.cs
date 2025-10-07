using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace SeparatistCrisis.CustomBattle
{
    public class SCTroopSelectionListSelectorItemVM: SelectorItemVM
    {
        public SCTroopSelectionListSelectorItemVM(TroopSelectionListItemComparer comparer) : base(comparer?.SortController.Name.ToString())
        {
            Comparer = comparer;
        }

        public TroopSelectionListItemComparer? Comparer { get; set; }
    }
}
