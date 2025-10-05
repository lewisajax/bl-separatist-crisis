using Helpers;
using SandBox.ViewModelCollection.Nameplate;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace SeparatistCrisis.ViewModels
{
    public class SCNameplateVM : SettlementNameplateVM
    {
        private Camera _mapCamera;

        protected readonly Action<CampaignVec2> _fastMoveCameraToPosition;

        private object _lockObject = new object();

        protected Vec3 _worldPos;

        protected Vec3 _worldPosWithHeight;

        protected float _heightOffset;

        protected float _latestX;

        protected float _latestY;

        protected float _latestW;

        protected float _wPosAfterPositionCalculation;

        protected bool _latestIsInsideWindow;

        protected bool _isVillage;

        protected bool _isCastle;

        protected bool _isTown;

        protected IFaction _currentFaction;

        protected Banner _latestBanner;

        protected int _latestBannerVersionNo;

        protected float _bindDistanceToCamera;

        protected bool _bindIsVisibleOnMap;

        protected string _bindFactionColor;

        protected bool _bindIsTracked;

        protected BannerImageIdentifierVM _bindBanner;

        protected int _bindRelation;

        protected float _bindWPos;

        protected int _bindWSign;

        protected bool _bindIsInside;

        protected Vec2 _bindPosition;

        protected bool _bindIsInRange;

        protected bool _isGroup;

        public bool IsGroup
        {
            get
            {
                return _isGroup;
            }
            protected set
            {
                _isGroup = value;
            }
        }

        public Camera MapCamera
        {
            get
            {
                return this._mapCamera;
            }
            protected set
            {
                if (this._mapCamera != value)
                {
                    this._mapCamera = value;
                }
            }
        }

        public SettlementGroup? SettlementGroup { get; set; }

        public GameEntity? Entity { get; set; }

        public SCNameplateVM(Settlement settlement, GameEntity? entity, Camera mapCamera, Action<CampaignVec2> fastMoveCameraToPosition, bool isGroup = false) : base(settlement, entity, mapCamera, fastMoveCameraToPosition)
        {
            this._mapCamera = mapCamera;
            this._fastMoveCameraToPosition = fastMoveCameraToPosition;

            if (entity != null)
            {
                this._worldPos = entity.GlobalPosition;
            }
            else
            {
                this._worldPos = this.Settlement.GetPositionAsVec3();
            }

            this._isGroup = isGroup;
            this.AssignSettlementGroup();

            this.Entity = entity;

            this.DetermineSettlementType(settlement);
        }

        public void AssignSettlementGroup()
        {
            if (this.IsGroup)
            {
                this.SettlementGroup = SettlementGroup.FindFirst(x => x.StringId == this.Settlement.StringId);

                if (this.SettlementGroup != null)
                {
                    this.SettlementGroup.Nameplate = this;
                    this.SettlementGroup.MapEntity = this.Settlement;
                }
            }
            else
            {
                this.SettlementGroup = SettlementGroup.FindFirst(x => x.BoundSettlements.Contains(this.Settlement));
            }
        }

        public void DetermineSettlementType(Settlement settlement)
        {
            if (settlement.IsCastle)
            {
                this.SettlementType = 1;
                this._isCastle = true;
            }
            else if (settlement.IsVillage)
            {
                this.SettlementType = 0;
                this._isVillage = true;
            }
            else if (settlement.IsTown)
            {
                this.SettlementType = 2;
                this._isTown = true;
            }
            else if (this.IsGroup)
            {
                this.SettlementType = 3;
            }
            else
            {
                this.SettlementType = 0;
                this._isTown = true;
            }
        }

        public new void UpdateNameplateMT(Vec3 cameraPosition)
        {
            object lockObject = this._lockObject;
            lock (lockObject)
            {
                this.CalculatePosition(cameraPosition);
                this.DetermineIsInsideWindow();
                this.DetermineIsVisibleOnMap(cameraPosition);
                this.SetEntityVisibility(cameraPosition);
                this.RefreshPosition();
                this.RefreshDynamicProperties(false);
            }

            base.UpdateNameplateMT(cameraPosition);
        }

        protected void CalculatePosition(in Vec3 cameraPosition)
        {
            this._worldPosWithHeight = this._worldPos;

            /* When the camera is fully zoomed in, this changes the height of the nameplate so u can still see it and caps it when it reaches 30f height, when u zoom out */
            /* The minimum zoom seems to be 6.338 when over the main body of water, it increases when over higher terrain e.g. it's 15.414 around battania */
            if (this._isVillage)
            {
                this._heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 2.5f;
            }
            else if (this._isCastle)
            {
                this._heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 3f;
            }
            else if (this._isTown)
            {
                this._heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 6f;
            }
            else
            {
                this._heightOffset = 1f;
            }

            /* Sometimes the W value is NaN and needs to be assigned a value */
            this._worldPosWithHeight += new Vec3(0f, 0f, this._heightOffset, -1f);
            if (this._worldPosWithHeight.IsValidXYZW && this._mapCamera.Position.IsValidXYZW)
            {
                this._latestX = 0f;
                this._latestY = 0f;
                this._latestW = 0f;
                MBWindowManager.WorldToScreenInsideUsableArea(this._mapCamera, this._worldPosWithHeight, ref this._latestX, ref this._latestY, ref this._latestW);
            }

            this._wPosAfterPositionCalculation = ((this._latestW < 0f) ? -1f : 1.1f);
        }

        protected void DetermineIsVisibleOnMap(in Vec3 cameraPosition)
        {
            this._bindIsVisibleOnMap = this.IsVisible(cameraPosition);
        }

        protected void DetermineIsInsideWindow()
        {
            this._latestIsInsideWindow = this.IsInsideWindow();
        }

        public void SetEntityVisibility(in Vec3 cameraPosition)
        {
            if (this.Entity != null && this.SettlementGroup != null)
            {
                // Check if camera is zoomed in on system
                bool isCameraInGroup = this.SettlementGroup.Nameplate?.DistanceToCamera - 60f <= this.SettlementGroup.Radius;

                // Check if player is inside system
                bool isPlayerInGroup = Campaign.Current.MainParty.Position.ToVec2().Distance(this.SettlementGroup.Position2D) <= this.SettlementGroup.Radius;

                // If camera height is above 600, hide all bound entities except for the group's primary node
                if (cameraPosition.z > 600)
                {
                    if (this.SettlementGroup != null)
                    {
                        if (!this.IsGroup)
                            this.Entity.SetVisibilityExcludeParents(false);
                    }

                    return;
                }

                // If the camera or the player is not within a group's radius, there's no point showing any settlements
                if (!isCameraInGroup && !isPlayerInGroup)
                {
                    if (!this.IsGroup)
                        this.Entity.SetVisibilityExcludeParents(false);

                    return;
                }

                // If the camera or the player is in a group, then render the settlements
                // Should only have 2 groups max, rendering their settlements
                if (isCameraInGroup || isPlayerInGroup)
                {
                    this.Entity.SetVisibilityExcludeParents(true);
                }
            }
        }

        protected bool IsVisible(in Vec3 cameraPosition)
        {
            this._bindDistanceToCamera = this._worldPos.Distance(cameraPosition);

            bool isCameraInGroup = true;
            bool isPlayerInGroup = false;

            if (this.SettlementGroup != null)
            {
                // Check if camera is zoomed in on system
                isCameraInGroup = this.SettlementGroup.Nameplate?.DistanceToCamera - 60f <= this.SettlementGroup.Radius;

                // Check if player is inside system
                isPlayerInGroup = Campaign.Current.MainParty.Position.ToVec2().Distance(this.SettlementGroup.Position2D) <= this.SettlementGroup.Radius;
            }


            // If they've pinned the settlement
            if (this.IsTracked)
            {
                return true;
            }

            if (this.WPos < 0f || !this._latestIsInsideWindow)
            {
                return false;
            }

            if (cameraPosition.z > 600)
            {
                return this.IsGroup;
            }

            // If the camera or the player are not in a group, only show the group's primary map entity.
            if (!isCameraInGroup && !isPlayerInGroup)
            {
                return this.IsGroup;
            }

            if (cameraPosition.z > 400f)
            {
                return this.Settlement.IsTown;
            }

            /* Village/Hideout nameplates are hidden when above 200 camera height */
            if (cameraPosition.z > 200f)
            {
                return this.Settlement.IsFortification;
            }

            return this._bindDistanceToCamera < cameraPosition.z + 60f;
        }

        public new void ExecuteSetCameraPosition()
        {
            this._fastMoveCameraToPosition(this.Settlement.Position);
        }

        public new void RefreshBindValues()
        {
            object lockObject = this._lockObject;
            lock (lockObject)
            {
                base.FactionColor = this._bindFactionColor;
                this.Banner = this._bindBanner;
                this.Relation = this._bindRelation;
                this.WPos = this._bindWPos;
                this.WSign = this._bindWSign;
                this.IsInside = this._bindIsInside;
                base.Position = this._bindPosition;
                base.IsVisibleOnMap = this._bindIsVisibleOnMap;
                this.IsInRange = this._bindIsInRange;
                base.IsTargetedByTutorial = this._bindIsTargetedByTutorial;
                this.IsTracked = this._bindIsTracked;
                base.DistanceToCamera = this._bindDistanceToCamera;
            }

            if (this.SettlementNotifications.IsEventsRegistered)
            {
                this.SettlementNotifications.Tick();
            }

            if (this.SettlementEvents.IsEventsRegistered)
            {
                this.SettlementEvents.Tick();
            }
        }

        public override void RefreshDynamicProperties(bool forceUpdate)
        {
            base.RefreshDynamicProperties(forceUpdate);
            if ((this._bindIsVisibleOnMap && this._currentFaction != this.Settlement.MapFaction) || forceUpdate)
            {
                string str = "#";
                IFaction mapFaction = this.Settlement.MapFaction;
                this._bindFactionColor = str + Color.UIntToColorString((mapFaction != null) ? mapFaction.Color : uint.MaxValue);
                Banner banner = null;
                if (this.Settlement.OwnerClan != null)
                {
                    banner = this.Settlement.OwnerClan.Banner;
                    IFaction mapFaction2 = this.Settlement.MapFaction;
                    if (mapFaction2 != null && mapFaction2.IsKingdomFaction && ((Kingdom)this.Settlement.MapFaction).RulingClan == this.Settlement.OwnerClan)
                    {
                        banner = this.Settlement.OwnerClan.Kingdom.Banner;
                    }
                }
                int num = (banner != null) ? banner.GetVersionNo() : 0;
                if ((this._latestBanner != banner && !this._latestBanner.IsContentsSameWith(banner)) || this._latestBannerVersionNo != num)
                {
                    this._bindBanner = ((banner != null) ? new BannerImageIdentifierVM(banner, true) : new BannerImageIdentifierVM(null, false));
                    this._latestBannerVersionNo = num;
                    this._latestBanner = banner;
                }
                this._currentFaction = this.Settlement.MapFaction;
            }
            this._bindIsTracked = Campaign.Current.VisualTrackerManager.CheckTracked(this.Settlement);
            if (this.Settlement.IsHideout)
            {
                ISpottable spottable = (ISpottable)this.Settlement.SettlementComponent;
                this._bindIsInRange = (spottable != null && spottable.IsSpotted);
                return;
            }
            this._bindIsInRange = this.Settlement.IsInspected;
        }

        public override void RefreshRelationStatus()
        {
            this._bindRelation = 0;
            if (this.Settlement.OwnerClan != null)
            {
                if (FactionManager.IsAtWarAgainstFaction(this.Settlement.MapFaction, Hero.MainHero.MapFaction))
                {
                    this._bindRelation = 2;
                    return;
                }
                if (DiplomacyHelper.IsSameFactionAndNotEliminated(this.Settlement.MapFaction, Hero.MainHero.MapFaction))
                {
                    this._bindRelation = 1;
                }
            }
        }

        public override void RefreshPosition()
        {
            base.RefreshPosition();
            this._bindWPos = this._wPosAfterPositionCalculation;
            this._bindWSign = (int)this._bindWPos;
            this._bindIsInside = this._latestIsInsideWindow;
            if (this._bindIsVisibleOnMap)
            {
                this._bindPosition = new Vec2(this._latestX, this._latestY);
                return;
            }
            this._bindPosition = new Vec2(-1000f, -1000f);
        }

        protected bool IsInsideWindow()
        {
            return this._latestX <= Screen.RealScreenResolutionWidth && this._latestY <= Screen.RealScreenResolutionHeight && this._latestX + 200f >= 0f && this._latestY + 100f >= 0f;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Initialize(GameEntity strategicEntity)
        {
            base.Initialize(strategicEntity);
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
        }

        public override void RefreshTutorialStatus(string newTutorialHighlightElementID)
        {
            base.RefreshTutorialStatus(newTutorialHighlightElementID);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
