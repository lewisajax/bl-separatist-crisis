using JetBrains.Annotations;
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
using TaleWorlds.TwoDimension;
using MathF = TaleWorlds.Library.MathF;

namespace SeparatistCrisis.Widgets
{
    public class SCNameplateWidget: Widget
    {
        public SCNameplateWidget(UIContext context) : base(context)
        {
        }

        private float _screenEdgeAlphaTarget
        {
            get
            {
                return 1f;
            }
        }

        private float _normalNeutralAlphaTarget
        {
            get
            {
                return 0.35f;
            }
        }

        private float _normalAllyAlphaTarget
        {
            get
            {
                return 0.5f;
            }
        }

        private float _normalEnemyAlphaTarget
        {
            get
            {
                return 0.35f;
            }
        }

        private float _trackedAlphaTarget
        {
            get
            {
                return 0.8f;
            }
        }

        private float _trackedColorFactorTarget
        {
            get
            {
                return 1.3f;
            }
        }

        private float _normalColorFactorTarget
        {
            get
            {
                return 1f;
            }
        }

        protected override void OnParallelUpdate(float dt)
        {
            base.OnParallelUpdate(dt);
            SCNameplateItemWidget currentNameplate = this._currentNameplate;
            if (currentNameplate != null)
            {
                currentNameplate.ParallelUpdate(dt);
            }
            if (currentNameplate != null && this._cachedItemSize != currentNameplate.Size)
            {
                this._cachedItemSize = currentNameplate.Size;
                ListPanel eventsListPanel = this._eventsListPanel;
                ListPanel notificationListPanel = this._notificationListPanel;
                if (eventsListPanel != null)
                {
                    eventsListPanel.ScaledPositionXOffset = this._cachedItemSize.X;
                }
                if (notificationListPanel != null)
                {
                    notificationListPanel.ScaledPositionYOffset = -this._cachedItemSize.Y;
                }
                base.SuggestedWidth = this._cachedItemSize.X * base._inverseScaleToUse;
                base.SuggestedHeight = this._cachedItemSize.Y * base._inverseScaleToUse;
                base.ScaledSuggestedWidth = this._cachedItemSize.X;
                base.ScaledSuggestedHeight = this._cachedItemSize.Y;
            }
            base.IsEnabled = this.IsVisibleOnMap;
            this.UpdateNameplateTransparencyAndBrightness(dt);
            this.UpdatePosition(dt);
            this.UpdateTutorialState();
        }

        private void UpdatePosition(float dt)
        {
            SCNameplateItemWidget currentNameplate = this._currentNameplate;

            MapEventVisualBrushWidget mapEventVisualBrushWidget = (currentNameplate != null) ? currentNameplate.MapEventVisualWidget : null;
            if (currentNameplate == null || mapEventVisualBrushWidget == null)
            {
                Debug.FailedAssert("Related widget null on UpdatePosition!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateWidget.cs", "UpdatePosition", 105);
                return;
            }
            bool flag = false;
            this._positionTimer += dt;
            if (this.IsVisibleOnMap || this._positionTimer < 2f)
            {
                float x = base.Context.EventManager.PageSize.X;
                float y = base.Context.EventManager.PageSize.Y;
                Vec2 vec = this.Position;
                if (this.IsTracked)
                {
                    if (this.WSign > 0 && vec.x - base.Size.X / 2f > 0f && vec.x + base.Size.X / 2f < x && vec.y > 0f && vec.y + base.Size.Y < y)
                    {
                        base.ScaledPositionXOffset = vec.x - base.Size.X / 2f;
                        base.ScaledPositionYOffset = vec.y - base.Size.Y;
                    }
                    else
                    {
                        Vec2 vec2 = new Vec2(x / 2f, y / 2f);
                        vec -= vec2;
                        if (this.WSign < 0)
                        {
                            vec *= -1f;
                        }
                        float radian = Mathf.Atan2(vec.y, vec.x) - 1.5707964f;
                        float num = Mathf.Cos(radian);
                        float num2 = Mathf.Sin(radian);
                        float num3 = num / num2;
                        Vec2 vec3 = vec2 * 1f;
                        vec = ((num > 0f) ? new Vec2(-vec3.y / num3, vec2.y) : new Vec2(vec3.y / num3, -vec2.y));
                        if (vec.x > vec3.x)
                        {
                            vec = new Vec2(vec3.x, -vec3.x * num3);
                        }
                        else if (vec.x < -vec3.x)
                        {
                            vec = new Vec2(-vec3.x, vec3.x * num3);
                        }
                        vec += vec2;
                        flag = (vec.y - base.Size.Y - mapEventVisualBrushWidget.Size.Y <= 0f);
                        base.ScaledPositionXOffset = Mathf.Clamp(vec.x - base.Size.X / 2f, 0f, x - currentNameplate.Size.X);
                        base.ScaledPositionYOffset = Mathf.Clamp(vec.y - base.Size.Y, 0f, y - (currentNameplate.Size.Y + 55f));
                    }
                }
                else
                {
                    base.ScaledPositionXOffset = vec.x - base.Size.X / 2f;
                    base.ScaledPositionYOffset = vec.y - base.Size.Y;
                }
            }
            if (flag)
            {
                mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Bottom;
                mapEventVisualBrushWidget.ScaledPositionYOffset = mapEventVisualBrushWidget.Size.Y;
                return;
            }
            mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Top;
            mapEventVisualBrushWidget.ScaledPositionYOffset = -mapEventVisualBrushWidget.Size.Y;
        }

        private void OnNotificationListUpdated(Widget widget)
        {
            this._updatePositionNextFrame = true;
            this.AddLateUpdateAction();
        }

        private void OnNotificationListUpdated(Widget parentWidget, Widget addedWidget)
        {
            this._updatePositionNextFrame = true;
            this.AddLateUpdateAction();
        }

        private void AddLateUpdateAction()
        {
            if (!this._lateUpdateActionAdded)
            {
                base.EventManager.AddLateUpdateAction(this, new Action<float>(this.CustomLateUpdate), 1);
                this._lateUpdateActionAdded = true;
            }
        }

        private void CustomLateUpdate(float dt)
        {
            if (this._updatePositionNextFrame)
            {
                this.UpdatePosition(dt);
                this._updatePositionNextFrame = false;
            }
            this._lateUpdateActionAdded = false;
        }

        private void UpdateTutorialState()
        {
            if (this._tutorialAnimState == SCNameplateWidget.TutorialAnimState.Start)
            {
                this._tutorialAnimState = SCNameplateWidget.TutorialAnimState.FirstFrame;
            }
            else
            {
                SCNameplateWidget.TutorialAnimState tutorialAnimState = this._tutorialAnimState;
            }
            if (this.IsTargetedByTutorial)
            {
                this.SetState("Default");
                return;
            }
            this.SetState("Disabled");
        }

        private void SetNameplateTypeVisual(int type)
        {
            if (this._currentNameplate == null)
            {
                this.SmallNameplateWidget.IsVisible = false;
                this.NormalNameplateWidget.IsVisible = false;
                this.BigNameplateWidget.IsVisible = false;

                switch (type)
                {
                    case 0:
                        this._currentNameplate = this.SmallNameplateWidget;
                        this.SmallNameplateWidget.IsVisible = true;
                        return;
                    case 1:
                        this._currentNameplate = this.NormalNameplateWidget;
                        this.NormalNameplateWidget.IsVisible = true;
                        return;
                    case 2:
                        this._currentNameplate = this.BigNameplateWidget;
                        this.BigNameplateWidget.IsVisible = true;
                        break;
                    case 3:
                        this._currentNameplate = this.NormalNameplateWidget;
                        this.NormalNameplateWidget.IsVisible = true;
                        break;
                    default:
                        return;
                }
            }
        }

        private void SetNameplateRelationType(int type)
        {
            if (this._currentNameplate != null)
            {
                switch (type)
                {
                    case 0:
                        this._currentNameplate.Color = Color.Black;
                        return;
                    case 1:
                        this._currentNameplate.Color = Color.ConvertStringToColor("#245E05FF");
                        return;
                    case 2:
                        this._currentNameplate.Color = Color.ConvertStringToColor("#870707FF");
                        break;
                    default:
                        return;
                }
            }
        }

        private void UpdateNameplateTransparencyAndBrightness(float dt)
        {
            SCNameplateItemWidget currentNameplate = this._currentNameplate;
            TextWidget textWidget = (currentNameplate != null) ? currentNameplate.SettlementNameTextWidget : null;
            MaskedTextureWidget maskedTextureWidget = (currentNameplate != null) ? currentNameplate.SettlementBannerWidget : null;
            GridWidget gridWidget = (currentNameplate != null) ? currentNameplate.SettlementPartiesGridWidget : null;
            Widget widget = (currentNameplate != null) ? currentNameplate.SettlementNameplateInspectedWidget : null;
            ListPanel eventsListPanel = this._eventsListPanel;
            if (currentNameplate == null || textWidget == null || maskedTextureWidget == null || gridWidget == null || widget == null || eventsListPanel == null)
            {
                Debug.FailedAssert("Related widget null on UpdateNameplateTransparencyAndBrightness!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateWidget.cs", "UpdateNameplateTransparencyAndBrightness", 342);
                return;
            }
            float amount = dt * this._lerpModifier;
            if (this.IsVisibleOnMap)
            {
                base.IsVisible = true;
                float valueTo = this.DetermineTargetAlphaValue();
                float valueTo2 = this.DetermineTargetColorFactor();
                float alphaFactor = MathF.Lerp(currentNameplate.AlphaFactor, valueTo, amount, 1E-05f);
                float colorFactor = MathF.Lerp(currentNameplate.ColorFactor, valueTo2, amount, 1E-05f);
                float num = MathF.Lerp(textWidget.ReadOnlyBrush.GlobalAlphaFactor, 1f, amount, 1E-05f);
                currentNameplate.AlphaFactor = alphaFactor;
                currentNameplate.ColorFactor = colorFactor;
                textWidget.Brush.GlobalAlphaFactor = num;
                maskedTextureWidget.Brush.GlobalAlphaFactor = num;
                gridWidget.SetGlobalAlphaRecursively(num);
                eventsListPanel.SetGlobalAlphaRecursively(num);
            }
            else if (currentNameplate.AlphaFactor > this._lerpThreshold)
            {
                float num2 = MathF.Lerp(currentNameplate.AlphaFactor, 0f, amount, 1E-05f);
                currentNameplate.AlphaFactor = num2;
                textWidget.Brush.GlobalAlphaFactor = num2;
                maskedTextureWidget.Brush.GlobalAlphaFactor = num2;
                gridWidget.SetGlobalAlphaRecursively(num2);
                eventsListPanel.SetGlobalAlphaRecursively(num2);
            }
            else
            {
                base.IsVisible = false;
            }
            if (this.IsInRange && this.IsVisibleOnMap)
            {
                if (Math.Abs(widget.AlphaFactor - 1f) > this._lerpThreshold)
                {
                    widget.AlphaFactor = MathF.Lerp(widget.AlphaFactor, 1f, amount, 1E-05f);
                    return;
                }
            }
            else if (currentNameplate.AlphaFactor - 0f > this._lerpThreshold)
            {
                widget.AlphaFactor = MathF.Lerp(widget.AlphaFactor, 0f, amount, 1E-05f);
            }
        }

        private float DetermineTargetAlphaValue()
        {
            if (this.IsInsideWindow)
            {
                if (this.IsTracked)
                {
                    return this._trackedAlphaTarget;
                }
                if (this.RelationType == 0)
                {
                    return this._normalNeutralAlphaTarget;
                }
                if (this.RelationType == 1)
                {
                    return this._normalAllyAlphaTarget;
                }
                return this._normalEnemyAlphaTarget;
            }
            else
            {
                if (this.IsTracked)
                {
                    return this._screenEdgeAlphaTarget;
                }
                return 0f;
            }
        }

        private float DetermineTargetColorFactor()
        {
            if (this.IsTracked)
            {
                return this._trackedColorFactorTarget;
            }
            return this._normalColorFactorTarget;
        }

        public int CompareTo(SCNameplateWidget other)
        {
            return other.DistanceToCamera.CompareTo(this.DistanceToCamera);
        }

        public Vec2 Position
        {
            get
            {
                return this._position;
            }
            set
            {
                if (this._position != value)
                {
                    this._position = value;
                    base.OnPropertyChanged(value, "Position");
                }
            }
        }

        public bool IsVisibleOnMap
        {
            get
            {
                return this._isVisibleOnMap;
            }
            set
            {
                if (this._isVisibleOnMap != value)
                {
                    if (this._isVisibleOnMap && !value)
                    {
                        this._positionTimer = 0f;
                    }
                    this._isVisibleOnMap = value;
                    base.OnPropertyChanged(value, "IsVisibleOnMap");
                }
            }
        }

        public bool IsTracked
        {
            get
            {
                return this._isTracked;
            }
            set
            {
                if (this._isTracked != value)
                {
                    this._isTracked = value;
                    base.OnPropertyChanged(value, "IsTracked");
                }
            }
        }

        public bool IsTargetedByTutorial
        {
            get
            {
                return this._isTargetedByTutorial;
            }
            set
            {
                if (this._isTargetedByTutorial != value)
                {
                    this._isTargetedByTutorial = value;
                    base.OnPropertyChanged(value, "IsTargetedByTutorial");
                    if (value)
                    {
                        this._tutorialAnimState = SCNameplateWidget.TutorialAnimState.Start;
                    }
                }
            }
        }

        public bool IsInsideWindow
        {
            get
            {
                return this._isInsideWindow;
            }
            set
            {
                if (this._isInsideWindow != value)
                {
                    this._isInsideWindow = value;
                    base.OnPropertyChanged(value, "IsInsideWindow");
                }
            }
        }

        public bool IsInRange
        {
            get
            {
                return this._isInRange;
            }
            set
            {
                if (this._isInRange != value)
                {
                    this._isInRange = value;
                }
            }
        }

        public int NameplateType
        {
            get
            {
                return this._nameplateType;
            }
            set
            {
                if (this._nameplateType != value)
                {
                    this._nameplateType = value;
                    base.OnPropertyChanged(value, "NameplateType");
                    this.SetNameplateTypeVisual(value);
                }
            }
        }

        public int RelationType
        {
            get
            {
                return this._relationType;
            }
            set
            {
                if (this._relationType != value)
                {
                    this._relationType = value;
                    base.OnPropertyChanged(value, "RelationType");
                    this.SetNameplateRelationType(value);
                }
            }
        }

        public int WSign
        {
            get
            {
                return this._wSign;
            }
            set
            {
                if (this._wSign != value)
                {
                    this._wSign = value;
                    base.OnPropertyChanged(value, "WSign");
                }
            }
        }

        public float WPos
        {
            get
            {
                return this._wPos;
            }
            set
            {
                if (this._wPos != value)
                {
                    this._wPos = value;
                    base.OnPropertyChanged(value, "WPos");
                }
            }
        }

        public float DistanceToCamera
        {
            get
            {
                return this._distanceToCamera;
            }
            set
            {
                if (this._distanceToCamera != value)
                {
                    this._distanceToCamera = value;
                    base.OnPropertyChanged(value, "DistanceToCamera");
                }
            }
        }

        public ListPanel NotificationListPanel
        {
            get
            {
                return this._notificationListPanel;
            }
            set
            {
                if (this._notificationListPanel != value)
                {
                    this._notificationListPanel = value;
                    base.OnPropertyChanged<ListPanel>(value, "NotificationListPanel");
                    this._notificationListPanel.ItemAddEventHandlers.Add(new Action<Widget, Widget>(this.OnNotificationListUpdated));
                    this._notificationListPanel.ItemAfterRemoveEventHandlers.Add(new Action<Widget>(this.OnNotificationListUpdated));
                }
            }
        }

        public ListPanel EventsListPanel
        {
            get
            {
                return this._eventsListPanel;
            }
            set
            {
                if (value != this._eventsListPanel)
                {
                    this._eventsListPanel = value;
                    base.OnPropertyChanged<ListPanel>(value, "EventsListPanel");
                }
            }
        }

        public SCNameplateItemWidget SmallNameplateWidget
        {
            get
            {
                return this._smallNameplateWidget;
            }
            set
            {
                if (this._smallNameplateWidget != value)
                {
                    this._smallNameplateWidget = value;
                    base.OnPropertyChanged<SCNameplateItemWidget>(value, "SmallNameplateWidget");
                }
            }
        }

        public SCNameplateItemWidget NormalNameplateWidget
        {
            get
            {
                return this._normalNameplateWidget;
            }
            set
            {
                if (this._normalNameplateWidget != value)
                {
                    this._normalNameplateWidget = value;
                    base.OnPropertyChanged<SCNameplateItemWidget>(value, "NormalNameplateWidget");
                }
            }
        }

        public SCNameplateItemWidget BigNameplateWidget
        {
            get
            {
                return this._bigNameplateWidget;
            }
            set
            {
                if (this._bigNameplateWidget != value)
                {
                    this._bigNameplateWidget = value;
                    base.OnPropertyChanged<SCNameplateItemWidget>(value, "BigNameplateWidget");
                }
            }
        }

        private float _positionTimer;

        private SCNameplateItemWidget _currentNameplate;

        private bool _updatePositionNextFrame;

        private SCNameplateWidget.TutorialAnimState _tutorialAnimState;

        private float _lerpThreshold = 5E-05f;

        private float _lerpModifier = 10f;

        private Vector2 _cachedItemSize;

        private bool _lateUpdateActionAdded;

        private Vec2 _position;

        private bool _isVisibleOnMap;

        private bool _isTracked;

        private bool _isInsideWindow;

        private bool _isTargetedByTutorial;

        private int _nameplateType = -1;

        private int _relationType = -1;

        private int _wSign;

        private float _wPos;

        private float _distanceToCamera;

        private bool _isInRange;

        private SCNameplateItemWidget _smallNameplateWidget;

        private SCNameplateItemWidget _normalNameplateWidget;

        private SCNameplateItemWidget _bigNameplateWidget;

        private ListPanel _notificationListPanel;

        private ListPanel _eventsListPanel;

        public enum TutorialAnimState
        {
            Idle,
            Start,
            FirstFrame,
            Playing
        }
    }
}
