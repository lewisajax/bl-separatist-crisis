using Helpers;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace SeparatistCrisis.PartyVisuals
{
    public class SCMobilePartyVisual : MapEntityVisual<PartyBase>
    {
        private const float PartyScale = 0.3f;
        private const float HorseAnimationSpeedFactor = 1.3f;
        private float _speed;
        private float _entityAlpha;
        private float _transitionStartRotation;
        private Vec2 _lastFrameVisualPositionWithoutError;
        private bool _isEntityMovingCache;
        private bool _isInTransitionProgressCached;
        private float _bearingRotation;
        private (string, GameEntityComponent) _cachedBannerComponent;
        private (string, GameEntity) _cachedBannerEntity;
        private Scene _mapScene;

        public override float BearingRotation => this._bearingRotation;

        private Scene MapScene
        {
            get
            {
                if ((NativeObject)this._mapScene == (NativeObject)null && Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
                    this._mapScene = ((SandBox.MapScene)Campaign.Current.MapSceneWrapper).Scene;
                return this._mapScene;
            }
        }

        public override MapEntityVisual AttachedTo
        {
            get
            {
                return this.MapEntity.MobileParty?.AttachedTo != null ? (MapEntityVisual)SCMobilePartyVisualManager.Current.GetVisualOfEntity(this.MapEntity.MobileParty.AttachedTo.Party) : (MapEntityVisual)null;
            }
        }

        public override CampaignVec2 InteractionPositionForPlayer
        {
            get => ((IInteractablePoint)this.MapEntity).GetInteractionPosition(MobileParty.MainParty);
        }

        public override bool IsMobileEntity => this.MapEntity.IsMobile;

        public override bool IsMainEntity => this.MapEntity == PartyBase.MainParty;

        public GameEntity StrategicEntity { get; private set; }

        public GameEntity ShipVisuals { get; private set; }

        public AgentVisuals HumanAgentVisuals { get; private set; }

        public AgentVisuals MountAgentVisuals { get; private set; }

        public AgentVisuals CaravanMountAgentVisuals { get; private set; }

        public SCMobilePartyVisual(PartyBase partyBase)
          : base(partyBase)
        {
            this.CircleLocalFrame = MatrixFrame.Identity;
        }

        public override bool IsEnemyOf(IFaction faction)
        {
            return FactionManager.IsAtWarAgainstFaction(this.MapEntity.MapFaction, Hero.MainHero.MapFaction);
        }

        public override bool IsAllyOf(IFaction faction)
        {
            return DiplomacyHelper.IsSameFactionAndNotEliminated(this.MapEntity.MapFaction, Hero.MainHero.MapFaction);
        }

        public void OnPartyRemoved()
        {
            if (!(this.StrategicEntity != (GameEntity)null))
                return;
            this.RemoveVisualFromVisualsOfEntities();
            this.ReleaseResources();
            this.StrategicEntity.Remove(111);
        }

        public override void OnTrackAction()
        {
            MobileParty mobileParty = this.MapEntity.MobileParty;
            if (mobileParty == null)
                return;
            if (Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)mobileParty))
                Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)mobileParty);
            else
                Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)mobileParty);
        }

        public override bool OnMapClick(bool followModifierUsed)
        {
            if (this.IsMainEntity)
            {
                MobileParty.MainParty.SetMoveModeHold();
            }
            else
            {
                MobileParty.NavigationType navigationType;
                if (this.MapEntity.MobileParty.IsCurrentlyAtSea == MobileParty.MainParty.IsCurrentlyAtSea && NavigationHelper.CanPlayerNavigateToPosition(this.MapEntity.MobileParty.Position, out navigationType, out float _))
                {
                    if (followModifierUsed)
                        MobileParty.MainParty.SetMoveEscortParty(this.MapEntity.MobileParty, navigationType, false);
                    else
                        MobileParty.MainParty.SetMoveEngageParty(this.MapEntity.MobileParty, navigationType);
                }
            }
            return true;
        }

        public override void OnHover()
        {
            if (this.MapEntity.MapEvent != null)
            {
                InformationManager.ShowTooltip(typeof(MapEvent), (object)this.MapEntity.MapEvent);
            }
            else
            {
                if (!this.MapEntity.IsMobile || !this.MapEntity.IsVisible)
                    return;
                if (this.MapEntity.MobileParty.Army != null && this.MapEntity.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(this.MapEntity.MobileParty))
                {
                    if (this.MapEntity.MobileParty.Army.LeaderParty.SiegeEvent != null)
                        InformationManager.ShowTooltip(typeof(SiegeEvent), (object)this.MapEntity.MobileParty.Army.LeaderParty.SiegeEvent);
                    else
                        InformationManager.ShowTooltip(typeof(Army), (object)this.MapEntity.MobileParty.Army, (object)false, (object)true);
                }
                else if (this.MapEntity.MobileParty.SiegeEvent != null)
                    InformationManager.ShowTooltip(typeof(SiegeEvent), (object)this.MapEntity.MobileParty.SiegeEvent);
                else
                    InformationManager.ShowTooltip(typeof(MobileParty), (object)this.MapEntity.MobileParty, (object)false, (object)true);
            }
        }

        public override Vec3 GetVisualPosition()
        {
            return this.MapEntity.MobileParty.VisualPosition2DWithoutError.ToVec3(this.MapEntity.Position.AsVec3().Z);
        }

        public override void ReleaseResources() => this.ResetPartyIcon();

        public override bool IsVisibleOrFadingOut() => (double)this._entityAlpha > 0.0;

        public override void OnOpenEncyclopedia()
        {
            if (!this.MapEntity.MobileParty.IsLordParty || this.MapEntity.MobileParty.LeaderHero == null)
                return;
            Campaign.Current.EncyclopediaManager.GoToLink(this.MapEntity.MobileParty.LeaderHero.EncyclopediaLink);
        }

        public void Tick(
          float dt,
          float realDt,
          ref int dirtyPartiesCount,
          ref SCMobilePartyVisual[] dirtyPartiesList)
        {
            if (this.StrategicEntity == (GameEntity)null)
                return;
            if (this.MapEntity.IsVisualDirty && ((double)this._entityAlpha > 0.0 || this.MapEntity.IsVisible))
            {
                int index = Interlocked.Increment(ref dirtyPartiesCount);
                dirtyPartiesList[index] = this;
            }
            if (!this.IsVisibleOrFadingOut() || !(this.StrategicEntity != (GameEntity)null) || this.MapEntity.MobileParty.IsCurrentlyAtSea && !this.MapEntity.MobileParty.IsTransitionInProgress)
                return;
            this.UpdateBearingRotation(realDt, dt);
            this._speed = this.MapEntity.MobileParty.IsActive ? this.MapEntity.MobileParty.Speed : 0.0f;
            float speed = TaleWorlds.Library.MathF.Min((float)(0.25 * (this.MountAgentVisuals != null ? 1.2999999523162842 : 1.0) * (double)this._speed / 0.30000001192092896), 20f);
            bool isEntityMoving = this.IsEntityMovingVisually();
            this.HumanAgentVisuals?.Tick(this.MountAgentVisuals, dt, isEntityMoving, speed);
            this.MountAgentVisuals?.Tick((AgentVisuals)null, dt, isEntityMoving, speed);
            this.CaravanMountAgentVisuals?.Tick((AgentVisuals)null, dt, isEntityMoving, speed);
            MobileParty mobileParty = this.MapEntity.MobileParty;

            MatrixFrame identity = MatrixFrame.Identity;
            identity.origin = this.GetVisualPosition();

            if (mobileParty.Army != null && mobileParty.AttachedTo == mobileParty.Army.LeaderParty && (this.MapEntity.MapEvent == null || !this.MapEntity.MapEvent.IsFieldBattle))
            {
                MatrixFrame frame = this.StrategicEntity.GetFrame();
                Vec2 vec2_1 = identity.origin.AsVec2 - frame.origin.AsVec2;
                if ((double)vec2_1.Length / (double)dt > 20.0)
                    identity.rotation.RotateAboutUp(this._bearingRotation);
                else if (mobileParty.CurrentSettlement == null)
                {
                    Vec2 vec2_2 = frame.rotation.f.AsVec2;
                    double rotationInRadians1 = (double)vec2_2.RotationInRadians;
                    vec2_2 = vec2_1 + Vec2.FromRotation(this._bearingRotation) * 0.01f;
                    double rotationInRadians2 = (double)vec2_2.RotationInRadians;
                    double amount = (double)Math.Min(6f * dt, 1f);
                    double minChange = 0.029999999329447746 * (double)dt;
                    double maxChange = 10.0 * (double)dt;
                    float a = MBMath.LerpRadians((float)rotationInRadians1, (float)rotationInRadians2, (float)amount, (float)minChange, (float)maxChange);
                    identity.rotation.RotateAboutUp(a);
                }
                else
                {
                    float rotationInRadians = frame.rotation.f.AsVec2.RotationInRadians;
                    identity.rotation.RotateAboutUp(rotationInRadians);
                }
            }
            else if (mobileParty.CurrentSettlement == null)
                identity.rotation.RotateAboutUp(this.GetVisualRotation());
            MatrixFrame frame1 = this.StrategicEntity.GetFrame();
            if (!frame1.NearlyEquals(identity))
            {
                this.StrategicEntity.SetFrame(ref identity);

                if (this.ShipVisuals != null)
                {
                    MatrixFrame frame2 = identity;
                    frame2.rotation.ApplyScaleLocal(this.ShipVisuals.GetLocalScale());
                    this.ShipVisuals.SetFrame(ref frame2);
                }

                if (this.HumanAgentVisuals != null)
                {
                    MatrixFrame frame2 = identity;
                    frame2.rotation.ApplyScaleLocal(this.HumanAgentVisuals.GetScale());
                    this.HumanAgentVisuals.GetEntity().SetFrame(ref frame2);
                }

                if (this.MountAgentVisuals != null)
                {
                    MatrixFrame frame3 = identity;
                    frame3.rotation.ApplyScaleLocal(this.MountAgentVisuals.GetScale());
                    this.MountAgentVisuals.GetEntity().SetFrame(ref frame3);
                }

                if (this.CaravanMountAgentVisuals != null)
                {
                    ref MatrixFrame local1 = ref identity;
                    frame1 = this.CaravanMountAgentVisuals.GetFrame();
                    ref MatrixFrame local2 = ref frame1;
                    MatrixFrame parent = local1.TransformToParent(in local2);
                    parent.rotation.ApplyScaleLocal(this.CaravanMountAgentVisuals.GetScale());
                    this.CaravanMountAgentVisuals.GetEntity().SetFrame(ref parent);
                }
            }
            this.ApplyWindEffect();
        }

        private void ApplyWindEffect()
        {
            if (this.HumanAgentVisuals != null && !this.HumanAgentVisuals.GetEquipment()[EquipmentIndex.ExtraWeaponSlot].IsEmpty)
                this.HumanAgentVisuals.SetClothWindToWeaponAtIndex(-this.StrategicEntity.GetGlobalFrame().rotation.f, false, EquipmentIndex.ExtraWeaponSlot);
            if (!((NativeObject)this._cachedBannerComponent.Item2 != (NativeObject)null) || !(this._cachedBannerComponent.Item2 is ClothSimulatorComponent simulatorComponent))
                return;
            float num = this.IsPartOfBesiegerCamp(this.MapEntity) ? 6f : 1f;
            simulatorComponent.SetForcedWind(-this.StrategicEntity.GetGlobalFrame().rotation.f * num, false);
        }

        public void OnStartup()
        {
            if (this.MapEntity.IsMobile)
            {
                this.StrategicEntity = GameEntity.CreateEmpty(this.MapScene);
                if (!this.MapEntity.IsVisible)
                    this.StrategicEntity.EntityFlags |= EntityFlags.DoNotTick;
            }
            CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(this.MapEntity);
            if (true)
            {
                this.CircleLocalFrame = MatrixFrame.Identity;
                if (visualPartyLeader != null && visualPartyLeader.HasMount() || this.MapEntity.MobileParty.IsCaravan)
                {
                    MatrixFrame circleLocalFrame = this.CircleLocalFrame;
                    Mat3 rotation = circleLocalFrame.rotation;
                    rotation.ApplyScaleLocal(0.4625f);
                    circleLocalFrame.rotation = rotation;
                    this.CircleLocalFrame = circleLocalFrame;
                }
                else
                {
                    MatrixFrame circleLocalFrame = this.CircleLocalFrame;
                    Mat3 rotation = circleLocalFrame.rotation;
                    rotation.ApplyScaleLocal(0.3725f);
                    circleLocalFrame.rotation = rotation;
                    this.CircleLocalFrame = circleLocalFrame;
                }
            }
            this._bearingRotation = this.MapEntity.MobileParty.Bearing.RotationInRadians;
            this.StrategicEntity.SetVisibilityExcludeParents(this.MapEntity.IsVisible);
            this.HumanAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(this.MapEntity.IsVisible);
            this.MountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(this.MapEntity.IsVisible);
            this.CaravanMountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(this.MapEntity.IsVisible);
            this.StrategicEntity.SetReadyToRender(true);
            this.StrategicEntity.SetEntityEnvMapVisibility(false);
            this._entityAlpha = 0.0f;
            if (this.MapEntity.IsVisible)
            {
                if (this.MapEntity.MobileParty.IsTransitionInProgress)
                    this.TickFadingState(0.1f, 0.1f);
                else
                    this._entityAlpha = 1f;
            }
            this.AddVisualToVisualsOfEntities();
        }

        public void TickFadingState(float realDt, float dt)
        {
            if ((!this.MapEntity.MobileParty.IsTransitionInProgress || !this.MapEntity.IsVisible) && ((double)this._entityAlpha < 1.0 && this.MapEntity.IsVisible || (double)this._entityAlpha > 0.0 && !this.MapEntity.IsVisible))
            {
                if (this.MapEntity.IsVisible)
                {
                    if ((double)this._entityAlpha <= 0.0)
                    {
                        this.StrategicEntity.SetVisibilityExcludeParents(true);
                        this.ShipVisuals?.SetVisibilityExcludeParents(true);
                        this.HumanAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(true);
                        this.MountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(true);
                        this.CaravanMountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(true);
                    }
                    this._entityAlpha = TaleWorlds.Library.MathF.Min(this._entityAlpha + TaleWorlds.Library.MathF.Max(realDt, 1E-05f), 1f);
                    this.StrategicEntity.SetAlpha(this._entityAlpha);
                    this.ShipVisuals?.SetAlpha(this._entityAlpha);
                    this.HumanAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    this.MountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    this.CaravanMountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    this.StrategicEntity.EntityFlags &= ~EntityFlags.DoNotTick;
                }
                else
                {
                    this._entityAlpha = TaleWorlds.Library.MathF.Max(this._entityAlpha - TaleWorlds.Library.MathF.Max(realDt, 1E-05f), 0.0f);
                    this.StrategicEntity.SetAlpha(this._entityAlpha);
                    this.ShipVisuals?.SetAlpha(this._entityAlpha);
                    this.HumanAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    this.MountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    this.CaravanMountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                    if ((double)this._entityAlpha > 0.0)
                        return;
                    this.StrategicEntity.SetVisibilityExcludeParents(false);
                    this.ShipVisuals?.SetVisibilityExcludeParents(false);
                    this.HumanAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(false);
                    this.MountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(false);
                    this.CaravanMountAgentVisuals?.GetEntity()?.SetVisibilityExcludeParents(false);
                    this.StrategicEntity.EntityFlags |= EntityFlags.DoNotTick;
                }
            }
            else if (this.MapEntity.MobileParty.IsTransitionInProgress)
            {
                if ((this.MapEntity.MobileParty.Army == null || this.MapEntity.MobileParty.Army.LeaderParty == this.MapEntity.MobileParty ? 0 : (this.MapEntity.MobileParty.AttachedTo != null ? 1 : 0)) != 0 || !this.IsMobileEntity || (double)this.GetTransitionProgress() >= 1.0)
                    return;
                this.TickTransitionFadeState(dt);
            }
            else
                SCMobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
        }

        private void UpdateBearingRotation(float realDt, float dt)
        {
            this._bearingRotation += MBMath.WrapAngle(this.MapEntity.MobileParty.Bearing.RotationInRadians - this._bearingRotation) * TaleWorlds.Library.MathF.Min((this.MapEntity.MapEvent != null ? realDt : dt) * 30f, 1f);
            this._bearingRotation = MBMath.WrapAngle(this._bearingRotation);
        }

        private void TickTransitionFadeState(float dt)
        {
            float transitionProgress = this.GetTransitionProgress();
            if (this.MapEntity.MobileParty.IsCurrentlyAtSea)
            {
                this._entityAlpha = transitionProgress;
                this.ShipVisuals?.SetAlpha(this._entityAlpha);
                this.HumanAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                this.MountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                this.CaravanMountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                if (this.HumanAgentVisuals == null)
                    return;
                MatrixFrame frame = this.HumanAgentVisuals.GetEntity().GetFrame();
                CampaignVec2 campaignVec2 = this.MapEntity.MobileParty.EndPositionForNavigationTransition + this.MapEntity.MobileParty.ArmyPositionAdder;
                float x = TaleWorlds.Library.MathF.Lerp(frame.origin.X, campaignVec2.X, dt);
                float y = TaleWorlds.Library.MathF.Lerp(frame.origin.Y, campaignVec2.Y, dt);
                float z = TaleWorlds.Library.MathF.Lerp(frame.origin.z, campaignVec2.AsVec3().Z, dt);
                frame.origin = new Vec3(x, y, z);
                this.ShipVisuals?.SetFrame(ref frame, false);
                this.HumanAgentVisuals.GetEntity()?.SetFrame(ref frame, false);
                this.MountAgentVisuals?.GetEntity()?.SetFrame(ref frame, false);
                this.CaravanMountAgentVisuals?.GetEntity()?.SetFrame(ref frame, false);
            }
            else
            {
                this._entityAlpha = 1f - transitionProgress;
                this.ShipVisuals?.SetAlpha(this._entityAlpha);
                this.HumanAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                this.MountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
                this.CaravanMountAgentVisuals?.GetEntity()?.SetAlpha(this._entityAlpha);
            }
        }

        public void ValidateIsDirty()
        {
            if (this.MapEntity.MemberRoster.TotalManCount != 0)
            {
                this.RefreshPartyIcon();
                if (((double)this._entityAlpha >= 1.0 || !this.MapEntity.IsVisible) && ((double)this._entityAlpha <= 0.0 || this.MapEntity.IsVisible))
                    return;
                SCMobilePartyVisualManager.Current.RegisterFadingVisual(this);
            }
            else
                this.ResetPartyIcon();
        }

        private void RefreshPartyIcon()
        {
            if (!this.MapEntity.IsVisualDirty)
                return;

            this.MapEntity.OnVisualsUpdated();
            bool clearBannerComponentCache = true;
            bool clearBannerEntityCache = true;
            this.ResetPartyIcon();

            MatrixFrame circleLocalFrame = this.CircleLocalFrame;
            circleLocalFrame.origin = Vec3.Zero;
            this.CircleLocalFrame = circleLocalFrame;

            if (this.MapEntity.MobileParty?.CurrentSettlement != null)
            {
                this.AddVisualToVisualsOfEntities();
                if (!this.MapEntity.MobileParty.MapFaction.IsAtWarWith(this.MapEntity.MobileParty.CurrentSettlement.MapFaction) && this.MapEntity.LeaderHero?.ClanBanner != null)
                {
                    string bannerCode = this.MapEntity.LeaderHero.ClanBanner.BannerCode;
                    if (!string.IsNullOrEmpty(bannerCode))
                    {
                        MatrixFrame matrixFrame = MatrixFrame.Identity;
                        Vec3 positionForParty = SettlementVisualManager.Current.GetSettlementVisual(this.MapEntity.MobileParty.CurrentSettlement).GetBannerPositionForParty(this.MapEntity.MobileParty);
                        if (positionForParty.IsValid)
                        {
                            matrixFrame.origin = positionForParty;
                            ref MatrixFrame local1 = ref matrixFrame;
                            MatrixFrame globalFrame = this.StrategicEntity.GetGlobalFrame();
                            Vec3 local2 = globalFrame.TransformToLocal(in matrixFrame.origin);
                            local1.origin = local2;
                            float scaleAmount = MBMath.Map((float)((double)this.MapEntity.NumberOfAllMembers / 400.0 * (this.MapEntity.MobileParty.Army == null || this.MapEntity.MobileParty.Army.LeaderParty != this.MapEntity.MobileParty ? 1.0 : 1.25)), 0.0f, 1f, 0.2f, 0.5f);
                            matrixFrame = matrixFrame.Elevate(-scaleAmount);
                            matrixFrame.rotation.ApplyScaleLocal(scaleAmount);
                            ref MatrixFrame local3 = ref matrixFrame;
                            globalFrame = this.StrategicEntity.GetGlobalFrame();
                            Mat3 local4 = globalFrame.rotation.TransformToLocal(in matrixFrame.rotation);
                            local3.rotation = local4;
                            this.StrategicEntity.AddSphereAsBody(matrixFrame.origin + Vec3.Up * 0.3f, 0.15f, BodyFlags.None);
                            clearBannerComponentCache = false;
                            string bannerMeshName = "campaign_flag";
                            if (this._cachedBannerComponent.Item1 == bannerCode + bannerMeshName)
                            {
                                this._cachedBannerComponent.Item2.GetFirstMetaMesh().Frame = matrixFrame;
                                this.StrategicEntity.AddComponent(this._cachedBannerComponent.Item2);
                            }
                            else
                            {
                                MetaMesh bannerOfCharacter = SCMobilePartyVisual.GetBannerOfCharacter(new Banner(bannerCode), bannerMeshName);
                                bannerOfCharacter.Frame = matrixFrame;
                                int componentCount = this.StrategicEntity.GetComponentCount(GameEntity.ComponentType.ClothSimulator);
                                this.StrategicEntity.AddMultiMesh(bannerOfCharacter);
                                if (this.StrategicEntity.GetComponentCount(GameEntity.ComponentType.ClothSimulator) > componentCount)
                                {
                                    this._cachedBannerComponent.Item1 = bannerCode + bannerMeshName;
                                    this._cachedBannerComponent.Item2 = this.StrategicEntity.GetComponentAtIndex(componentCount, GameEntity.ComponentType.ClothSimulator);
                                }
                            }
                        }
                    }
                }
                else
                    this.StrategicEntity.RemovePhysics();
            }
            else if (this.MapEntity.MobileParty != null && (this.MapEntity.MobileParty.IsCurrentlyAtSea || this.MapEntity.MobileParty.IsTransitionInProgress))
            {
                this.RemoveVisualFromVisualsOfEntities();
                if (this.MapEntity.MobileParty.IsTransitionInProgress)
                {
                    if (this.MapEntity.MobileParty.Army == null || this.MapEntity.MobileParty.Army.LeaderParty == this.MapEntity.MobileParty || this.MapEntity.MobileParty.AttachedTo == null)
                        this.AddMobileIconComponents(this.MapEntity, ref clearBannerComponentCache, ref clearBannerEntityCache);
                    if (!this._isInTransitionProgressCached)
                    {
                        this.AddVisualToVisualsOfEntities();
                        this.OnTransitionStarted();
                    }
                }
                if (this.MapEntity.MobileParty.IsTransitionInProgress != this._isInTransitionProgressCached)
                {
                    if (this._isInTransitionProgressCached)
                        this.OnTransitionEnded();
                    else
                        this.OnTransitionStarted();
                }
            }
            else
            {
                this.AddVisualToVisualsOfEntities();
                this.InitializePartyCollider(this.MapEntity);
                this.AddMobileIconComponents(this.MapEntity, ref clearBannerComponentCache, ref clearBannerEntityCache);
            }
            if (clearBannerComponentCache)
                this._cachedBannerComponent = ((string)null, (GameEntityComponent)null);
            if (clearBannerEntityCache)
                this._cachedBannerEntity = ((string)null, (GameEntity)null);
            this.StrategicEntity.CheckResources(true, false);
            if (!this.IsMobileEntity)
                return;
            this._isInTransitionProgressCached = this.MapEntity.MobileParty.IsTransitionInProgress;
        }

        private void AddMobileIconComponents(PartyBase party, ref bool clearBannerComponentCache, ref bool clearBannerEntityCache)
        {
            uint contourColor = FactionManager.IsAtWarAgainstFaction(party.MapFaction, Hero.MainHero.MapFaction) ? 4294905856U : 4278206719U;

            if (this.IsPartOfBesiegerCamp(party))
            {
                this.AddTentEntityForParty(this.StrategicEntity, party, ref clearBannerComponentCache);
            }
            else
            {
                if (PartyBaseHelper.GetVisualPartyLeader(party) == null)
                    return;

                string bannerKey = (string)null;
                if (party.LeaderHero?.ClanBanner != null)
                    bannerKey = party.LeaderHero.ClanBanner.BannerCode;

                ActionIndexCache leaderAction = ActionIndexCache.act_none;
                ActionIndexCache mountAction = ActionIndexCache.act_none;
                MapEvent mapEvent = party.MobileParty.Army == null || !party.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(party.MobileParty) ? party.MapEvent : party.MobileParty.Army.LeaderParty.MapEvent;

                this.AddShipToPartyIcon(party);

                //int wieldedItemIndex;
                //this.GetMeleeWeaponToWield(party, out wieldedItemIndex);

                //if (mapEvent != null && (mapEvent.EventType == MapEvent.BattleTypes.FieldBattle || mapEvent.EventType == MapEvent.BattleTypes.Raid || mapEvent.EventType == MapEvent.BattleTypes.SiegeOutside || mapEvent.EventType == MapEvent.BattleTypes.SallyOut))
                //    SCMobilePartyVisual.GetPartyBattleAnimation(party, wieldedItemIndex, out leaderAction, out mountAction);
                
                IFaction mapFaction1 = party.MapFaction;
                uint teamColor1 = mapFaction1 != null ? mapFaction1.Color : 4291609515U;
                IFaction mapFaction2 = party.MapFaction;
                uint teamColor2 = mapFaction2 != null ? mapFaction2.Color2 : 4291609515U;
                // this.AddCharacterToPartyIcon(party, PartyBaseHelper.GetVisualPartyLeader(party), contourColor, bannerKey, wieldedItemIndex, teamColor1, teamColor2, in leaderAction, in mountAction, MBRandom.NondeterministicRandomFloat * 0.7f, ref clearBannerEntityCache);
                
                if (!party.IsMobile)
                    return;

                //string mountStringId;
                //string harnessStringId;
                //this.GetMountAndHarnessVisualIdsForPartyIcon(out mountStringId, out harnessStringId);
                
                //if (string.IsNullOrEmpty(mountStringId))
                //    return;

                //this.AddMountToPartyIcon(new Vec3(0.3f, -0.25f), mountStringId, harnessStringId, contourColor, PartyBaseHelper.GetVisualPartyLeader(party));
            }
        }

        private void AddShipToPartyIcon(PartyBase party)
        {

            SpaceShip? spaceShip = party.LeaderHero != null ?
                AssignedSpaceShip.GetShip(party.LeaderHero.CharacterObject) :
                AssignedSpaceShip.GetShip(party);
            string shipName = "v_wing"; // Default ship. We'll probably handle this better later on. Maybe
            if (spaceShip != null)
                shipName = spaceShip.Mesh;
            MetaMesh shipMesh = MetaMesh.GetMultiMesh(shipName);
            // shipMesh.Frame.Scale(new Vec3(.5f, .5f, .5f));
            this.ShipVisuals = GameEntity.CreateEmptyDynamic(this.MapScene);
            this.ShipVisuals.AddMultiMesh(shipMesh);
            MatrixFrame m = this.ShipVisuals.GetFrame();
            m.rotation.ApplyScaleLocal(this.ShipVisuals.GetLocalScale() * (spaceShip?.ScaleMultiplier != null ? spaceShip.ScaleMultiplier : 1f));
            m = this.StrategicEntity.GetFrame().TransformToParent(in m);
            this.ShipVisuals.SetFrame(ref m);
        }

        private void AddMountToPartyIcon(
          Vec3 positionOffset,
          string mountItemId,
          string harnessItemId,
          uint contourColor,
          CharacterObject character)
        {
            ItemObject mountItem = Game.Current.ObjectManager.GetObject<ItemObject>(mountItemId);
            Monster monster = mountItem.HorseComponent.Monster;
            ItemObject itemObject = (ItemObject)null;
            if (!string.IsNullOrEmpty(harnessItemId))
                itemObject = Game.Current.ObjectManager.GetObject<ItemObject>(harnessItemId);
            this.CaravanMountAgentVisuals = AgentVisuals.Create(new AgentVisualsData().Equipment(new Equipment()
            {
                [EquipmentIndex.ArmorItemEndSlot] = new EquipmentElement(mountItem),
                [EquipmentIndex.HorseHarness] = new EquipmentElement(itemObject)
            }).Scale(mountItem.ScaleFactor * 0.3f).Frame(new MatrixFrame(Mat3.Identity, in positionOffset)).ActionSet(MBGlobals.GetActionSet(monster.ActionSetCode + "_map")).Scene(this.MapScene).Monster(monster).PrepareImmediately(false).UseScaledWeapons(true).HasClippingPlane(true).MountCreationKey(MountCreationKey.GetRandomMountKeyString(mountItem, character.GetMountKeySeed())), "PartyIcon " + mountItemId, false, false, false);
            this.CaravanMountAgentVisuals.GetEntity().SetContourColor(new uint?(contourColor), false);
            MatrixFrame m = this.CaravanMountAgentVisuals.GetFrame();
            m.rotation.ApplyScaleLocal(this.CaravanMountAgentVisuals.GetScale());
            m = this.StrategicEntity.GetFrame().TransformToParent(in m);
            this.CaravanMountAgentVisuals.GetEntity().SetFrame(ref m);
            float speed = TaleWorlds.Library.MathF.Min((float)(0.32499998807907104 * (double)this._speed / 0.30000001192092896), 20f);
            this.CaravanMountAgentVisuals.Tick((AgentVisuals)null, 0.0001f, this.IsEntityMovingVisually(), speed);
            this.CaravanMountAgentVisuals.GetEntity().Skeleton.ForceUpdateBoneFrames();
        }

        private void AddCharacterToPartyIcon(
          PartyBase party,
          CharacterObject characterObject,
          uint contourColor,
          string bannerKey,
          int wieldedItemIndex,
          uint teamColor1,
          uint teamColor2,
          in ActionIndexCache leaderAction,
          in ActionIndexCache mountAction,
          float animationStartDuration,
          ref bool clearBannerEntityCache)
        {
            Equipment equipment = characterObject.Equipment.Clone();
            bool flag = !string.IsNullOrEmpty(bannerKey) && ((characterObject.IsPlayerCharacter || characterObject.HeroObject.Clan == Clan.PlayerClan) && Clan.PlayerClan.Tier >= Campaign.Current.Models.ClanTierModel.BannerEligibleTier || !characterObject.IsPlayerCharacter && (!characterObject.IsHero || characterObject.IsHero && characterObject.HeroObject.Clan != Clan.PlayerClan));
            int leftWieldedItemIndex = 4;

            if (flag)
            {
                ItemObject itemObject = Game.Current.ObjectManager.GetObject<ItemObject>("campaign_banner_small");
                equipment[EquipmentIndex.ExtraWeaponSlot] = new EquipmentElement(itemObject);
            }

            Monster baseMonsterFromRace = TaleWorlds.Core.FaceGen.GetBaseMonsterFromRace(characterObject.Race);
            MBActionSet actionSetWithSuffix = MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, characterObject.IsFemale, flag ? "_map_with_banner" : "_map");
            AgentVisualsData data1 = new AgentVisualsData().UseMorphAnims(true).Equipment(equipment).BodyProperties(characterObject.GetBodyProperties(characterObject.Equipment, -1)).SkeletonType(characterObject.IsFemale ? SkeletonType.Female : SkeletonType.Male).Scale(0.3f).Frame(this.StrategicEntity.GetFrame()).ActionSet(actionSetWithSuffix).Scene(this.MapScene).Monster(baseMonsterFromRace).PrepareImmediately(false).RightWieldedItemIndex(wieldedItemIndex).HasClippingPlane(true).UseScaledWeapons(true).ClothColor1(teamColor1).ClothColor2(teamColor2).CharacterObjectStringId(characterObject.StringId).AddColorRandomness(!characterObject.IsHero).Race(characterObject.Race);
            
            if (flag)
            {
                Banner banner = new Banner(bannerKey);
                data1.Banner(banner).LeftWieldedItemIndex(leftWieldedItemIndex);
                if (this._cachedBannerEntity.Item1 == bannerKey + "campaign_banner_small")
                    data1.CachedWeaponEntity(EquipmentIndex.ExtraWeaponSlot, this._cachedBannerEntity.Item2);
            }

            if (!party.MobileParty.IsCurrentlyAtSea || party.MobileParty.IsTransitionInProgress)
                this.HumanAgentVisuals = AgentVisuals.Create(data1, "PartyIcon " + (object)characterObject.Name, false, false, false);

            if (this.HumanAgentVisuals != null)
            {
                if (flag)
                {
                    GameEntity entity = this.HumanAgentVisuals.GetEntity();
                    GameEntity child = entity.GetChild(entity.ChildCount - 1);
                    if (child.GetComponentCount(GameEntity.ComponentType.ClothSimulator) > 0)
                    {
                        clearBannerEntityCache = false;
                        this._cachedBannerEntity = (bannerKey + "campaign_banner_small", child);
                    }
                }

                if (leaderAction != ActionIndexCache.act_none)
                {
                    float animationDuration = MBActionSet.GetActionAnimationDuration(actionSetWithSuffix, in leaderAction);
                    if ((double)animationDuration < 1.0)
                        this.HumanAgentVisuals.GetVisuals().GetSkeleton().SetAgentActionChannel(0, in leaderAction, animationStartDuration);
                    else
                        this.HumanAgentVisuals.GetVisuals().GetSkeleton().SetAgentActionChannel(0, in leaderAction, animationStartDuration / animationDuration);
                }
            }

            if (characterObject.HasMount() && (!party.MobileParty.IsCurrentlyAtSea || party.MobileParty.IsTransitionInProgress))
            {
                Monster monster = characterObject.Equipment[EquipmentIndex.ArmorItemEndSlot].Item.HorseComponent.Monster;
                MBActionSet actionSet = MBGlobals.GetActionSet(monster.ActionSetCode + "_map");
                AgentVisualsData agentVisualsData1 = new AgentVisualsData().Equipment(characterObject.Equipment);
                EquipmentElement equipmentElement = characterObject.Equipment[EquipmentIndex.ArmorItemEndSlot];
                double scale = (double)equipmentElement.Item.ScaleFactor * 0.30000001192092896;
                AgentVisualsData agentVisualsData2 = agentVisualsData1.Scale((float)scale).Frame(MatrixFrame.Identity).ActionSet(actionSet).Scene(this.MapScene).Monster(monster).PrepareImmediately(false).UseScaledWeapons(true).HasClippingPlane(true);
                equipmentElement = characterObject.Equipment[EquipmentIndex.ArmorItemEndSlot];
                string randomMountKeyString = MountCreationKey.GetRandomMountKeyString(equipmentElement.Item, characterObject.GetMountKeySeed());
                AgentVisualsData data2 = agentVisualsData2.MountCreationKey(randomMountKeyString);
                this.MountAgentVisuals = AgentVisuals.Create(data2, "PartyIcon " + (object)characterObject.Name + " mount", false, false, false);

                if (mountAction != ActionIndexCache.act_none)
                {
                    float animationDuration = MBActionSet.GetActionAnimationDuration(actionSet, in mountAction);
                    if ((double)animationDuration < 1.0)
                        this.MountAgentVisuals.GetEntity().Skeleton.SetAgentActionChannel(0, in mountAction, animationStartDuration);
                    else
                        this.MountAgentVisuals.GetEntity().Skeleton.SetAgentActionChannel(0, in mountAction, animationStartDuration / animationDuration);
                }

                this.MountAgentVisuals.GetEntity().SetContourColor(new uint?(contourColor), false);
                MatrixFrame frame = this.StrategicEntity.GetFrame();
                frame.rotation.ApplyScaleLocal(data2.ScaleData);
                this.MountAgentVisuals.GetEntity().SetFrame(ref frame);
            }

            float speed = TaleWorlds.Library.MathF.Min((float)(0.25 * (this.MountAgentVisuals != null ? 1.2999999523162842 : 1.0) * (double)this._speed / 0.30000001192092896), 20f);
            if (this.MountAgentVisuals != null)
            {
                this.MountAgentVisuals.Tick((AgentVisuals)null, 0.0001f, this.IsEntityMovingVisually(), speed);
                this.MountAgentVisuals.GetEntity().Skeleton.ForceUpdateBoneFrames();
            }

            if (this.HumanAgentVisuals == null)
                return;

            this.HumanAgentVisuals.GetEntity().SetContourColor(new uint?(contourColor), false);
            MatrixFrame frame1 = this.StrategicEntity.GetFrame();
            frame1.rotation.ApplyScaleLocal(data1.ScaleData);
            this.HumanAgentVisuals.GetEntity().SetFrame(ref frame1);
            this.HumanAgentVisuals.Tick(this.MountAgentVisuals, 0.0001f, this.IsEntityMovingVisually(), speed);
            this.HumanAgentVisuals.GetEntity().Skeleton.ForceUpdateBoneFrames();
        }

        private bool IsEntityMovingVisually()
        {
            if (this.MapEntity.IsMobile && this.MapEntity.MapEvent != null)
            {
                this._isEntityMovingCache = false;
            }
            else
            {
                if ((double)Campaign.Current.CampaignDt <= 0.0)
                {
                    MobileParty mobileParty = this.MapEntity.MobileParty;
                    if ((mobileParty != null ? (mobileParty.IsMainParty ? 1 : 0) : 0) == 0 || !Campaign.Current.IsMainPartyWaiting)
                        goto label_6;
                }
                this._isEntityMovingCache = false;
                MobileParty mobileParty1 = this.MapEntity.MobileParty;
                if ((mobileParty1 != null ? (!mobileParty1.VisualPosition2DWithoutError.NearlyEquals(this._lastFrameVisualPositionWithoutError) ? 1 : 0) : 0) != 0)
                {
                    this._lastFrameVisualPositionWithoutError = this.MapEntity.MobileParty.VisualPosition2DWithoutError;
                    this._isEntityMovingCache = true;
                }
            }
        label_6:
            if (this._isInTransitionProgressCached)
                this._isEntityMovingCache = true;
            return this._isEntityMovingCache;
        }

        public static MetaMesh GetBannerOfCharacter(Banner banner, string bannerMeshName)
        {
            MetaMesh copy = MetaMesh.GetCopy(bannerMeshName, true, false);
            for (int i = 0; i < copy.MeshCount; i++)
            {
                Mesh meshAtIndex = copy.GetMeshAtIndex(i);
                if (!meshAtIndex.HasTag("dont_use_tableau"))
                {
                    Material material = meshAtIndex.GetMaterial();
                    Material tableauMaterial = null;
                    Tuple<Material, Banner> key = new Tuple<Material, Banner>(material, banner);
                    if (MapScreen.Instance.CharacterBannerMaterialCache.ContainsKey(key))
                    {
                        tableauMaterial = MapScreen.Instance.CharacterBannerMaterialCache[key];
                    }
                    else
                    {
                        tableauMaterial = material.CreateCopy();
                        Action<Texture> setAction = delegate (Texture tex)
                        {
                            tableauMaterial.SetTexture(Material.MBTextureType.DiffuseMap2, tex);
                            uint num = (uint)tableauMaterial.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
                            ulong shaderFlags = tableauMaterial.GetShaderFlags();
                            tableauMaterial.SetShaderFlags(shaderFlags | (ulong)num);
                        };
                        BannerDebugInfo bannerDebugInfo = BannerDebugInfo.CreateManual("MobilePartyVisual");
                        banner.GetTableauTextureLarge(bannerDebugInfo, setAction);
                        MapScreen.Instance.CharacterBannerMaterialCache[key] = tableauMaterial;
                    }
                    meshAtIndex.SetMaterial(tableauMaterial);
                }
            }
            return copy;
        }

        public void AddTentEntityForParty(
          GameEntity strategicEntity,
          PartyBase party,
          ref bool clearBannerComponentCache)
        {
            GameEntity empty = GameEntity.CreateEmpty(strategicEntity.Scene);
            empty.AddMultiMesh(MetaMesh.GetCopy("map_icon_siege_camp_tent"));
            MatrixFrame identity1 = MatrixFrame.Identity;
            identity1.rotation.ApplyScaleLocal(1.2f);
            empty.SetFrame(ref identity1);
            string bannerKey = (string)null;
            if (party.LeaderHero?.ClanBanner != null)
                bannerKey = party.LeaderHero.ClanBanner.BannerCode;
            bool flag = party.MobileParty.Army != null && party.MobileParty.Army.LeaderParty == party.MobileParty;
            MatrixFrame identity2 = MatrixFrame.Identity;
            identity2.origin.z += flag ? 0.2f : 0.15f;
            identity2.rotation.RotateAboutUp(1.57079637f);
            float scaleAmount = MBMath.Map((float)((double)party.CalculateCurrentStrength() / 500.0 * (party.MobileParty.Army != null & flag ? 1.0 : 0.800000011920929)), 0.0f, 1f, 0.15f, 0.5f);
            identity2.rotation.ApplyScaleLocal(scaleAmount);
            if (!string.IsNullOrEmpty(bannerKey))
            {
                clearBannerComponentCache = false;
                string bannerMeshName = "campaign_flag";
                if (this._cachedBannerComponent.Item1 == bannerKey + bannerMeshName)
                {
                    this._cachedBannerComponent.Item2.GetFirstMetaMesh().Frame = identity2;
                    strategicEntity.AddComponent(this._cachedBannerComponent.Item2);
                }
                else
                {
                    MetaMesh bannerOfCharacter = SCMobilePartyVisual.GetBannerOfCharacter(new Banner(bannerKey), bannerMeshName);
                    bannerOfCharacter.Frame = identity2;
                    int componentCount = empty.GetComponentCount(GameEntity.ComponentType.ClothSimulator);
                    empty.AddMultiMesh(bannerOfCharacter);
                    if (empty.GetComponentCount(GameEntity.ComponentType.ClothSimulator) > componentCount)
                    {
                        this._cachedBannerComponent.Item1 = bannerKey + bannerMeshName;
                        this._cachedBannerComponent.Item2 = empty.GetComponentAtIndex(componentCount, GameEntity.ComponentType.ClothSimulator);
                    }
                }
            }
            strategicEntity.AddChild(empty);
            empty.SetVisibilityExcludeParents(true);
        }

        private void GetMeleeWeaponToWield(PartyBase party, out int wieldedItemIndex)
        {
            wieldedItemIndex = -1;
            CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(party);
            if (visualPartyLeader == null)
                return;
            for (int index = 0; index < 5; ++index)
            {
                EquipmentElement equipmentElement = visualPartyLeader.Equipment[index];
                if (equipmentElement.Item != null)
                {
                    equipmentElement = visualPartyLeader.Equipment[index];
                    if (equipmentElement.Item.PrimaryWeapon.IsMeleeWeapon)
                    {
                        wieldedItemIndex = index;
                        break;
                    }
                }
            }
        }

        private static void GetPartyBattleAnimation(
          PartyBase party,
          int wieldedItemIndex,
          out ActionIndexCache leaderAction,
          out ActionIndexCache mountAction)
        {
            leaderAction = ActionIndexCache.act_none;
            mountAction = ActionIndexCache.act_none;
            if (party.MobileParty.Army == null || !party.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(party.MobileParty))
            {
                MapEvent mapEvent1 = party.MapEvent;
            }
            else
            {
                MapEvent mapEvent2 = party.MobileParty.Army.LeaderParty.MapEvent;
            }
            CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(party);
            if (party.MapEvent?.MapEventSettlement != null && visualPartyLeader != null && !visualPartyLeader.HasMount())
            {
                leaderAction = ActionIndexCache.act_map_raid;
            }
            else
            {
                EquipmentElement equipmentElement;
                if (wieldedItemIndex > -1)
                {
                    ItemObject itemObject;
                    if (visualPartyLeader == null)
                    {
                        itemObject = (ItemObject)null;
                    }
                    else
                    {
                        equipmentElement = visualPartyLeader.Equipment[wieldedItemIndex];
                        itemObject = equipmentElement.Item;
                    }
                    if (itemObject != null)
                    {
                        equipmentElement = visualPartyLeader.Equipment[wieldedItemIndex];
                        WeaponComponent weaponComponent = equipmentElement.Item.WeaponComponent;
                        if (weaponComponent != null && weaponComponent.PrimaryWeapon.IsMeleeWeapon)
                        {
                            if (visualPartyLeader.HasMount())
                            {
                                equipmentElement = visualPartyLeader.Equipment[10];
                                if (equipmentElement.Item.HorseComponent.Monster.MonsterUsage == "camel")
                                {
                                    if (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.OneHandedWeapon || weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.TwoHandedWeapon)
                                    {
                                        leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h;
                                        mountAction = ActionIndexCache.act_map_mount_attack_1h;
                                    }
                                    else if (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Polearm)
                                    {
                                        if (weaponComponent.PrimaryWeapon.SwingDamageType == DamageTypes.Invalid)
                                        {
                                            leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h_spear;
                                            mountAction = ActionIndexCache.act_map_mount_attack_spear;
                                        }
                                        else if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedPolearm)
                                        {
                                            leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h_swing;
                                            mountAction = ActionIndexCache.act_map_mount_attack_swing;
                                        }
                                        else
                                        {
                                            leaderAction = ActionIndexCache.act_map_rider_camel_attack_2h_swing;
                                            mountAction = ActionIndexCache.act_map_mount_attack_swing;
                                        }
                                    }
                                }
                                else if (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.OneHandedWeapon || weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.TwoHandedWeapon)
                                {
                                    leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h;
                                    mountAction = ActionIndexCache.act_map_mount_attack_1h;
                                }
                                else if (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Polearm)
                                {
                                    if (weaponComponent.PrimaryWeapon.SwingDamageType == DamageTypes.Invalid)
                                    {
                                        leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h_spear;
                                        mountAction = ActionIndexCache.act_map_mount_attack_spear;
                                    }
                                    else if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedPolearm)
                                    {
                                        leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h_swing;
                                        mountAction = ActionIndexCache.act_map_mount_attack_swing;
                                    }
                                    else
                                    {
                                        leaderAction = ActionIndexCache.act_map_rider_horse_attack_2h_swing;
                                        mountAction = ActionIndexCache.act_map_mount_attack_swing;
                                    }
                                }
                            }
                            else if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedAxe || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Mace || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedSword)
                                leaderAction = ActionIndexCache.act_map_attack_1h;
                            else if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedAxe || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedMace || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedSword)
                                leaderAction = ActionIndexCache.act_map_attack_2h;
                            else if (weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedPolearm || weaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.TwoHandedPolearm)
                                leaderAction = ActionIndexCache.act_map_attack_spear_1h_or_2h;
                        }
                    }
                }
                if (!(leaderAction == ActionIndexCache.act_none))
                    return;
                if (visualPartyLeader.HasMount())
                {
                    equipmentElement = visualPartyLeader.Equipment[10];
                    HorseComponent horseComponent = equipmentElement.Item.HorseComponent;
                    leaderAction = horseComponent.Monster.MonsterUsage == "camel" ? ActionIndexCache.act_map_rider_camel_attack_unarmed : ActionIndexCache.act_map_rider_horse_attack_unarmed;
                    mountAction = ActionIndexCache.act_map_mount_attack_unarmed;
                }
                else
                    leaderAction = ActionIndexCache.act_map_attack_unarmed;
            }
        }

        private void GetMountAndHarnessVisualIdsForPartyIcon(
          out string mountStringId,
          out string harnessStringId)
        {
            mountStringId = "";
            harnessStringId = "";
            if (!this.MapEntity.IsMobile)
                return;
            this.MapEntity.MobileParty.PartyComponent?.GetMountAndHarnessVisualIdsForPartyIcon(this.MapEntity, out mountStringId, out harnessStringId);
        }

        private void InitializePartyCollider(PartyBase party)
        {
            if (!(this.StrategicEntity != (GameEntity)null) || !party.IsMobile)
                return;
            this.StrategicEntity.AddSphereAsBody(new Vec3(), 0.5f, BodyFlags.Moveable | BodyFlags.OnlyCollideWithRaycast);
        }

        private void ResetPartyIcon()
        {
            if (this.ShipVisuals != null)
            {
                this.ShipVisuals = null;
            }
            if (this.HumanAgentVisuals != null)
            {
                this.HumanAgentVisuals.Reset();
                this.HumanAgentVisuals = (AgentVisuals)null;
            }
            if (this.MountAgentVisuals != null)
            {
                this.MountAgentVisuals.Reset();
                this.MountAgentVisuals = (AgentVisuals)null;
            }
            if (this.CaravanMountAgentVisuals != null)
            {
                this.CaravanMountAgentVisuals.Reset();
                this.CaravanMountAgentVisuals = (AgentVisuals)null;
            }
            if (this.StrategicEntity != (GameEntity)null)
            {
                if ((this.StrategicEntity.EntityFlags & EntityFlags.Ignore) != (EntityFlags)0)
                    this.StrategicEntity.RemoveFromPredisplayEntity();
                this.StrategicEntity.ClearComponents();
            }
            this._bearingRotation = this.MapEntity.MobileParty.Bearing.RotationInRadians;
            SCMobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
        }

        private float GetTransitionProgress()
        {
            return this.IsMobileEntity && this.MapEntity.MobileParty.IsTransitionInProgress && this.MapEntity.MobileParty.NavigationTransitionDuration != CampaignTime.Zero ? MBMath.ClampFloat(this.MapEntity.MobileParty.NavigationTransitionStartTime.ElapsedHoursUntilNow / (float)this.MapEntity.MobileParty.NavigationTransitionDuration.ToHours, 0.0f, 1f) : 1f;
        }

        private void OnTransitionStarted()
        {
            SCMobilePartyVisualManager.Current.RegisterFadingVisual(this);
            CampaignVec2 campaignVec2 = this.MapEntity.MobileParty.EndPositionForNavigationTransition;
            Vec2 vec2_1 = campaignVec2.ToVec2();
            campaignVec2 = this.MapEntity.Position;
            Vec2 vec2_2 = campaignVec2.ToVec2();
            this._transitionStartRotation = (vec2_1 - vec2_2).RotationInRadians;
        }

        private void OnTransitionEnded()
        {
        }

        private float GetVisualRotation()
        {
            if (this.MapEntity.IsMobile && this.MapEntity.MapEvent != null && this.MapEntity.MapEvent.IsFieldBattle)
                return this.GetMapEventVisualRotation();
            return this.MapEntity.IsMobile && this.MapEntity.MobileParty.IsTransitionInProgress ? this._transitionStartRotation : this._bearingRotation;
        }

        private float GetMapEventVisualRotation()
        {
            return this.MapEntity.MapEventSide.OtherSide.LeaderParty != null && this.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile && this.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile ? (this.MapEntity.MapEventSide.OtherSide.LeaderParty.MobileParty.VisualPosition2DWithoutError - this.MapEntity.MobileParty.VisualPosition2DWithoutError).Normalized().RotationInRadians : this._bearingRotation;
        }

        private void AddVisualToVisualsOfEntities()
        {
            if (MapScreen.VisualsOfEntities.ContainsKey(this.StrategicEntity.Pointer))
                return;
            MapScreen.VisualsOfEntities.Add(this.StrategicEntity.Pointer, (MapEntityVisual)this);
        }

        private void RemoveVisualFromVisualsOfEntities()
        {
            MapScreen.VisualsOfEntities.Remove(this.StrategicEntity.Pointer);
            foreach (NativeObject child in this.StrategicEntity.GetChildren())
                MapScreen.VisualsOfEntities.Remove(child.Pointer);
        }

        private bool IsPartOfBesiegerCamp(PartyBase party)
        {
            return party.MobileParty.BesiegedSettlement?.SiegeEvent != null && party.MobileParty.BesiegedSettlement.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(party, MapEvent.BattleTypes.Siege);
        }
    }
}
