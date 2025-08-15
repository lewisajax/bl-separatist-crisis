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
        public SCNameplateItemWidget(UIContext context) : base(context)
        {
        }

        public bool IsOverWidget { get; private set; }

        public int QuestType { get; set; }

        public int IssueType { get; set; }

        public void ParallelUpdate(float dt)
        {
            Widget widgetToShow = this._widgetToShow;
            Widget parentWidget = base.ParentWidget;
            if (widgetToShow == null)
            {
                Debug.FailedAssert("widgetToShow is null during ParallelUpdate!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\GrimNameplateItemWidget.cs", "ParallelUpdate", 24);
                return;
            }
            if (parentWidget != null && parentWidget.IsEnabled)
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
                if (!this.IsOverWidget && widgetToShow.IsVisible)
                {
                    widgetToShow.IsVisible = false;
                    return;
                }
            }
            else
            {
                widgetToShow.IsVisible = false;
            }
        }

        private bool IsMouseOverWidget()
        {
            Vector2 globalPosition = base.GlobalPosition;
            return this.IsBetween(base.EventManager.MousePosition.X, globalPosition.X, globalPosition.X + base.Size.X) && this.IsBetween(base.EventManager.MousePosition.Y, globalPosition.Y, globalPosition.Y + base.Size.Y);
        }

        private bool IsBetween(float number, float min, float max)
        {
            return number >= min && number <= max;
        }

        public Widget SettlementNameplateCapsuleWidget
        {
            get
            {
                return this._settlementNameplateCapsuleWidget;
            }
            set
            {
                if (this._settlementNameplateCapsuleWidget != value)
                {
                    this._settlementNameplateCapsuleWidget = value;
                    base.OnPropertyChanged<Widget>(value, "SettlementNameplateCapsuleWidget");
                }
            }
        }

        public GridWidget SettlementPartiesGridWidget
        {
            get
            {
                return this._settlementPartiesGridWidget;
            }
            set
            {
                if (this._settlementPartiesGridWidget != value)
                {
                    this._settlementPartiesGridWidget = value;
                    base.OnPropertyChanged<GridWidget>(value, "SettlementPartiesGridWidget");
                }
            }
        }

        public MapEventVisualBrushWidget MapEventVisualWidget
        {
            get
            {
                return this._mapEventVisualWidget;
            }
            set
            {
                if (this._mapEventVisualWidget != value)
                {
                    this._mapEventVisualWidget = value;
                    base.OnPropertyChanged<MapEventVisualBrushWidget>(value, "MapEventVisualWidget");
                }
            }
        }

        [Editor(false)]
        public Widget WidgetToShow
        {
            get
            {
                return this._widgetToShow;
            }
            set
            {
                if (this._widgetToShow != value)
                {
                    this._widgetToShow = value;
                    base.OnPropertyChanged<Widget>(value, "WidgetToShow");
                }
            }
        }

        public Widget SettlementNameplateInspectedWidget
        {
            get
            {
                return this._settlementNameplateInspectedWidget;
            }
            set
            {
                if (this._settlementNameplateInspectedWidget != value)
                {
                    this._settlementNameplateInspectedWidget = value;
                    base.OnPropertyChanged<Widget>(value, "SettlementNameplateInspectedWidget");
                }
            }
        }

        public MaskedTextureWidget SettlementBannerWidget
        {
            get
            {
                return this._settlementBannerWidget;
            }
            set
            {
                if (this._settlementBannerWidget != value)
                {
                    this._settlementBannerWidget = value;
                    base.OnPropertyChanged<MaskedTextureWidget>(value, "SettlementBannerWidget");
                }
            }
        }

        public TextWidget SettlementNameTextWidget
        {
            get
            {
                return this._settlementNameTextWidget;
            }
            set
            {
                if (this._settlementNameTextWidget != value)
                {
                    this._settlementNameTextWidget = value;
                    base.OnPropertyChanged<TextWidget>(value, "SettlementNameTextWidget");
                }
            }
        }

        private bool _hoverBegan;

        private Widget _settlementNameplateCapsuleWidget;

        private Widget _settlementNameplateInspectedWidget;

        private MapEventVisualBrushWidget _mapEventVisualWidget;

        private MaskedTextureWidget _settlementBannerWidget;

        private TextWidget _settlementNameTextWidget;

        private GridWidget _settlementPartiesGridWidget;

        private Widget _widgetToShow;
    }
}
