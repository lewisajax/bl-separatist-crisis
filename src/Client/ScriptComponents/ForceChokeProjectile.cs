using SeparatistCrisis.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.ScriptComponents
{
    public class ForceChokeProjectile : ScriptComponentBehavior
    {
        private float _creationTime;
        private Dictionary<Agent, float>? _hitAgents;
        private Dictionary<Agent, GameEntity> _platforms;
        private Dictionary<GameEntity, int>? _platformHeights;

        public AbilityAgent? AbilityAgent { get; set; }

        public static string Name { get; } = typeof(ForceChokeProjectile).Name;

        // If I can figure out how to get a GameEntity/Agent from OnPhysicsCollison, we wouldnt have to do this
        // But I think OnPhysicsCollision is only meant for testing Physics Materials/Sounds etc
        public MBList<Agent> GetCollidedAgents(float radius)
        {
            // Raycast inside the box?
            // LengthSquared or DotProduct

            MBList<Agent> mblist = Mission.Current.GetNearbyAgents(base.GameEntity.GetGlobalFrame().origin.AsVec2, radius, new MBList<Agent>());
            MBList<Agent> newList = new MBList<Agent>();

            for (int i = 0; i < mblist.Count; i++)
            {
                Agent agent = mblist[i];
                Vec3 agentPos = Vec3.Abs(this.GameEntity.GlobalBoxMax) - Vec3.Abs(agent.Position);

                float midZ = this.GameEntity.GlobalBoxMin.z + (this.GameEntity.GetBoundingBoxMax().z / 2);
                bool isInside = this.GameEntity.CheckPointWithOrientedBoundingBox(new Vec3(agent.Position.AsVec2, midZ));

                if (isInside)
                {
                    newList.Add(agent);
                }
            }

            return newList;
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);

            MatrixFrame newFrame = this.GameEntity.GetGlobalFrame();

            if (this.AbilityAgent?.Agent != null)
            {
                MatrixFrame globalFrame = this.AbilityAgent.Agent.AgentVisuals.GetGlobalFrame();
                MatrixFrame boneEntitialFrameWithIndex = this.AbilityAgent.Agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(this.AbilityAgent.Agent.Monster.MainHandItemBoneIndex);
                MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

                this.GameEntity.SetGlobalFrame(new MatrixFrame(this.AbilityAgent.Agent.LookRotation, matrixFrame.origin));
                this.GameEntity.UpdateGlobalBounds();
                this.GameEntity.RecomputeBoundingBox();
            }

            // Scene.BoxCast
            // Scene.SelectEntitiesCollidedWith()

            float radius = base.GameEntity.GlobalBoxMax.AsVec2.Distance(base.GameEntity.GlobalBoxMin.AsVec2);
            MBList<Agent> mblist = this.GetCollidedAgents(radius);

            if (this._hitAgents == null)
                this._hitAgents = new Dictionary<Agent, float>();

            float currTime = Mission.Current.CurrentTime + dt;

            foreach (Agent agent in this._hitAgents.Keys.ToArray())
            {
                if (this._hitAgents[agent] < currTime && !mblist.Contains(agent))
                {
                    agent.SetActionChannel(0, ActionIndexCache.act_none, true);
                    this._hitAgents.Remove(agent);
                }
            }

            for (int i = 0; i < mblist.Count; i++)
            {
                Agent agent = mblist[i];

                if (this.AbilityAgent?.Agent == agent)
                    continue;

                if (this._hitAgents.TryGetValue(agent, out float hitTime) && hitTime > currTime)
                    continue;

                Blow? blow = this.CreateBlow(agent);

                if (blow != null)
                {
                    MBActionSet set = MBActionSet.GetActionSet("as_human_hideout_bandit");
                    AnimationSystemData data = MonsterExtensions.FillAnimationSystemData(agent.Monster, set, agent.Character.GetStepSize(), false);
                    agent.SetActionSet(ref data);
                    ActionIndexCache action = ActionIndexCache.Create("act_scared_idle_1");

                    agent.SetActionChannel(0, action, false);

                    if (this._hitAgents.ContainsKey(agent))
                    {
                        this._hitAgents[agent] = Mission.Current.CurrentTime + .8f;
                    }
                    else
                    {
                        this._hitAgents.Add(agent, Mission.Current.CurrentTime + .8f);
                    }

                }
            }
        }

        public Blow? CreateBlow(Agent victim)
        {
            if (this.AbilityAgent?.Agent != null && victim != null)
            {
                Blow blow = new Blow(this.AbilityAgent.Agent.Index);

                blow.DamageType = DamageTypes.Blunt;
                blow.BlowFlag |= BlowFlags.KnockBack;
                blow.BaseMagnitude = 0f;
                blow.InflictedDamage = 2;
                blow.DamageCalculated = true;
                blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                blow.GlobalPosition = victim.Position;
                blow.BoneIndex = victim.Monster.PelvisBoneIndex;
                blow.SwingDirection = this.GameEntity.GetLinearVelocity();
                blow.Direction = blow.SwingDirection;

                return blow;
            }
            return null;
        }

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            if (this._hitAgents != null)
            {
                foreach (Agent agent in this._hitAgents.Keys)
                {
                    agent.SetActionChannel(0, ActionIndexCache.act_none, true);
                }
            }
            this._hitAgents = null;
            this.AbilityAgent = null;
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this._creationTime = Mission.Current.CurrentTime;
            this._hitAgents = new Dictionary<Agent, float>();
            this._platforms = new Dictionary<Agent, GameEntity>();
            this._platformHeights = new Dictionary<GameEntity, int>();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
    }
}
