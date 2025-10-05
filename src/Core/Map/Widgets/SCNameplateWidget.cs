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
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;
using TaleWorlds.TwoDimension;
using MathF = TaleWorlds.Library.MathF;

namespace SeparatistCrisis.Widgets
{
    public class SCNameplateWidget: Widget, IComparable<SCNameplateWidget>
    {
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
        private bool _canParley;
        private bool _hasPort;
        private SCNameplateItemWidget _smallNameplateWidget;
        private SCNameplateItemWidget _normalNameplateWidget;
        private SCNameplateItemWidget _bigNameplateWidget;
        private ListPanel _notificationListPanel;
        private ListPanel _eventsListPanel;

        private float _screenEdgeAlphaTarget => 1f;

        private float _normalNeutralAlphaTarget => 0.35f;

        private float _normalAllyAlphaTarget => 0.5f;

        private float _normalEnemyAlphaTarget => 0.35f;

        private float _trackedAlphaTarget => 0.8f;

        private float _trackedColorFactorTarget => 1.3f;

        private float _normalColorFactorTarget => 1f;

        public SCNameplateWidget(UIContext context) : base(context)
        {
        }

        protected override void OnParallelUpdate(float dt)
        {
            base.OnParallelUpdate(dt);
            SCNameplateItemWidget currentNameplate = this._currentNameplate;
            currentNameplate?.ParallelUpdate(dt);
            if (currentNameplate != null && this._cachedItemSize != currentNameplate.Size)
            {
                this._cachedItemSize = currentNameplate.Size;
                ListPanel eventsListPanel = this._eventsListPanel;
                ListPanel notificationListPanel = this._notificationListPanel;
                if (eventsListPanel != null)
                    eventsListPanel.ScaledPositionXOffset = this._cachedItemSize.X;
                if (notificationListPanel != null)
                    notificationListPanel.ScaledPositionYOffset = -this._cachedItemSize.Y;
                this.SuggestedWidth = this._cachedItemSize.X * this._inverseScaleToUse;
                this.SuggestedHeight = this._cachedItemSize.Y * this._inverseScaleToUse;
                this.ScaledSuggestedWidth = this._cachedItemSize.X;
                this.ScaledSuggestedHeight = this._cachedItemSize.Y;
            }
            this.IsEnabled = this.IsVisibleOnMap;
            this.UpdateNameplateTransparencyAndBrightness(dt);
            this.UpdatePosition(dt);
            this.UpdateTutorialState();
        }

        //private void UpdatePosition(float dt)
        //{
        //    SCNameplateItemWidget currentNameplate = this._currentNameplate;

        //    MapEventVisualBrushWidget mapEventVisualBrushWidget = (currentNameplate != null) ? currentNameplate.MapEventVisualWidget : null;
        //    if (currentNameplate == null || mapEventVisualBrushWidget == null)
        //    {
        //        Debug.FailedAssert("Related widget null on UpdatePosition!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateWidget.cs", "UpdatePosition", 105);
        //        return;
        //    }
        //    bool flag = false;
        //    this._positionTimer += dt;
        //    if (this.IsVisibleOnMap || this._positionTimer < 2f)
        //    {
        //        float x = base.Context.EventManager.PageSize.X;
        //        float y = base.Context.EventManager.PageSize.Y;
        //        Vec2 vec = this.Position;
        //        if (this.IsTracked)
        //        {
        //            if (this.WSign > 0 && vec.x - base.Size.X / 2f > 0f && vec.x + base.Size.X / 2f < x && vec.y > 0f && vec.y + base.Size.Y < y)
        //            {
        //                base.ScaledPositionXOffset = vec.x - base.Size.X / 2f;
        //                base.ScaledPositionYOffset = vec.y - base.Size.Y;
        //            }
        //            else
        //            {
        //                Vec2 vec2 = new Vec2(x / 2f, y / 2f);
        //                vec -= vec2;
        //                if (this.WSign < 0)
        //                {
        //                    vec *= -1f;
        //                }
        //                float radian = Mathf.Atan2(vec.y, vec.x) - 1.5707964f;
        //                float num = Mathf.Cos(radian);
        //                float num2 = Mathf.Sin(radian);
        //                float num3 = num / num2;
        //                Vec2 vec3 = vec2 * 1f;
        //                vec = ((num > 0f) ? new Vec2(-vec3.y / num3, vec2.y) : new Vec2(vec3.y / num3, -vec2.y));
        //                if (vec.x > vec3.x)
        //                {
        //                    vec = new Vec2(vec3.x, -vec3.x * num3);
        //                }
        //                else if (vec.x < -vec3.x)
        //                {
        //                    vec = new Vec2(-vec3.x, vec3.x * num3);
        //                }
        //                vec += vec2;
        //                flag = (vec.y - base.Size.Y - mapEventVisualBrushWidget.Size.Y <= 0f);
        //                base.ScaledPositionXOffset = Mathf.Clamp(vec.x - base.Size.X / 2f, 0f, x - currentNameplate.Size.X);
        //                base.ScaledPositionYOffset = Mathf.Clamp(vec.y - base.Size.Y, 0f, y - (currentNameplate.Size.Y + 55f));
        //            }
        //        }
        //        else
        //        {
        //            base.ScaledPositionXOffset = vec.x - base.Size.X / 2f;
        //            base.ScaledPositionYOffset = vec.y - base.Size.Y;
        //        }
        //    }
        //    if (flag)
        //    {
        //        mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Bottom;
        //        mapEventVisualBrushWidget.ScaledPositionYOffset = mapEventVisualBrushWidget.Size.Y;
        //        return;
        //    }
        //    mapEventVisualBrushWidget.VerticalAlignment = VerticalAlignment.Top;
        //    mapEventVisualBrushWidget.ScaledPositionYOffset = -mapEventVisualBrushWidget.Size.Y;
        //}

        private void UpdatePosition(float dt)
        {
            SCNameplateItemWidget currentNameplate = this._currentNameplate;
            MapEventVisualBrushWidget eventVisualWidget = currentNameplate?.MapEventVisualWidget;
            if (currentNameplate == null || eventVisualWidget == null)
            {
                Debug.FailedAssert("Related widget null on UpdatePosition!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateWidget.cs", nameof(UpdatePosition), 105);
            }
            else
            {
                bool flag1 = false;
                this._positionTimer += dt;
                if (this.IsVisibleOnMap || (double)this._positionTimer < 2.0)
                {
                    float a = this.Position.X - this.Size.X / 2f - this.ScaledMarginLeft;
                    Vec2 position = this.Position;
                    float num1 = position.X + this.Size.X / 2f + this.ScaledMarginRight;
                    position = this.Position;
                    float b = position.Y - this.Size.Y - this.ScaledMarginTop;
                    position = this.Position;
                    float num2 = position.Y + this.ScaledMarginBottom;
                    bool flag2 = this.WSign > 0 && (double)a > 0.0 && (double)num1 < (double)this.Context.EventManager.PageSize.X && (double)b > 0.0 && (double)num2 < (double)this.Context.EventManager.PageSize.Y;

                    if (this.IsTracked && !flag2)
                    {
                        Vec2 vec2_1 = new Vec2(a, b);
                        Vector2 vector2 = this.Context.EventManager.PageSize - this.Size;
                        vector2.X -= this.ScaledMarginLeft + this.ScaledMarginRight;
                        vector2.Y -= this.ScaledMarginTop + this.ScaledMarginBottom;
                        Vec2 vec2_2 = (Vec2)(vector2 / 2f);
                        Vec2 vec2_3 = vec2_1 - vec2_2;

                        if (this.WSign < 0)
                            vec2_3 *= -1f;

                        double radian = (double)Mathf.Atan2(vec2_3.y, vec2_3.x) - 1.5707963705062866;
                        float num3 = Mathf.Cos((float)radian);
                        float num4 = Mathf.Sin((float)radian);
                        float num5 = num3 / num4;
                        Vec2 vec2_4 = vec2_2 * 1f;
                        Vec2 vec2_5 = (double)num3 > 0.0 ? new Vec2(-vec2_4.y / num5, vec2_2.y) : new Vec2(vec2_4.y / num5, -vec2_2.y);

                        if ((double)vec2_5.x > (double)vec2_4.x)
                            vec2_5 = new Vec2(vec2_4.x, -vec2_4.x * num5);
                        else if ((double)vec2_5.x < -(double)vec2_4.x)
                            vec2_5 = new Vec2(-vec2_4.x, vec2_4.x * num5);

                        vec2_5 += vec2_2;
                        this.ScaledPositionXOffset = Mathf.Clamp(vec2_5.x, 0.0f, vector2.X);
                        this.ScaledPositionYOffset = Mathf.Clamp(vec2_5.y, 0.0f, vector2.Y);
                    }
                    else
                    {
                        this.ScaledPositionXOffset = a;
                        this.ScaledPositionYOffset = b;
                    }
                    flag1 = (double)this.ScaledPositionYOffset - (double)eventVisualWidget.Size.Y < 0.0;
                }
                if (flag1)
                {
                    eventVisualWidget.VerticalAlignment = VerticalAlignment.Bottom;
                    eventVisualWidget.ScaledPositionYOffset = eventVisualWidget.Size.Y;
                }
                else
                {
                    eventVisualWidget.VerticalAlignment = VerticalAlignment.Top;
                    eventVisualWidget.ScaledPositionYOffset = -eventVisualWidget.Size.Y;
                }
            }
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
            if (this._lateUpdateActionAdded)
                return;
            this.EventManager.AddLateUpdateAction((Widget)this, new Action<float>(this.CustomLateUpdate), 1);
            this._lateUpdateActionAdded = true;
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
                int tutorialAnimState = (int)this._tutorialAnimState;
            }
            if (this.IsTargetedByTutorial)
                this.SetState("Default");
            else
                this.SetState("Disabled");
        }

        private void SetNameplateTypeVisual(int type)
        {
            if (this._currentNameplate != null)
                return;

            this.SmallNameplateWidget.IsVisible = false;
            this.NormalNameplateWidget.IsVisible = false;
            this.BigNameplateWidget.IsVisible = false;

            switch (type)
            {
                case 0:
                    this._currentNameplate = this.SmallNameplateWidget;
                    this.SmallNameplateWidget.IsVisible = true;
                    break;
                case 1:
                    this._currentNameplate = this.NormalNameplateWidget;
                    this.NormalNameplateWidget.IsVisible = true;
                    break;
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

        private void SetNameplateRelationType(int type)
        {
            if (this._currentNameplate == null)
                return;
            switch (type)
            {
                case 0:
                    this._currentNameplate.Color = Color.Black;
                    break;
                case 1:
                    this._currentNameplate.Color = Color.ConvertStringToColor("#245E05FF");
                    break;
                case 2:
                    this._currentNameplate.Color = Color.ConvertStringToColor("#870707FF");
                    break;
            }
        }

        private void UpdateNameplateTransparencyAndBrightness(float dt)
        {
            SCNameplateItemWidget currentNameplate = this._currentNameplate;
            TextWidget settlementNameTextWidget = currentNameplate?.SettlementNameTextWidget;
            MaskedTextureWidget settlementBannerWidget = currentNameplate?.SettlementBannerWidget;
            GridWidget partiesGridWidget = currentNameplate?.SettlementPartiesGridWidget;
            Widget inspectedIconWidget = currentNameplate?.InspectedIconWidget;
            Widget portIconWidget = currentNameplate?.PortIconWidget;
            Widget parleyIconWidget = currentNameplate?.ParleyIconWidget;
            ListPanel eventsListPanel = this._eventsListPanel;
            if (currentNameplate == null || settlementNameTextWidget == null || settlementBannerWidget == null || partiesGridWidget == null || inspectedIconWidget == null || portIconWidget == null || parleyIconWidget == null || eventsListPanel == null)
            {
                Debug.FailedAssert("Related widget null on UpdateNameplateTransparencyAndBrightness!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI.Widgets\\Nameplate\\SCNameplateWidget.cs", nameof(UpdateNameplateTransparencyAndBrightness), 337);
            }
            else
            {
                portIconWidget.IsVisible = this.HasPort;
                float amount = dt * this._lerpModifier;
                if (this.IsVisibleOnMap)
                {
                    this.IsVisible = true;
                    float targetAlphaValue = this.DetermineTargetAlphaValue();
                    float targetColorFactor = this.DetermineTargetColorFactor();
                    float num1 = TaleWorlds.Library.MathF.Lerp(currentNameplate.AlphaFactor, targetAlphaValue, amount);
                    float num2 = TaleWorlds.Library.MathF.Lerp(currentNameplate.ColorFactor, targetColorFactor, amount);
                    float alphaFactor = TaleWorlds.Library.MathF.Lerp(settlementNameTextWidget.ReadOnlyBrush.GlobalAlphaFactor, 1f, amount);
                    currentNameplate.AlphaFactor = num1;
                    currentNameplate.ColorFactor = num2;
                    settlementNameTextWidget.Brush.GlobalAlphaFactor = alphaFactor;
                    settlementBannerWidget.Brush.GlobalAlphaFactor = alphaFactor;
                    partiesGridWidget.SetGlobalAlphaRecursively(alphaFactor);
                    parleyIconWidget.AlphaFactor = TaleWorlds.Library.MathF.Lerp(parleyIconWidget.AlphaFactor, this.CanParley ? 1f : 0.0f, amount);
                    eventsListPanel.SetGlobalAlphaRecursively(alphaFactor);
                }
                else if ((double)currentNameplate.AlphaFactor > (double)this._lerpThreshold)
                {
                    float alphaFactor = TaleWorlds.Library.MathF.Lerp(currentNameplate.AlphaFactor, 0.0f, amount);
                    currentNameplate.AlphaFactor = alphaFactor;
                    settlementNameTextWidget.Brush.GlobalAlphaFactor = alphaFactor;
                    settlementBannerWidget.Brush.GlobalAlphaFactor = alphaFactor;
                    partiesGridWidget.SetGlobalAlphaRecursively(alphaFactor);
                    parleyIconWidget.AlphaFactor = alphaFactor;
                    eventsListPanel.SetGlobalAlphaRecursively(alphaFactor);
                }
                else
                    this.IsVisible = false;
                if (this.IsInRange && this.IsVisibleOnMap)
                {
                    if ((double)Math.Abs(inspectedIconWidget.AlphaFactor - 1f) <= (double)this._lerpThreshold)
                        return;
                    inspectedIconWidget.AlphaFactor = TaleWorlds.Library.MathF.Lerp(inspectedIconWidget.AlphaFactor, 1f, amount);
                }
                else
                {
                    if ((double)currentNameplate.AlphaFactor - 0.0 <= (double)this._lerpThreshold)
                        return;
                    inspectedIconWidget.AlphaFactor = TaleWorlds.Library.MathF.Lerp(inspectedIconWidget.AlphaFactor, 0.0f, amount);
                }
            }
        }

        private float DetermineTargetAlphaValue()
        {
            if (this.IsInsideWindow)
            {
                if (this.IsTracked)
                    return this._trackedAlphaTarget;
                if (this.RelationType == 0)
                    return this._normalNeutralAlphaTarget;
                return this.RelationType == 1 ? this._normalAllyAlphaTarget : this._normalEnemyAlphaTarget;
            }
            return this.IsTracked ? this._screenEdgeAlphaTarget : 0.0f;
        }

        private float DetermineTargetColorFactor()
        {
            return this.IsTracked ? this._trackedColorFactorTarget : this._normalColorFactorTarget;
        }

        public int CompareTo(SCNameplateWidget other)
        {
            return other.DistanceToCamera.CompareTo(this.DistanceToCamera);
        }

        public Vec2 Position
        {
            get => this._position;
            set
            {
                if (!(this._position != value))
                    return;
                this._position = value;
                this.OnPropertyChanged(value, nameof(Position));
            }
        }

        public bool IsVisibleOnMap
        {
            get => this._isVisibleOnMap;
            set
            {
                if (this._isVisibleOnMap == value)
                    return;
                if (this._isVisibleOnMap && !value)
                    this._positionTimer = 0.0f;
                this._isVisibleOnMap = value;
                this.OnPropertyChanged(value, nameof(IsVisibleOnMap));
            }
        }

        public bool IsTracked
        {
            get => this._isTracked;
            set
            {
                if (this._isTracked == value)
                    return;
                this._isTracked = value;
                this.OnPropertyChanged(value, nameof(IsTracked));
            }
        }

        public bool IsTargetedByTutorial
        {
            get => this._isTargetedByTutorial;
            set
            {
                if (this._isTargetedByTutorial == value)
                    return;
                this._isTargetedByTutorial = value;
                this.OnPropertyChanged(value, nameof(IsTargetedByTutorial));
                if (!value)
                    return;
                this._tutorialAnimState = SCNameplateWidget.TutorialAnimState.Start;
            }
        }

        public bool IsInsideWindow
        {
            get => this._isInsideWindow;
            set
            {
                if (this._isInsideWindow == value)
                    return;
                this._isInsideWindow = value;
                this.OnPropertyChanged(value, nameof(IsInsideWindow));
            }
        }

        public bool IsInRange
        {
            get => this._isInRange;
            set
            {
                if (this._isInRange == value)
                    return;
                this._isInRange = value;
            }
        }

        public bool CanParley
        {
            get => this._canParley;
            set
            {
                if (this._canParley == value)
                    return;
                this._canParley = value;
                this.OnPropertyChanged(value, nameof(CanParley));
            }
        }

        public bool HasPort
        {
            get => this._hasPort;
            set
            {
                if (value == this._hasPort)
                    return;
                this._hasPort = value;
                this.OnPropertyChanged(value, nameof(HasPort));
            }
        }

        public int NameplateType
        {
            get => this._nameplateType;
            set
            {
                if (this._nameplateType == value)
                    return;
                this._nameplateType = value;
                this.OnPropertyChanged(value, nameof(NameplateType));
                this.SetNameplateTypeVisual(value);
            }
        }

        public int RelationType
        {
            get => this._relationType;
            set
            {
                if (this._relationType == value)
                    return;
                this._relationType = value;
                this.OnPropertyChanged(value, nameof(RelationType));
                this.SetNameplateRelationType(value);
            }
        }

        public int WSign
        {
            get => this._wSign;
            set
            {
                if (this._wSign == value)
                    return;
                this._wSign = value;
                this.OnPropertyChanged(value, nameof(WSign));
            }
        }

        public float WPos
        {
            get => this._wPos;
            set
            {
                if ((double)this._wPos == (double)value)
                    return;
                this._wPos = value;
                this.OnPropertyChanged(value, nameof(WPos));
            }
        }

        public float DistanceToCamera
        {
            get => this._distanceToCamera;
            set
            {
                if ((double)this._distanceToCamera == (double)value)
                    return;
                this._distanceToCamera = value;
                this.OnPropertyChanged(value, nameof(DistanceToCamera));
            }
        }

        public ListPanel NotificationListPanel
        {
            get => this._notificationListPanel;
            set
            {
                if (this._notificationListPanel == value)
                    return;
                this._notificationListPanel = value;
                this.OnPropertyChanged<ListPanel>(value, nameof(NotificationListPanel));
                this._notificationListPanel.ItemAddEventHandlers.Add(new Action<Widget, Widget>(this.OnNotificationListUpdated));
                this._notificationListPanel.ItemAfterRemoveEventHandlers.Add(new Action<Widget>(this.OnNotificationListUpdated));
            }
        }

        public ListPanel EventsListPanel
        {
            get => this._eventsListPanel;
            set
            {
                if (value == this._eventsListPanel)
                    return;
                this._eventsListPanel = value;
                this.OnPropertyChanged<ListPanel>(value, nameof(EventsListPanel));
            }
        }

        public SCNameplateItemWidget SmallNameplateWidget
        {
            get => this._smallNameplateWidget;
            set
            {
                if (this._smallNameplateWidget == value)
                    return;
                this._smallNameplateWidget = value;
                this.OnPropertyChanged<SCNameplateItemWidget>(value, nameof(SmallNameplateWidget));
            }
        }

        public SCNameplateItemWidget NormalNameplateWidget
        {
            get => this._normalNameplateWidget;
            set
            {
                if (this._normalNameplateWidget == value)
                    return;
                this._normalNameplateWidget = value;
                this.OnPropertyChanged<SCNameplateItemWidget>(value, nameof(NormalNameplateWidget));
            }
        }

        public SCNameplateItemWidget BigNameplateWidget
        {
            get => this._bigNameplateWidget;
            set
            {
                if (this._bigNameplateWidget == value)
                    return;
                this._bigNameplateWidget = value;
                this.OnPropertyChanged<SCNameplateItemWidget>(value, nameof(BigNameplateWidget));
            }
        }

        public enum TutorialAnimState
        {
            Idle,
            Start,
            FirstFrame,
            Playing,
        }
    }
}
