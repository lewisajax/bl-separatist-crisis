using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Map;

namespace SeparatistCrisis.Widgets
{
    public class SCNameplateItemWidget: Widget
    {
        private bool _hoverBegan;
        private Widget _inspectedIconWidget;
        private Widget _portIconWidget;
        private MapEventVisualBrushWidget _mapEventVisualWidget;
        private MaskedTextureWidget _settlementBannerWidget;
        private TextWidget _settlementNameTextWidget;
        private GridWidget _settlementPartiesGridWidget;
        private Widget _widgetToShow;
        private Widget _parleyIconWidget;

        public Widget InspectedIconWidget
        {
            get => this._inspectedIconWidget;
            set
            {
                if (this._inspectedIconWidget == value)
                    return;
                this._inspectedIconWidget = value;
                this.OnPropertyChanged<Widget>(value, nameof(InspectedIconWidget));
            }
        }

        public Widget PortIconWidget
        {
            get => this._portIconWidget;
            set
            {
                if (this._portIconWidget == value)
                    return;
                this._portIconWidget = value;
                this.OnPropertyChanged<Widget>(value, nameof(PortIconWidget));
            }
        }

        public GridWidget SettlementPartiesGridWidget
        {
            get => this._settlementPartiesGridWidget;
            set
            {
                if (this._settlementPartiesGridWidget == value)
                    return;
                this._settlementPartiesGridWidget = value;
                this.OnPropertyChanged<GridWidget>(value, nameof(SettlementPartiesGridWidget));
            }
        }

        public MapEventVisualBrushWidget MapEventVisualWidget
        {
            get => this._mapEventVisualWidget;
            set
            {
                if (this._mapEventVisualWidget == value)
                    return;
                this._mapEventVisualWidget = value;
                this.OnPropertyChanged<MapEventVisualBrushWidget>(value, nameof(MapEventVisualWidget));
            }
        }

        [Editor(false)]
        public Widget WidgetToShow
        {
            get => this._widgetToShow;
            set
            {
                if (this._widgetToShow == value)
                    return;
                this._widgetToShow = value;
                this.OnPropertyChanged<Widget>(value, nameof(WidgetToShow));
            }
        }

        public MaskedTextureWidget SettlementBannerWidget
        {
            get => this._settlementBannerWidget;
            set
            {
                if (this._settlementBannerWidget == value)
                    return;
                this._settlementBannerWidget = value;
                this.OnPropertyChanged<MaskedTextureWidget>(value, nameof(SettlementBannerWidget));
            }
        }

        public TextWidget SettlementNameTextWidget
        {
            get => this._settlementNameTextWidget;
            set
            {
                if (this._settlementNameTextWidget == value)
                    return;
                this._settlementNameTextWidget = value;
                this.OnPropertyChanged<TextWidget>(value, nameof(SettlementNameTextWidget));
            }
        }

        public Widget ParleyIconWidget
        {
            get => this._parleyIconWidget;
            set
            {
                if (this._parleyIconWidget == value)
                    return;
                this._parleyIconWidget = value;
                this.OnPropertyChanged<Widget>(value, nameof(ParleyIconWidget));
            }
        }

        public bool IsOverWidget { get; private set; }

        public int QuestType { get; set; }

        public int IssueType { get; set; }

        public SCNameplateItemWidget(UIContext context) : base(context)
        {
        }

        public new void ParallelUpdate(float dt)
        {
            Widget widgetToShow = this._widgetToShow;
            Widget parentWidget = this.ParentWidget;
            if (widgetToShow == null)
                Debug.FailedAssert("widgetToShow is null during ParallelUpdate!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateItemWidget.cs", nameof(ParallelUpdate), 24);
            else if (parentWidget != null && parentWidget.IsEnabled)
            {
                this.IsOverWidget = this.IsMouseOverWidget();
                if (this.IsOverWidget && !this._hoverBegan)
                {
                    this._hoverBegan = true;
                    widgetToShow.IsVisible = true;
                }
                else if (!this.IsOverWidget && this._hoverBegan)
                {
                    this._hoverBegan = false;
                    widgetToShow.IsVisible = false;
                }
                if (this.IsOverWidget || !widgetToShow.IsVisible)
                    return;
                widgetToShow.IsVisible = false;
            }
            else
                widgetToShow.IsVisible = false;
        }

        private bool IsMouseOverWidget()
        {
            return this.EventManager.IsPointInsideUsableArea(this.EventManager.MousePosition) && this.AreaRect.IsPointInside(this.EventManager.MousePosition);
        }
    }
}
