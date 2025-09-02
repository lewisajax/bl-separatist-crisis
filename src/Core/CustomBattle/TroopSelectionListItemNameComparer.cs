using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Encyclopedia;

namespace SeparatistCrisis.CustomBattle
{
    public class TroopSelectionListItemNameComparer : EncyclopediaListItemComparerBase
    {
        public override int Compare(EncyclopediaListItem x, EncyclopediaListItem y)
        {
            return base.ResolveEquality(x, y);
        }

        public override string GetComparedValueText(EncyclopediaListItem item)
        {
            return "";
        }
    }
}
