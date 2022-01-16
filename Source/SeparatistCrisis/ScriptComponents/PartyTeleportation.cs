#define TRACE

using System;
using System.Collections.Generic;
using SeparatistCrisis.Util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade
{
    public class PartyTeleportation : ScriptComponentBehavior
    {
        public float radius = 1f;
        public bool showDebugInfo = true;
        public int numVerts = 12;
        private Vec3 TargetPosition { get; set; } = Vec3.Zero;
    
        private Vec3 Position { get; set; }

        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            Setup();
        }

        protected override void OnEditorInit()
        {
            base.OnEditorInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            Setup();
        }
        
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => ScriptComponentBehavior.TickRequirement.Tick;
        
        protected override void OnTick(float dt)
        {
            // check performance - we might need to optimize this part
            List<MobileParty> parties = new List<MobileParty>();
            //MBObjectManager.Instance.GetAllInstancesOfObjectType(ref parties);

            foreach (MobileParty party in parties)
            {
                if (IsPartyInArea(party))
                {
                    party.Position2D = TargetPosition.AsVec2;
                    party.SetMoveModeHold();
                }
            }
        }

        protected override void OnEditorTick(float dt)
        {
            Setup();
            MBDebug.ClearRenderObjects();
            if (showDebugInfo)
            {
                Vec3 direction = TargetPosition - Position;
                MBDebug.RenderDebugLine(Position, direction, UInt32.MaxValue, false, dt);
                DebugRender.RenderCircle(Position, radius, numVerts, dt);
            }
        }

        private void Setup()
        {
            Position = GameEntity.GetFrame().origin;
            if (GameEntity.ChildCount > 0)
            {
                TargetPosition = GameEntity.GetChild(0).GlobalPosition;
            }
        }

        private bool IsPartyInArea(MobileParty party)
        {
            float distX = party.GetPosition2D.x - Position.x;
            float distY = party.GetPosition2D.y - Position.y;
            float distance = (distX * distX) + (distY * distY);
            
            return distance <= radius * radius;
        }
    }
}