using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;
using TaleWorlds.TwoDimension;

namespace SeparatistCrisis.Widgets
{
    public class SCNameplateManagerWidget: Widget
    {
        private readonly List<SCNameplateWidget> _visibleNameplates = new List<SCNameplateWidget>();
        private List<SCNameplateWidget> _allChildrenNameplates = new List<SCNameplateWidget>();

        public SCNameplateManagerWidget(UIContext context) : base(context)
        {
        }

        protected override void OnRender(
          TwoDimensionContext twoDimensionContext,
          TwoDimensionDrawContext drawContext)
        {
            this._visibleNameplates.Clear();
            foreach (SCNameplateWidget childrenNameplate in this._allChildrenNameplates)
            {
                if (childrenNameplate != null && childrenNameplate.IsVisibleOnMap)
                    this._visibleNameplates.Add(childrenNameplate);
            }
            // this._visibleNameplates.Sort();
            foreach (SCNameplateWidget visibleNameplate in this._visibleNameplates)
            {
                visibleNameplate.DisableRender = false;
                visibleNameplate.Render(twoDimensionContext, drawContext);
                visibleNameplate.DisableRender = true;
            }
        }

        protected override void OnChildAdded(Widget child)
        {
            base.OnChildAdded(child);
            child.DisableRender = true;
            this._allChildrenNameplates.Add(child as SCNameplateWidget);
        }

        protected override void OnBeforeChildRemoved(Widget child)
        {
            base.OnBeforeChildRemoved(child);
            this._allChildrenNameplates.Remove(child as SCNameplateWidget);
        }

        protected override void OnDisconnectedFromRoot()
        {
            base.OnDisconnectedFromRoot();
            this._allChildrenNameplates.Clear();
            this._allChildrenNameplates = (List<SCNameplateWidget>)null;
        }
    }
}
