using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace SeparatistCrisis.Widgets
{
    public class SCNameplateManagerWidget: Widget
    {
        public SCNameplateManagerWidget(UIContext context) : base(context)
        {
        }

        protected override void OnRender(TwoDimensionContext twoDimensionContext, TwoDimensionDrawContext drawContext)
        {
            this._visibleNameplates.Clear();
            foreach (SCNameplateWidget settlementNameplateWidget in this._allChildrenNameplates)
            {
                if (settlementNameplateWidget != null && settlementNameplateWidget.IsVisibleOnMap)
                {
                    this._visibleNameplates.Add(settlementNameplateWidget);
                }
            }
            // this._visibleNameplates.Sort();
            foreach (SCNameplateWidget settlementNameplateWidget2 in this._visibleNameplates)
            {
                settlementNameplateWidget2.DisableRender = false;
                settlementNameplateWidget2.Render(twoDimensionContext, drawContext);
                settlementNameplateWidget2.DisableRender = true;
            }
        }

        protected override void OnChildAdded(Widget child)
        {
            base.OnChildAdded(child);
            child.DisableRender = true;
            this._allChildrenNameplates.Add(child as SCNameplateWidget);
        }

        protected override void OnChildRemoved(Widget child)
        {
            base.OnChildRemoved(child);
            this._allChildrenNameplates.Remove(child as SCNameplateWidget);
        }

        protected override void OnDisconnectedFromRoot()
        {
            base.OnDisconnectedFromRoot();
            this._allChildrenNameplates.Clear();
            this._allChildrenNameplates = null;
        }

        private readonly List<SCNameplateWidget> _visibleNameplates = new List<SCNameplateWidget>();

        private List<SCNameplateWidget> _allChildrenNameplates = new List<SCNameplateWidget>();
    }
}
