using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace SeparatistCrisis.CustomBattle
{
    public class SCTroopSelectionListSelectorVM: SelectorVM<SCTroopSelectionListSelectorItemVM>
    {
        public SCTroopSelectionListSelectorVM(int selectedIndex, Action<SelectorVM<SCTroopSelectionListSelectorItemVM>> onChange, Action onActivate) : base(selectedIndex, onChange)
        {
            _onActivate = onActivate;
        }

        public void ExecuteOnDropdownActivated()
        {
            Action onActivate = _onActivate;
            if (onActivate == null)
            {
                return;
            }
            onActivate();
        }

        private Action _onActivate;
    }
}
