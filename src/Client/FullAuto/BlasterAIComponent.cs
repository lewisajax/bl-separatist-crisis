using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Components
{
    public class BlasterAIComponent : AgentComponent
    {
        private readonly MissionTimer _itemPickUpTickTimer;

        HumanAIComponent AIComponent { get; set; }

        public BlasterAIComponent(Agent agent) : base(agent)
        {
            if (agent != null)
            {
                this.AIComponent = agent.HumanAIComponent;
                this._itemPickUpTickTimer = new MissionTimer(2.5f + MBRandom.RandomFloat);
            }
        }

        public override void OnTickAsAI(float dt)
        {
            if (this._itemPickUpTickTimer.Check(true) && !this.Agent.Mission.MissionEnded)
            {
                EquipmentIndex wieldedItemIndex = this.Agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                WeaponComponentData? weaponComponentData = (wieldedItemIndex == EquipmentIndex.None) ? null : this.Agent.Equipment[wieldedItemIndex].CurrentUsageItem;
                bool flag = weaponComponentData != null && weaponComponentData.IsRangedWeapon; //blaster_heavy, blaster_light

                if (weaponComponentData?.ItemUsage != "blaster")
                    return;

                if (this.Agent.CurrentWatchState == Agent.WatchState.Alarmed && 
                    (this.Agent.GetAgentFlags() & AgentFlag.CanAttack) != AgentFlag.None && 
                    this.Agent.GetCurrentActionType(1) == Agent.ActionCodeType.ReadyRanged)
                {
                    Agent targetAgent = this.Agent.GetTargetAgent();
                    if (targetAgent != null)
                    {
                        Vec3 position = targetAgent.Position;
                        
                        if (flag)
                        {
                            position = targetAgent.Position;
                            if (position.DistanceSquared(this.Agent.Position) < this.Agent.GetMissileRange() * 1.2f && this.Agent.GetLastTargetVisibilityState() == AITargetVisibilityState.TargetIsClear)
                            {
                                // Spawn the missiles
                            }
                        }
                    }
                }
            }
        }
    }
}
