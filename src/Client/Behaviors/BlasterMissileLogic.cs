using HarmonyLib;
using NetworkMessages.FromServer;
using SeparatistCrisis.Entities;
using SeparatistCrisis.ObjectTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace SeparatistCrisis.Behaviors
{
    /// <summary>
    ///   This is where we handle the different firing modes logic.
    ///   Many thanks to Reznov from the 40k mod and shokuho for helping out. 
    ///   If anyone's looking at this in the future and the class has dependencies shooting out of it's arse.
    ///   You can go back however many commits and you'll find that the class is self contained and can be used for native ranged weapons.
    /// </summary>
    public class BlasterMissileLogic: MissionLogic
    {
        private MissionState? _state;
        private Dictionary<Agent, MissileAgent> _missileAgents = null!;
        private delegate void OnAgentShootMissileDelegate(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, bool isPrimaryWeaponShot, int forcedMissileIndex);
        private OnAgentShootMissileDelegate AgentShootMissile = null!;

        public override void EarlyStart()
        {
            this._state = Game.Current.GameStateManager.ActiveState as MissionState;
            this._missileAgents = new Dictionary<Agent, MissileAgent>();

            MethodInfo methodInfo = AccessTools.Method(typeof(Mission), "OnAgentShootMissile", new Type[] { typeof(Agent), typeof(EquipmentIndex), typeof(Vec3), typeof(Vec3), typeof(Mat3), typeof(bool), typeof(bool), typeof(int) });
            AgentShootMissile = (OnAgentShootMissileDelegate)methodInfo.CreateDelegate(typeof(OnAgentShootMissileDelegate), this.Mission);
        }

        // Don't forget to get it working for controllers
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            for (int i = 0; i < base.Mission.Agents.Count; i++)
            {
                Agent agent = base.Mission.Agents[i];
                if (agent != null && agent.IsActive() && agent.Health > 1f)
                {
                    // Horses etc
                    if (!agent.IsHuman)
                        continue;

                    // Sometimes, agents dont seem to have a primary weapon equipped on mission start
                    if (agent.WieldedWeapon.Item == null)
                        continue;

                    // This will make it so the _lastShot and _canFire dictionaries dont get bloated
                    if (!agent.WieldedWeapon.CurrentUsageItem.IsRangedWeapon)
                        continue;

                    // Double look up on first go around but then its just the 1 from there on
                    if (!this._missileAgents.ContainsKey(agent))
                        this._missileAgents.Add(agent, new MissileAgent(agent));

                    MissileAgent missileAgent = this._missileAgents[agent];

                    // If the agent is using a different ranged weapon.
                    if (missileAgent?.WeaponOptions?.WeaponId != agent.WieldedWeapon.Item.StringId)
                        missileAgent.WeaponOptions = RangedWeaponOptions.FindFirst(x => x.WeaponId == agent?.WieldedWeapon.Item?.StringId);

                    // If the weapon doesnt have different firing modes, then let it act as default xbow/bow
                    if (missileAgent?.WeaponOptions == null)
                        continue;

                    // InputContext.RegisterHotKeyCategory is mainly used inside Views
                    // We will probably need a view and vm for this stuff anyways, so it's worth cranking 1 out
                    // And then registering the hot key category inside there
                    if (agent.IsMainAgent && this.Mission.InputManager.IsKeyReleased(InputKey.V))
                    {
                        if (missileAgent.WeaponOptions.FiringModeFlags.Count > (missileAgent.CurrentFiringModeIndex + 1))
                            missileAgent.CurrentFiringModeIndex += 1;
                        else
                            missileAgent.CurrentFiringModeIndex = 0;

                        InformationManager.DisplayMessage(new InformationMessage($"Switched to {missileAgent.CurrentFiringMode} firing mode!"));
                    }

                    // Checks and balances baby!
                    if (missileAgent.CanFire && agent.GetCurrentActionType(1) == Agent.ActionCodeType.ReadyRanged && agent.GetCurrentActionStage(1) == Agent.ActionStage.AttackReady &&
                        agent.GetActionChannelCurrentActionWeight(1) >= 1f && Mission.Current.CurrentTime - missileAgent.LastShot > missileAgent.ShootInterval &&
                        !agent.WieldedWeapon.IsEmpty && agent.Equipment.GetAmmoAmount(agent.GetWieldedItemIndex(Agent.HandIndex.MainHand)) > 0)
                    {
                        // There is a hard crash with no exception happening that only happens every now and again
                        // I think its a race condition issue since if we run it without any breakpoints, then 25% of the time it hard crashes.
                        // But with a breakpoint here, it never crashes.

                        // We'll probably want to abstract away the shooting methods or else this could become a god class but I'll leave that for some1 else

                        // The alternative usage key (x) works on OnKeyDown instead of OnKeyUp
                        // So we'll probably need to use a different key to change firing modes
                        // Instead of holding x down to change firing modes

                        if (missileAgent.CurrentFiringMode == FiringMode.FULLAUTO)
                            this.ShootFullAuto(agent);

                        if (missileAgent.CurrentFiringMode == FiringMode.SHOTGUN)
                            this.ShootShotgun(agent);

                        // SetActionChannel works but SetAgentActionChannel doesnt
                        // Could use something like this for recoil, in the future
                        // agent.SetActionChannel(1, ActionIndexCache.act_none, true);
                    }
                }
            }
        }

        // We'll need to swap out this method for different firing modes, maybe
        private void ShootFullAuto(Agent agent)
        {
            MissileAgent missileAgent = this._missileAgents[agent];

            missileAgent.CanFire = false;
            MissionWeapon weapon = agent.WieldedWeapon;
            EquipmentIndex equipIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand); // This uses a loop

            if (equipIndex == EquipmentIndex.None)
                return;

            // This would make it so the missile's position spawns near the main hand but when we send it to OnAgentShootMissile, it looks like it overwrites our pos and uses the eye position for the missile
            MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(agent.Monster.MainHandItemBoneIndex);
            MatrixFrame globalFrame = agent.AgentVisuals.GetGlobalFrame();
            MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

            Vec3 direction = agent.LookDirection;
            Vec3 position = matrixFrame.origin;
            Mat3 orientation = Mat3.CreateMat3WithForward(direction);
            float baseSpeed = weapon.GetModifiedMissileSpeedForCurrentUsage();

            // We just add the skill level on to the speed rating. So 200 in a skill would probably shoot lightning fast missiles. Who says hard work doesn't pay off.
            SkillObject relevantSkill = WeaponComponentData.GetRelevantSkillFromWeaponClass(weapon.CurrentUsageItem.WeaponClass);
            float speed = baseSpeed + agent.Character.GetSkillValue(relevantSkill);
            Vec3 velocity = direction * speed;

            // If we want to go the custom missile route, reznov's code shows that the lower the baseSpeed, the higher the damage.
            // Mission.Current.AddCustomMissile(agent, weapon.AmmoWeapon, position, direction, orientation, baseSpeed, speed, true, null);
            
            if (this.AgentShootMissile != null && agent.Equipment[equipIndex].CurrentUsageItem.IsRangedWeapon)
            {
                // There's still the issue of the ammo being 0 and the final missile still being in the weapon slot and it gets shot once lmb is released
                // But since we're doing stuff for blasters, I'm not gonna tackle this just yet.

                this.AgentShootMissile.Invoke(agent, equipIndex, position, velocity, orientation, true, true, -1);

                this.ConsumeAmmo(agent, equipIndex, 1);
                missileAgent.LastShot = Mission.Current.CurrentTime;

                // Don't even know if these do anything yet, havent gotten around to messing with the AI
                // The AI agents do use this logic its just that there's no code to keep them aiming yet.
                agent.UpdateLastRangedAttackTimeDueToAnAttack(MBCommon.GetTotalMissionTime());
                agent.ResetAiWaitBeforeShootFactor();
            }

            missileAgent.CanFire = true;
        }

        private void ShootShotgun(Agent agent)
        {
            MissileAgent missileAgent = this._missileAgents[agent];

            missileAgent.CanFire = false;
            MissionWeapon weapon = agent.WieldedWeapon;
            EquipmentIndex equipIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand); // This uses a loop

            if (equipIndex == EquipmentIndex.None)
                return;

            // This would make it so the missile's position spawns near the main hand but when we send it to OnAgentShootMissile, it looks like it overwrites our pos and uses the eye position for the missile
            MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(agent.Monster.MainHandItemBoneIndex);
            MatrixFrame globalFrame = agent.AgentVisuals.GetGlobalFrame();
            MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

            Vec3 direction = agent.LookDirection;
            Vec3 position = matrixFrame.origin;
            Mat3 orientation = Mat3.CreateMat3WithForward(direction);
            float baseSpeed = weapon.GetModifiedMissileSpeedForCurrentUsage();

            // We just add the skill level on to the speed rating. So 200 in a skill would probably shoot lightning fast missiles. Who says hard work doesn't pay off.
            SkillObject relevantSkill = WeaponComponentData.GetRelevantSkillFromWeaponClass(weapon.CurrentUsageItem.WeaponClass);
            float speed = baseSpeed + agent.Character.GetSkillValue(relevantSkill);
            Vec3 velocity = direction * speed;

            // If we want to go the custom missile route, reznov's code shows that the lower the baseSpeed, the higher the damage.
            // Mission.Current.AddCustomMissile(agent, weapon.AmmoWeapon, position, direction, orientation, baseSpeed, speed, true, null);

            if (this.AgentShootMissile != null && agent.Equipment[equipIndex].CurrentUsageItem.IsRangedWeapon)
            {
                // There's still the issue of the ammo being 0 and the final missile still being in the weapon slot and it gets shot once lmb is released
                // But since we're doing stuff for blasters, I'm not gonna tackle this just yet.

                int numBullets = agent.Equipment.GetAmmoAmount(equipIndex);
                short spentBullets = 0;

                for (int i = 3; i > 0; i--)
                {
                    if (numBullets - i > 0) 
                    {
                        Vec3 dir = new Vec3(direction.x, direction.y, direction.z, direction.w);

                        // Need to tone down these values
                        dir.RotateAboutX(MBRandom.RandomFloatRanged(-.05f, .05f));
                        dir.RotateAboutY(MBRandom.RandomFloatRanged(-.05f, .05f));
                        dir.RotateAboutZ(MBRandom.RandomFloatRanged(-.05f, .05f));

                        Mat3 orien = Mat3.CreateMat3WithForward(dir);

                        // this.AgentShootMissile.Invoke(agent, equipIndex, position, velocity, orien, true, true, -1);
                        Mission.Current.AddCustomMissile(agent, weapon.AmmoWeapon, position, dir, orien, baseSpeed / 4, speed, true, null);
                        spentBullets++;
                    }
                }

                this.ConsumeAmmo(agent, equipIndex, spentBullets);
                missileAgent.LastShot = Mission.Current.CurrentTime;

                // Don't even know if these do anything yet, havent gotten around to messing with the AI
                // The AI agents do use this logic its just that there's no code to keep them aiming yet.
                agent.UpdateLastRangedAttackTimeDueToAnAttack(MBCommon.GetTotalMissionTime());
                agent.ResetAiWaitBeforeShootFactor();
            }

            missileAgent.CanFire = true;
        }

        private void ConsumeAmmo(Agent agent, EquipmentIndex weaponSlot, short amount)
        {
            if (agent.Equipment[weaponSlot].CurrentUsageItem.IsRangedWeapon)
            {
                // This uses a loop, only goes up to 5 but a loop is a loop, even more so when it's in another loop
                agent.Equipment.GetAmmoCountAndIndexOfType(agent.WieldedWeapon.Item.ItemType, out int _, out EquipmentIndex ammoSlot);

                // This works for multiple ammo slots of same type
                // It doesnt play the reload animation to switch over to the other ammoSlot, it just continues on
                // If u need the character to reload after using up a quiver, then u can probably do a remainder on maxAmmo
                int ammoCount = agent.Equipment[ammoSlot].Amount;

                if (ammoSlot == EquipmentIndex.None)
                    return;

                // agent.Equipment.SetConsumedAmmoOfSlot(weaponSlot, 1);
                // agent.Equipment.SetAmountOfSlot(ammoSlot, (short)(ammoCount - amount));
                // agent.SetReloadAmmoInSlot(weaponSlot, ammoSlot, (short)-1);
                agent.SetWeaponAmountInSlot(ammoSlot, (short)(ammoCount - amount), true); // This was my saving grace
                agent.Equipment[weaponSlot].ConsumeAmmo(0);

                if (GameNetwork.IsServerOrRecorder)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SetWeaponAmmoData(agent.Index, weaponSlot, ammoSlot, amount));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }

                // Don't think this is necessary
                short reloadPhase = 0;
                agent.Equipment.SetReloadPhaseOfSlot(weaponSlot, reloadPhase);
                
                // The server would need to receive the above packet first so there's most likely an issue with how we're doing this.
                if (GameNetwork.IsServerOrRecorder)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new SetWeaponReloadPhase(agent.Index, weaponSlot, reloadPhase));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }

                // Changes the arrow count visuals in the quiver when we get down to around 5 arrows left
                // We probably dont even need this if we're only using this class for blasters.
                if (agent.Equipment[weaponSlot].CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.HasString))
                    agent.AgentVisuals.UpdateQuiverMeshesWithoutAgent((int)ammoSlot, (short)(ammoCount - amount));
            }

            // We're bypassing all the callbacks but all these callbacks tend to update agent props at the end
            // Do we need it? I don't know
            agent.UpdateAgentProperties();
        }
    }
}
