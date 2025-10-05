using System;
using System.Collections.Generic;
using SeparatistCrisis.Utils;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.ScripComponents
{
    //public class PartyTeleportation : ScriptComponentBehavior
    //{
    //    private Vec3 _targetPosition = Vec3.Zero;
    //    private Vec3 _position;

    //    public float Radius { get; set; } = 1f;
    //    public bool ShowDebugInfo { get; set; } = true;
    //    public int NumVerts { get; set; } = 12;

    //    protected override void OnInit()
    //    {
    //        base.OnInit();
    //        base.SetScriptComponentToTick(this.GetTickRequirement());
    //        Setup();
    //    }

    //    protected override void OnEditorInit()
    //    {
    //        base.OnEditorInit();
    //        base.SetScriptComponentToTick(this.GetTickRequirement());
    //        Setup();
    //    }
        
    //    public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => ScriptComponentBehavior.TickRequirement.Tick;
        
    //    protected override void OnTick(float dt)
    //    {
    //        // check performance - we might need to optimize this part
    //        List<MobileParty> parties = new List<MobileParty>();
    //        //MBObjectManager.Instance.GetAllInstancesOfObjectType(ref parties);

    //        foreach (MobileParty party in parties)
    //        {
    //            if (IsPartyInArea(party))
    //            {
    //                party.Position2D = this._targetPosition.AsVec2;
    //                party.Ai.SetMoveModeHold();
    //            }
    //        }
    //    }

    //    protected override void OnEditorTick(float dt)
    //    {
    //        Setup();
    //        MBDebug.ClearRenderObjects();
    //        if (this.ShowDebugInfo)
    //        {
    //            Vec3 direction = this._targetPosition - this._position;
    //            MBDebug.RenderDebugLine(this._position, direction, UInt32.MaxValue, false, dt);
    //            DebugRender.RenderCircle(this._position, this.Radius, this.NumVerts, dt);
    //        }
    //    }

    //    private void Setup()
    //    {
    //        this._position = GameEntity.GetFrame().origin;
    //        if (GameEntity.ChildCount > 0)
    //        {
    //            this._targetPosition = GameEntity.GetChild(0).GlobalPosition;
    //        }
    //    }

    //    private bool IsPartyInArea(MobileParty party)
    //    {
    //        float distX = party.GetPosition2D.x - this._position.x;
    //        float distY = party.GetPosition2D.y - this._position.y;
    //        float distance = (distX * distX) + (distY * distY);
            
    //        return distance <= this.Radius * this.Radius;
    //    }
    //}
}