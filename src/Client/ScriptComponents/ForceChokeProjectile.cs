using System;
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
    public class ForceChokeProjectile: ScriptComponentBehavior
    {
        private float _creationTime;
        private Dictionary<Agent, float>? _hitAgents;

        public Agent? Agent { get; set; }

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

            if (this.Agent != null)
            {
                MatrixFrame globalFrame = this.Agent.AgentVisuals.GetGlobalFrame();
                MatrixFrame boneEntitialFrameWithIndex = this.Agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(this.Agent.Monster.MainHandItemBoneIndex);
                MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

                this.GameEntity.SetGlobalFrame(new MatrixFrame(this.Agent.LookRotation, matrixFrame.origin));
                this.GameEntity.UpdateGlobalBounds();
                this.GameEntity.RecomputeBoundingBox();
            }

            // Scene.BoxCast
            // Scene.SelectEntitiesCollidedWith()

            float radius = base.GameEntity.GlobalBoxMax.AsVec2.Distance(base.GameEntity.GlobalBoxMin.AsVec2);
            MBList<Agent> mblist = this.GetCollidedAgents(radius);

            if (this._hitAgents == null)
                this._hitAgents = new Dictionary<Agent, float>();

            for (int i = 0; i < mblist.Count; i++)
            {
                Agent agent = mblist[i];

                if (this.Agent == agent)
                    continue;

                if (this._hitAgents.TryGetValue(agent, out float time) && time > Mission.Current.CurrentTime + dt + .5f)
                    continue;

                Blow? blow = this.CreateBlow(agent);

                if (blow != null)
                {
                    agent.RegisterBlow((Blow)blow, default(AttackCollisionData));

                    if (this._hitAgents.ContainsKey(agent))
                        this._hitAgents[agent] = Mission.Current.CurrentTime;
                    else
                        this._hitAgents.Add(agent, Mission.Current.CurrentTime);
                }
            }
        }

        public Blow? CreateBlow(Agent victim)
        {
            if (this.Agent != null && victim != null)
            {
                Blow blow = new Blow(this.Agent.Index);

                blow.DamageType = DamageTypes.Blunt;
                blow.BlowFlag |= BlowFlags.KnockDown;
                blow.BaseMagnitude = 0f;
                blow.InflictedDamage = 10;
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
            this._hitAgents = null;
            this.Agent = null;
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this._creationTime = Mission.Current.CurrentTime;
            this._hitAgents = new Dictionary<Agent, float>();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
    }
}
