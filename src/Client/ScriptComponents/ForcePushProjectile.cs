using HarmonyLib;
using SeparatistCrisis.Entities;
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
    public class ForcePushProjectile: ScriptComponentBehavior
    {
        private float _creationTime;
        private List<Agent>? _hitAgents;

        public AbilityAgent? AbilityAgent { get; set; }

        public static string Name { get; } = typeof(ForcePushProjectile).Name;

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);

            if ((this._creationTime + 0.5f) < Mission.Current.CurrentTime + dt)
            {
                this.GameEntity.Remove(0);
                return;
            }

            MatrixFrame newFrame = this.GameEntity.GetGlobalFrame().Advance(30f * dt);
            this.GameEntity.SetGlobalFrame(newFrame);

            // Scene.BoxCast
            // Scene.SelectEntitiesCollidedWith()

            float radius = this.GameEntity.GlobalBoxMax.Distance(this.GameEntity.GlobalBoxMin);
            // this.GameEntity.PhysicsGlobalBoxMin.Distance(this.GameEntity.PhysicsGlobalBoxMax)
            MBList<Agent> mblist = Mission.Current.GetNearbyAgents(base.GameEntity.GetGlobalFrame().origin.AsVec2, radius, new MBList<Agent>());

            for (int i = 0; i < mblist.Count; i++)
            {
                Agent agent = mblist[i];

                if (this.AbilityAgent?.Agent == agent)
                    continue;

                if (this._hitAgents == null)
                    this._hitAgents = new List<Agent>();

                if (this._hitAgents.Contains(agent))
                    continue;

                // If the agent is around the same height as the collision radius
                if (Math.Abs(this.GameEntity.GetGlobalFrame().origin.Z - agent.Position.Z) < radius)
                {
                    // Use ChargeDamageCallback
                    Blow? blow = this.CreateBlow(agent);

                    if (blow != null)
                    {
                        agent.RegisterBlow((Blow)blow, default(AttackCollisionData));
                        this._hitAgents.Add(agent);
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

        // Was hoping to use OnPhysicsCollision to detect when we hit an agent
        // But we were only able to see what PhysicsShape or Materials it was touching
        // Couldnt find a way to bubble up to a GameEnity from a PhysicsShape pointer

        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            this._hitAgents = null;
            this.AbilityAgent = null;
        }

        protected override void OnInit()
        { 
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this._creationTime = Mission.Current.CurrentTime;
            this._hitAgents = new List<Agent>();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
    }

}