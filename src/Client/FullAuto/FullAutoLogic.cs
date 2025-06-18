using HarmonyLib;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace SeparatistCrisis.Behaviors
{
    public class FullAutoLogic: MissionLogic
    {
        private MissionState? _state;
        private Dictionary<Agent, float> _lastShotTimes = null!;
        private delegate void OnAgentShootMissileDelegate(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, bool isPrimaryWeaponShot, int forcedMissileIndex);
        private OnAgentShootMissileDelegate AgentShootMissile = null!;
        // private UIntPtr _missionPointer;


        public override void EarlyStart()
        {
            this._state = Game.Current.GameStateManager.ActiveState as MissionState;
            this._lastShotTimes = new Dictionary<Agent, float>();

            /*PropertyInfo delegateProperty = AccessTools.Property(AccessTools.TypeByName("ManagedCallbacks.CoreCallbacksGenerated"), "Delegates");
            Delegate[]? delegates = (Delegate[]?)delegateProperty.GetValue(delegateProperty);

            if (delegates != null)
            {
                this._onAgentShootMissileDelegate = delegates.FirstOrDefault(x => x.Method.Name == "Mission_OnAgentShootMissile");
            }

            this._missionPointer = (UIntPtr)Traverse.Create(this.Mission).Property("Pointer").GetValue();*/

            // Traverse val = Traverse.Create(this.Mission).Method("OnAgentShootMissile", new[] { typeof(Agent), typeof(EquipmentIndex), typeof(Vec3), typeof(Vec3), typeof(Mat3), typeof(bool), typeof(bool), typeof(int) });
            
            MethodInfo methodInfo = AccessTools.Method(typeof(Mission), "OnAgentShootMissile", new Type[] { typeof(Agent), typeof(EquipmentIndex), typeof(Vec3), typeof(Vec3), typeof(Mat3), typeof(bool), typeof(bool), typeof(int) });
            AgentShootMissile = (OnAgentShootMissileDelegate)methodInfo.CreateDelegate(typeof(OnAgentShootMissileDelegate), this.Mission);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            for (int i = 0; i < base.Mission.Agents.Count; i++)
            {
                Agent agent = base.Mission.Agents[i];
                if (agent != null && agent.IsActive())
                {
                    if (!this._lastShotTimes.ContainsKey(agent))
                        this._lastShotTimes.Add(agent, Mission.Current.CurrentTime);

                    if (agent.GetCurrentActionType(1) == Agent.ActionCodeType.ReadyRanged &&
                        Mission.Current.CurrentTime - this._lastShotTimes[agent] > 0.4f &&
                        (!agent.WieldedWeapon.IsEmpty && !agent.WieldedWeapon.AmmoWeapon.IsEmpty))
                    {
                        this.ShootWeapon(agent);
                        this._lastShotTimes[agent] = Mission.Current.CurrentTime;
                    }
                }
            }
        }

        private void ShootWeapon(Agent agent)
        {
            MissionWeapon weapon = agent.WieldedWeapon;
            EquipmentIndex equipIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);

            if (weapon.Item == null)
                return;

            // GameEntity wepEnt = agent.GetWeaponEntityFromEquipmentSlot(equipIndex);

            MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(agent.Monster.MainHandItemBoneIndex);
            MatrixFrame globalFrame = agent.AgentVisuals.GetGlobalFrame();
            MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

            Vec3 direction = agent.LookDirection;
            // Vec3 direction = new Vec3(agent.GetMovementDirection());

            Vec3 position = matrixFrame.origin;
            Mat3 orientation = Mat3.CreateMat3WithForward(direction);
            float baseSpeed = weapon.GetModifiedMissileSpeedForCurrentUsage();
            float speed = baseSpeed + agent.Character.GetSkillValue(DefaultSkills.Crossbow);
            Vec3 velocity = direction * speed;

            /*if (weapon.AmmoWeapon.GetConsumableIfAny(out WeaponComponentData ammoType))
            {
                speed *= ammoType.GetModifiedMissileSpeed(null);
            }*/

            // Mission.Current.AddCustomMissile(agent, weapon.AmmoWeapon, position, direction, orientation, baseSpeed, speed, true, null);
            // this.OnAgentShootMissile(agent, equipIndex, position, velocity, orientation, true, -1);
            if (this.AgentShootMissile != null)
            {
                // this._onAgentShootMissileDelegate.GetMethodInfo().Invoke(this._onAgentShootMissileDelegate.Target, new object[] { this._missionPointer, agent, equipIndex, position, velocity, orientation, true, true, -1 });
                this.AgentShootMissile.DynamicInvoke(new object[] { agent, equipIndex, position, velocity, orientation, true, true, -1 });
                // FastInvokeHandler invoker =  MethodInvoker.GetHandler(this.OnAgentShootMissileDelegate.GetMethodInfo(), false);
                // invoker.Invoke(invoker, new object[] { this, agent, equipIndex, position, velocity, orientation, true, -1 });
            }
                
            this.ConsumeAmmo(agent, equipIndex, 1);

            agent.UpdateLastRangedAttackTimeDueToAnAttack(MBCommon.GetTotalMissionTime());
            agent.ResetAiWaitBeforeShootFactor();
        }

        private void ConsumeAmmo(Agent agent, EquipmentIndex slotIndex, short amount)
        {
            if (agent.Equipment[slotIndex].CurrentUsageItem.IsRangedWeapon)
            {
                agent.Equipment.SetConsumedAmmoOfSlot(slotIndex, amount);

                if (GameNetwork.IsServerOrRecorder)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SetWeaponAmmoData(agent.Index, slotIndex, EquipmentIndex.None, amount));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }

            }

            agent.UpdateAgentProperties();
        }
    }
}
