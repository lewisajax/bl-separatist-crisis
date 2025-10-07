using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;

namespace SeparatistCrisis.CustomBattle
{
    public class TroopSelectionListItemComparer: IComparer<SCTroopSelectionListItemVM>
    {
        public EncyclopediaSortController SortController { get; }

        public TroopSelectionListItemComparer(EncyclopediaSortController sortController)
        {
            this.SortController = sortController;
        }

        private int GetBookmarkComparison(SCTroopSelectionListItemVM x, SCTroopSelectionListItemVM y)
        {
            return -x.IsBookmarked.CompareTo(y.IsBookmarked);
        }

        public int Compare(SCTroopSelectionListItemVM? x, SCTroopSelectionListItemVM? y)
        {
            int bookmarkComparison = this.GetBookmarkComparison(x, y);
            if (bookmarkComparison != 0)
            {
                return bookmarkComparison;
            }
            return this.SortController.Comparer.Compare(x.ListItem, y.ListItem);
        }
    }
}
