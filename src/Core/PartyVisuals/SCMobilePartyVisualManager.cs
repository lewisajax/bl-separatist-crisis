using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.PartyVisuals
{
    public class SCMobilePartyVisualManager : EntityVisualManagerBase<PartyBase>
    {
        private readonly Dictionary<PartyBase, SCMobilePartyVisual> _partiesAndVisuals = new Dictionary<PartyBase, SCMobilePartyVisual>();
        private readonly List<SCMobilePartyVisual> _visualsFlattened = new List<SCMobilePartyVisual>();
        private int _dirtyPartyVisualCount;
        private SCMobilePartyVisual[] _dirtyPartiesList = new SCMobilePartyVisual[2500];
        private readonly List<SCMobilePartyVisual> _fadingPartiesFlatten = new List<SCMobilePartyVisual>();
        private readonly HashSet<SCMobilePartyVisual> _fadingPartiesSet = new HashSet<SCMobilePartyVisual>();

        public static SCMobilePartyVisualManager Current
        {
            get
            {
                return SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<SCMobilePartyVisualManager>();
            }
        }

        public override void OnTick(float realDt, float dt)
        {
            this._dirtyPartyVisualCount = -1;
            TWParallel.For(0, this._visualsFlattened.Count, (TWParallel.ParallelForAuxPredicate)((startInclusive, endExclusive) =>
            {
                for (int index = startInclusive; index < endExclusive; ++index)
                    this._visualsFlattened[index].Tick(dt, realDt, ref this._dirtyPartyVisualCount, ref this._dirtyPartiesList);
            }));
            for (int index = 0; index < this._dirtyPartyVisualCount + 1; ++index)
                this._dirtyPartiesList[index].ValidateIsDirty();
            for (int index = this._fadingPartiesFlatten.Count - 1; index >= 0; --index)
                this._fadingPartiesFlatten[index].TickFadingState(realDt, dt);
        }

        public override void OnVisualTick(MapScreen screen, float realDt, float dt)
        {
            base.OnVisualTick(screen, realDt, dt);
        }

        public override void OnVisualIntersected(
          Ray mouseRay,
          UIntPtr[] intersectedEntityIDs,
          Intersection[] intersectionInfos,
          int entityCount,
          Vec3 worldMouseNear,
          Vec3 worldMouseFar,
          Vec3 terrainIntersectionPoint,
          float closestDistanceSquared,
          ref MapEntityVisual hoveredVisual,
          ref MapEntityVisual selectedVisual)
        {
            float num1 = TaleWorlds.Library.MathF.Sqrt(closestDistanceSquared) + 1f;
            float num2 = num1;
            for (int index = entityCount - 1; index >= 0; --index)
            {
                UIntPtr intersectedEntityId = intersectedEntityIDs[index];
                MapEntityVisual mapEntityVisual;
                if (intersectedEntityId != UIntPtr.Zero && MapScreen.VisualsOfEntities.TryGetValue(intersectedEntityId, out mapEntityVisual) && mapEntityVisual is SCMobilePartyVisual SCMobilePartyVisual && mapEntityVisual.IsVisibleOrFadingOut() && (!SCMobilePartyVisual.MapEntity.IsMobile || SCMobilePartyVisual.MapEntity.MobileParty.IsMainParty || !SCMobilePartyVisual.MapEntity.MobileParty.IsInRaftState))
                {
                    Intersection intersectionInfo = intersectionInfos[index];
                    float num3 = (worldMouseNear - intersectionInfo.IntersectionPoint).Length - 1.5f;
                    if ((double)num3 < (double)num2)
                    {
                        num2 = num3;
                        hoveredVisual = mapEntityVisual.AttachedTo != null ? mapEntityVisual.AttachedTo : mapEntityVisual;
                    }
                    if ((double)num3 < (double)num1 && !mapEntityVisual.IsMainEntity && (mapEntityVisual.AttachedTo == null || !mapEntityVisual.AttachedTo.IsMainEntity))
                    {
                        num1 = num3;
                        selectedVisual = mapEntityVisual.AttachedTo == null ? mapEntityVisual : mapEntityVisual.AttachedTo;
                    }
                }
            }
        }

        public override MapEntityVisual<PartyBase> GetVisualOfEntity(PartyBase partyBase)
        {
            MobileParty mobileParty = partyBase.MobileParty;
            if ((mobileParty != null ? (!mobileParty.IsCurrentlyAtSea ? 1 : 0) : 0) == 0)
                return (MapEntityVisual<PartyBase>)null;
            SCMobilePartyVisual visualOfEntity;
            this._partiesAndVisuals.TryGetValue(partyBase, out visualOfEntity);
            return (MapEntityVisual<PartyBase>)visualOfEntity;
        }

        protected override void OnFinalize()
        {
            foreach (MapEntityVisual mapEntityVisual in this._partiesAndVisuals.Values)
                mapEntityVisual.ReleaseResources();
            CampaignEventDispatcher.Instance.RemoveListeners((object)this);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            foreach (MobileParty mobileParty in (List<MobileParty>)MobileParty.All)
                this.AddNewPartyVisualForParty(mobileParty, true);
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener((object)this, new Action<MobileParty>(this.OnMobilePartyCreated));
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, new Action<MobileParty, PartyBase>(this.OnMobilePartyDestroyed));
        }

        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            this.RemovePartyVisualForParty(mobileParty);
        }

        private void OnMobilePartyCreated(MobileParty mobileParty)
        {
            this.AddNewPartyVisualForParty(mobileParty);
        }

        public SCMobilePartyVisual GetPartyVisual(PartyBase partyBase)
        {
            return this._partiesAndVisuals[partyBase];
        }

        public void RegisterFadingVisual(SCMobilePartyVisual visual)
        {
            if (this._fadingPartiesSet.Contains(visual))
                return;
            this._fadingPartiesFlatten.Add(visual);
            this._fadingPartiesSet.Add(visual);
        }

        public void UnRegisterFadingVisual(SCMobilePartyVisual visual)
        {
            if (!this._fadingPartiesSet.Contains(visual))
                return;
            this._fadingPartiesFlatten[this._fadingPartiesFlatten.IndexOf(visual)] = this._fadingPartiesFlatten[this._fadingPartiesFlatten.Count - 1];
            this._fadingPartiesFlatten.Remove(this._fadingPartiesFlatten[this._fadingPartiesFlatten.Count - 1]);
            this._fadingPartiesSet.Remove(visual);
        }

        private void AddNewPartyVisualForParty(MobileParty mobileParty, bool shouldTick = false)
        {
            if (mobileParty.IsGarrison || mobileParty.IsMilitia || this._partiesAndVisuals.ContainsKey(mobileParty.Party))
                return;
            SCMobilePartyVisual SCMobilePartyVisual = new SCMobilePartyVisual(mobileParty.Party);
            SCMobilePartyVisual.OnStartup();
            this._partiesAndVisuals.Add(mobileParty.Party, SCMobilePartyVisual);
            this._visualsFlattened.Add(SCMobilePartyVisual);
            if (!shouldTick)
                return;
            SCMobilePartyVisual.Tick(0.1f, 0.1f, ref this._dirtyPartyVisualCount, ref this._dirtyPartiesList);
        }

        private void RemovePartyVisualForParty(MobileParty mobileParty)
        {
            SCMobilePartyVisual SCMobilePartyVisual;
            if (!this._partiesAndVisuals.TryGetValue(mobileParty.Party, out SCMobilePartyVisual))
                return;
            SCMobilePartyVisual.OnPartyRemoved();
            this._visualsFlattened.Remove(SCMobilePartyVisual);
            this._partiesAndVisuals.Remove(mobileParty.Party);
        }
    }
}
