using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.Agent;

namespace SeparatistCrisis.Behaviors
{
    public class LightsaberBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;


        /***********   Variables   ***********/
        internal static readonly Color StdTextColor = Color.FromUint(0x00F16D26); // Orange


        //Game Options
        bool calculateDeflectChance_Enabled = true;
        bool calculateDeflectAccuracy_Enabled = true;



        //Dev Options
        public bool debugMode = true;
        int debugOutputMode = 4;

        /*  Used to prevent looping sounds */
        bool isSwinging = false;
        bool isIdleSaber = false;



        /*     "Master" Variables  - Used for tracking various original variables from agent's and weapons  */
        string lastWeaponDescriptionID = "";
        public string masterCurrentAgentsGuardDirection = "";   //We use this to store the original guarding action because over the course of all of these method's the action changes from "defending" to "staggering"

        private float deflectHealth = 0;    //Probably can remove

        public bool masterWasGuarding = false;          //Used to store result of if Agent was in Guard Pose
        public bool masterDeflectChanceResult = false;  //Used to store result of Deflection Calculation


        public MissionWeapon masterMissile = new MissionWeapon();//This is the Missile/projectile we were hit with. ex) Arrow,Bolt,Javelin, etc. Stored here to access the copy of it over full hit logic (if agent was reloading it changes to empty)
        public MissionWeapon masterBow = new MissionWeapon();//This weapon used to SHOOT the missile we were hit with. ex) Bow/Crossbow. Stored here to access the copy of it over full hit logic (if agent was reloading it changes to empty) 

        /// <summary>Stores the Shooter's Weapon's original weapon flags. That way we can restore them after deflection finishes </summary>
        public WeaponFlags originalWeaponFlags = WeaponFlags.Consumable;

        /// <summary>We set these temporarily to the missile we were hit with, so that way blood doesnt spurt on us</summary>
        WeaponFlags masterNewFlags = WeaponFlags.Consumable
            | WeaponFlags.NoBlood
            | WeaponFlags.AmmoCanBreakOnBounceBack;


        private delegate void OnAgentShootMissileDelegate(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, bool isPrimaryWeaponShot, int forcedMissileIndex);
        private OnAgentShootMissileDelegate AgentShootMissile = null!;



        private Dictionary<Agent, SoundEvent> idleSaberSoundsAgents = new();

        LightsaberSounds ls = new LightsaberSounds();

        //-----------------------------------------------------------------------------------------------
        
        /***************************************    Harmony Methods    ***************************************/


        /// <summary> This is the beginning of where the deflecting behavior starts. Stopping it here prevents the missile from even registering a hit on the agent,
        /// which means no stagger or hit reaction. We can then trigger the deflect animation and sounds after this in OnMissileHit() and OnMissileCollisionReaction() </summary>
        [HarmonyPatch(typeof(MissionCombatMechanicsHelper))]
        [HarmonyPatch("GetAttackCollisionResults")]
        internal class GetAttackCollisionResultsPatch
        {
            private static void Postfix(in AttackInformation attackInformation, bool crushedThrough, float momentumRemaining, bool cancelDamage, ref AttackCollisionData attackCollisionData, ref CombatLogData combatLog, int speedBonus
                //,ref Mission __instance
                )
            {

                LightsaberSounds ls = new LightsaberSounds();   //Passed this to just get access to the AgentGuardCheck function, which sets the masterWasGuarding variable. Maybe can be optimized later by just moving the method.

                //if(Mission.Current.GetMissionBehavior<LightsaberBehavior>().IsWieldingLightsaber(attackInformation.VictimAgent) == true)  // May work

                if (attackInformation.VictimAgent?.IsHuman == true)
                {
                    if (attackInformation.VictimAgent?.WieldedWeapon.WeaponsCount > 0)
                    {

                        if (attackInformation.VictimAgent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "Lightsaber"
                                || attackInformation.VictimAgent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberBastardSword"
                                || attackInformation.VictimAgent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberTwoHanded")
                        {


                            //If We are looking at enemy
                            if (IsAgentLookingAtShooter(attackInformation.VictimAgent, attackCollisionData) == true)
                            {

                                //If YES
                                if (Mission.Current.GetMissionBehavior<LightsaberBehavior>().AgentGuardCheck(attackInformation.VictimAgent, "GetAttackCollisionResults") == true)  //THIS SETS masterWasGuarding
                                {
                                    
                                    //We will be triggering deflection. Set all damage to 0. This stops missile from hitting us entirely. (aka agent won't stagger)

                                    attackCollisionData.BaseMagnitude = 0;
                                    attackCollisionData.InflictedDamage = 0;


                                   
                                }
                                else
                                {
                                    //Mission.Current.GetMissionBehavior<LightsaberBehavior>().masterWasGuarding = false;
                                }
                                //Else = Failed and DONT Overwrite anything


                            }
      



                        }



                    }

                }



                //Mission.Current.GetMissionBehavior<LightsaberBehavior>().masterWasGuarding = result;
            }
        }


        /// <summary> If we passed all the "Deflect" checks, we set the missile's collision particle to empty
        /// (without this, agent could still deflect but blood would splatter on agent)</summary>
        [HarmonyPatch(typeof(Mission))]
        [HarmonyPatch("MissileHitCallback")]
        private class MissileHitCallbackPatch
        {
           
            private static void Postfix(
                ref int extraHitParticleIndex
                , ref AttackCollisionData collisionData, Vec3 missileStartingPosition, Vec3 missilePosition, Vec3 missileAngularVelocity, Vec3 movementVelocity, MatrixFrame attachGlobalFrame, MatrixFrame affectedShieldGlobalFrame, int numDamagedAgents, Agent attacker, Agent victim, GameEntity hitEntity
                //,ref int __state
                )
            {


                LightsaberSounds ls = new LightsaberSounds();

                                    //Use to track if extraHitParticleIndex is being overwritten by the method or not.
                                    //If false, then we did Not Successfully enter our Method and change the particle.
                                    //If true, We GOT into the method
                                    //Set to true after manually setting extraHitParticleIndex to -1 or a specific particle system index, and then check if it remains that value after the method runs.

                if (victim?.IsHuman == true)
                {
                    if (victim?.WieldedWeapon.WeaponsCount > 0) //if (wieldedWeaponIndex != EquipmentIndex.None)
                    {

                        if (victim.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "Lightsaber"
                                || victim.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberBastardSword"
                                || victim.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberTwoHanded")
                        {


                            if (ls.Anim_CheckAgentCurrentAction(Mission.Current.GetMissionBehavior<LightsaberBehavior>().masterCurrentAgentsGuardDirection //if our agent was guarding.
                                , "MissileHitCallbackPatch") == true)
                            {

                                 
                                extraHitParticleIndex = -1; //No Hit Particles

                                //Options if we wanted a particle to happen on collision going forward, like a smoke plume or something. (base game particles have sounds cooked in) 
                                //extraHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_burning_projectile_default_coll"); // HUGE boom lol
                                //extraHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_game_missile_metal_coll");


                                //Extra fallback in case damage still came through to this point.
                                collisionData.InflictedDamage = 0;
                                collisionData.SelfInflictedDamage = 0;
                                collisionData.DefenderStunPeriod = 0;
                                collisionData.BaseMagnitude = 0f;
                                collisionData.MovementSpeedDamageModifier = 0f;
                                collisionData.AbsorbedByArmor = 0;

                                //success = true;
                            }
                        
                        }
                     
                    }

                }


                Mission.Current.GetMissionBehavior<LightsaberBehavior>().Reset_AllMasterVariables();

            }

        }


        //-----------------------------------------------------------------------------------------------


        public override void EarlyStart()
        {
            base.EarlyStart();
            
            //Deflect Agents are esentially "shooters" borrowed from MissileBehavior.cs
            MethodInfo methodInfo = AccessTools.Method(typeof(Mission), "OnAgentShootMissile", new Type[] { typeof(Agent), typeof(EquipmentIndex), typeof(Vec3), typeof(Vec3), typeof(Mat3), typeof(bool), typeof(bool), typeof(int) });
            AgentShootMissile = (OnAgentShootMissileDelegate)methodInfo.CreateDelegate(typeof(OnAgentShootMissileDelegate), this.Mission);

        }


        /// <summary>Using Players Bow/Crossbow/Throw skill, This method will use that skill value to help calculate whether  </summary>
        /// <param name="enabled"> Is the option for this calculation enabled </param>
        /// <param name="deflectingAgent">The Agent attempting to deflect the missile</param>
        /// <param name="direction">Original direction (I think agent's facing direction)</param>
        /// <param name="relevantSkill">Skill related to the Weapon the deflecting agent was SHOT with, (and will shoot back with) aka. Bow/Crossbow/Throw </param>
        /// <returns>Vec3 - applied inaccuracy to deflection </returns>
        private Vec3 CalculateDeflectAccuracy(bool enabled, Agent deflectingAgent, Vec3 direction, SkillObject relevantSkill)
        {
            //use_calculateDeflectAccuracy

            if (enabled == true)
            {

                //the Min and Max bounds should be correlated to the deflecting agents Bow/Crossbow/throw skill (relevant skill for shooting weapon)
                /*
                 relevantSkill:   Bow
	            Agent's Skill for Deflect - Bow = 7

                Range: 0 - 250
                 */

                
                ////1. Generate the Random Chance Ranges based off of the skill value
                ////Bounds relative to the skill level being 0
                float SkillValueMax = 150; //If the users bow skill >= 150, no accuracy modifiers will be applied

                float accMin = -0.5f;       //-0.5f;  -50f
                float accMax = 0.5f;        //      50f


                float min = accMin;
                float max = accMax;

                //Adjust the Bounds relative to the users skill
                /*
                    
                    Min = AccuracyMin * ((100-((UserAccuracySkillValue/AccuracySkillMax) * 100)) /100)     
                    Max = AccuracyMax * ((100-((UserAccuracySkillValue/AccuracySkillMax) * 100)) /100)

                    
                    
                    Example) If Users Bow Skill = 12       (with SkillValueMax = 150                  
                        Min = -0.046
                        Max = 0.046

                 */

                if (deflectingAgent.Character.GetSkillValue(relevantSkill) > SkillValueMax)
                {
                    min = 0f;
                    max = 0f;
                }
                else
                {
                    min = accMin * ((100 - (((float)deflectingAgent.Character.GetSkillValue(relevantSkill) / SkillValueMax) * 100)) / 100);
                    max = accMax * ((100 - (((float)deflectingAgent.Character.GetSkillValue(relevantSkill) / SkillValueMax) * 100)) / 100);
                }


                //2 Apply the transformations
                direction.RotateAboutX(MBRandom.RandomFloatRanged(min, max));
                direction.RotateAboutY(MBRandom.RandomFloatRanged(min, max));
                direction.RotateAboutZ(MBRandom.RandomFloatRanged(min, max));

                
                //DebugMessage(dbg,dbg,debugOutputMode,debugMode);
            }



            return direction;

        }

        /// <summary>if enabled, will attempt to determine how likely agent will deflect or not based off their 1h/2h skill. 
        ///     The higher the agent's 1h/2h skill, the more likely will succeed </summary>
        /// <param name="enabled">Controlled by Master variable "calculateDeflectChance_Enabled"</param>
        /// <param name="deflectingAgent"></param>
        /// <returns>Returns if agent successfully deflected missile. (if enabled = false, will always return true)</returns>
        private bool CalculateDeflectChance(bool enabled, Agent deflectingAgent)
        {
            bool result = false;

            string dbg = "";


            if (enabled == true)
            {
                dbg = "\n\n\n     [CalculateDeflectChance] - ";

                //1. Calculate the value (using 2h skill) that the random number will have to be less than?
                int deflectChance = 0;


                SkillObject relevantSkill = deflectingAgent.WieldedWeapon.CurrentUsageItem.RelevantSkill;
                float skillLvl = deflectingAgent.Character.GetSkillValue(relevantSkill);


                // Formula =   ((currentSkillLevel / 250 ) * 100) + 25 minimum for skill min
                deflectChance = (int)((skillLvl / 250f) * 100) + 25;


                Random rnd = new Random();

                //Get random choice from list
                int r = rnd.Next(0, 101);


                if (r < deflectChance)
                {
                    //Success
                    result = true;
                }
;
                dbg += "\n     [CalculateDeflectChance] - result = " + result.ToString() + "\n\n\n"
                    ;

                DebugMessage(dbg, dbg, debugOutputMode, debugMode);
            }
            else
            {
                result = true;
            }




            if (result == false)
            {                            
                DebugMessage("[CalculateDeflectChance] - Accuracy failed.", "", debugOutputMode, debugMode);
            }


            return result;
        }



        /// <summary>Determines whether the Deflecting Agent was looking in the shooter's direction when they were hit. (Aka, if player, was the shooter visible on the players screen with default FOV) 
        /// If they were, then we can trigger the deflect behavior. If not, then we just do a normal hit with no deflect.</summary>
        /// <param name="victim">The Agent shot</param>
        /// <param name="attackCollisionData"></param>
        /// <returns>TRUE = Shooter agent was visible to the agent hit (around 50 degrees)</returns>
        public static bool IsAgentLookingAtShooter(Agent victim, AttackCollisionData attackCollisionData)
        {
            bool result = false;

            Vec3 direction = victim.LookDirection;
            Vec3 missileStartPos = attackCollisionData.MissileStartingPosition;


            Vec3 testDirectionToTarget = attackCollisionData.MissileStartingPosition - victim.Position;
            float victimAngletoShooter = Vec3.AngleBetweenTwoVectors(direction, testDirectionToTarget); //Calculates the angle between shooters position and victim look direction


            float fieldOfView = 50f; // Allowed deviation angle; aka is the enemy withing the view of my agents screen


            //IF the Victim agent is Looking at the Missile's direction when hit, THEN deflect. Otherwise normalCollision 
            if (victimAngletoShooter.ToDegrees() < fieldOfView)
            {
                result = true;
            }
            return result;
        }


        /// <summary>Heart and soul of Deflecting Feature. This method is what fires back the projectile. 
        /// This is done by creating a copy of the missile the agent was hit with, and shooting it back.</summary>
        /// <param name="deflectingAgent">The agent DOING the deflecting</param>
        /// <param name="missileSourceAgent">Who shot the victim agent</param>
        /// <param name="originalMissileWeapon">Reference the Bow/Crossbow/Javelin agent was hit with (not the ammo)</param>
        /// <param name="source">Used to track what method invoked this. Purely debugging help</param>
        public void DeflectProjectile(Agent deflectingAgent, Agent missileSourceAgent, MissionWeapon originalMissileWeapon, string source)
        {

            string dbg = "\n[DeflectProjectile()] Begin:  \n" + "Source = " + source + "\n";

            MissionWeapon weapon = masterBow; //Set Weapon to the original Bow/Crossbow

            //Set the Ammo to the copy of the missile you were shot with
            weapon.SetAmmo(masterMissile);



            EquipmentIndex equipIndex = missileSourceAgent.GetPrimaryWieldedItemIndex(); // This uses a loop


            //If we cant get the index of the shooter's weapon, exit
            if (equipIndex == EquipmentIndex.None)
            {
                return;
            }


            // This would make it so the missile's position spawns near the main hand but when we send it to OnAgentShootMissile, it looks like it overwrites our pos and uses the eye position for the missile
            MatrixFrame boneEntitialFrameWithIndex = deflectingAgent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(deflectingAgent.Monster.MainHandItemBoneIndex);
            MatrixFrame globalFrame = deflectingAgent.AgentVisuals.GetGlobalFrame();
            MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);



            //Bow (Or the shooting weapon skill)
            SkillObject relevantSkill = WeaponComponentData.GetRelevantSkillFromWeaponClass(weapon.CurrentUsageItem.WeaponClass);       //deflectingAgent.WieldedWeapon.CurrentUsageItem.RelevantSkill; // < For 2h
            Vec3 direction = deflectingAgent.LookDirection;

            //If enabled, Add on the Inaccuracy modifier to deflection.
            direction = CalculateDeflectAccuracy(calculateDeflectAccuracy_Enabled, deflectingAgent, direction, relevantSkill);


            Vec3 position = matrixFrame.origin;
            Mat3 orientation = Mat3.CreateMat3WithForward(direction);
            float baseSpeed = weapon.GetModifiedMissileSpeedForCurrentUsage();


            // We just add the skill level on to the speed rating. So 200 in a skill would probably shoot lightning fast missiles. Who says hard work doesn't pay off.            
            float speed = baseSpeed + deflectingAgent.Character.GetSkillValue(relevantSkill);
            Vec3 velocity = direction * speed;


            if (this.AgentShootMissile != null
                && weapon.CurrentUsageItem.IsRangedWeapon
                )
            {


                MissionWeapon blankweapon = new MissionWeapon();//use a new empty mission weapon for reference to compare to what we're trying to equip

                if (originalMissileWeapon.AmmoWeapon.Equals(blankweapon) == true)
                {
                    //dbg += "\nEMPTY - NO AMMO *******";
                    return;
                }


                //Temporarily Equip the Bow/Crossbow to "Deflect" back the missile shot with
                deflectingAgent.EquipWeaponWithNewEntity(EquipmentIndex.ExtraWeaponSlot
                    , ref weapon
                    );


                //FINAL VERSION *************************************************** Shooot the Missile
                this.AgentShootMissile.Invoke(deflectingAgent, EquipmentIndex.ExtraWeaponSlot, position, velocity, orientation, true, true, -1);    //WORKING**************************************************************************


                if (originalMissileWeapon.AmmoWeapon.CurrentUsageItem is null
                    && weapon.AmmoWeapon.CurrentUsageItem is null
                    )
                {
                    dbg += "\n  [DeflectProjectile] - invoking AgentShootMissile FAILED, originalMissileWeapon.AmmoWeapon.CurrentUsageItem is null ";
                    DebugMessage(dbg, dbg, debugOutputMode, debugMode);
                }



                //Change the Agent's Animation
                ls.ChangeGuardDirection(deflectingAgent, masterCurrentAgentsGuardDirection);          
                dbg = "\n[DeflectProjectile] - New GetCurrentActionDirection = " + deflectingAgent.GetDefendMovementFlag().ToString();



                //Play Block Sound
                Get_LightsaberSounds_CheckAgentActions(deflectingAgent, "Deflect");





                //Remove temporary shooting weapon from spare slot
                deflectingAgent.RemoveEquippedWeapon(EquipmentIndex.ExtraWeaponSlot); 


                // Don't even know if these do anything yet, havent gotten around to messing with the AI
                // The AI agents do use this logic its just that there's no code to keep them aiming yet.
                deflectingAgent.UpdateLastRangedAttackTimeDueToAnAttack(MBCommon.GetTotalMissionTime());
                deflectingAgent.ResetAiWaitBeforeShootFactor();
            }
            else
            {
                dbg += "\n[DeflectMethod] - DEFLECT FAILED TO EXECUTE";
            }

            //missileAgent.CanFire = true;
            DebugMessage(dbg, dbg, debugOutputMode, debugMode);
        }


        /// <summary>(Maybe can be deprecated) 
        /// Executes towards the end of the full Missile Hit logic.
        /// Sets damage to 0 if passed all deflect checks. 
        /// ALSO resets the shooting agents Missile flags back to their original (we changed them before to stop blood from spurting everywhere)</summary>
        /// <param name="attacker"></param>
        /// <param name="victim"></param>
        /// <param name="isCanceled"></param>
        /// <param name="collisionData"></param>
        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {       

            string dbg = "";

            AttackCollisionData atd = new AttackCollisionData();


            if (victim is not null && victim.IsHuman)
            {

                MissionWeapon mw = new MissionWeapon();
                mw = victim.WieldedWeapon;

                if (mw.Item != null)
                {

                    ///If the victim is wielding a lightsaber
                    if (IsWieldingLightsaber(victim) == true
                        && IsUsingShield(victim) == false
                        )
                    {
                                          
                        if (collisionData.IsMissile == true)///If the hit was from a missile
                        {


                            if (AgentGuardCheck(victim, "OnMissileHit") == true)  //if (victim.CurrentGuardMode != GuardMode.None)
                            {
                                dbg += "\n[OnMissileHit] - " + victim.Name + " is in Guard Mode: " + victim.GetCurrentActionType(1).ToString() + " [3]";


                                MissionWeapon originalMissile = attacker.WieldedWeapon.AmmoWeapon;  //KEY passed to DeflectProjectile()

                                deflectHealth = collisionData.InflictedDamage;

                                // Set Damage to 0
                                collisionData.InflictedDamage = 0;
                                collisionData.SelfInflictedDamage = 0;
                                collisionData.DefenderStunPeriod = 0;                               
                                collisionData.AbsorbedByArmor = 0;


                                Blow b = new Blow();//Created temporary Blow w 0 damage

                                //Tried to Overwrite the data in atd
                                b = GenerateNewDummyMeleeBlow(victim, "Melee", null, atd);


                                //This stuff is mostly to try and catch if for some reason damage has passed to this point when it shouldnt have
                                // Set Damage to 0  
                                atd.InflictedDamage = 0;
                                atd.SelfInflictedDamage = 0;

                                atd.DefenderStunPeriod = 0;
                                atd.AbsorbedByArmor = 0;


                                isCanceled = true;  //CANCEL the damage (aka set it to blocked              

                                //Try to undo the damage (may not be needed)
                                victim.Health += deflectHealth;

                                dbg += "\nVictim Health Post Deflect = " + victim.Health.ToString();

                            }

                            dbg += "\n\n[OnMissileHit]  - FINAL damageInflicted = " + atd.InflictedDamage.ToString() + " || MissileTotalDamage = " + atd.MissileTotalDamage.ToString() + "\n\n";

                        }

                        DebugMessage(dbg, dbg, debugOutputMode, debugMode);

                    }
                   
                }
                
            }
            
           



            if (victim is not null &&
                victim.IsHuman == true &&

                IsWieldingLightsaber(victim) == true
                && IsUsingShield(victim) == false
                && AgentGuardCheck(victim, "DebugOnMissileHit") == true
                )
            {

                dbg = "";

                base.OnMissileHit(attacker, victim, isCanceled, atd);       //Pass the new AttackCollisionData with 0 damage to the base method

                try
                {
                    masterMissile.Item.PrimaryWeapon.WeaponFlags = originalWeaponFlags;                     
                    dbg += "\n        [OnMissileHit] - reset mastermissile flags to originalWeaponFlags";
                }
                catch (Exception e)
                {
                    dbg += "\n        [OnMissileHit] - <ERROR>      could NOT set masterMissile.Item.PrimaryWeapon.WeaponFlags = originalWeaponFlags    <ERROR> " + "" + "\n         [OnMissileHitERROR] - msg = " + e.Message.ToString();
                }

                DebugMessage(dbg, dbg, debugOutputMode, debugMode);
               
            }
            else
            {
                base.OnMissileHit(attacker, victim, isCanceled, collisionData); //Fall back to base method.
            }

        }


        /// <summary>[2] - Occurs on 2nd part of Execution for a Missile hitting Saber Wielder. Occurs AFTER OnAgentHit() </summary>
        /// <param name="attacker"></param>
        /// <param name="victim"></param>
        /// <param name="realHitEntity"></param>
        /// <param name="b"></param>
        /// <param name="collisionData"></param>
        /// <param name="attackerWeapon"></param>
        public override void OnRegisterBlow(Agent attacker, Agent victim, WeakGameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {

            string dbg = "";


            AttackCollisionData atd = collisionData;//Copy of the original collision data

            if (victim is not null && victim.IsHuman)
            {

                MissionWeapon mw = new MissionWeapon();
                mw = victim.WieldedWeapon;


                if (mw.Item != null)
                {
                  
                    if (IsWieldingLightsaber(victim) == true
                        && IsUsingShield(victim) == false
                        )
                    {

                        if (collisionData.IsMissile == true)
                        {
                       

                            if (AgentGuardCheck(victim, "OnRegisterBlow") == true)
                            {


                                //Probably can remove
                                //  Brought Over from OnAgentHit()      ===========================================================================================


                                //  NEED THIS TO STOP EXECUTIONS AT ALL STAGES if FALSE (masterDeflectChanceResult set in GuardCheck from different source method)
                                if (masterDeflectChanceResult == false)
                                {                                   
                                    DebugMessage("DEFLECT CHANCE FAILED (OnRegisterBlow)", dbg, debugOutputMode, debugMode);
                                    return;
                                }


                                //If MasterVariables are Null UpdateThem
                                AssignMasterBowAndMissile(attacker, attackerWeapon);


                                //From Mission.cs
                                b.BaseMagnitude = 0f;
                                b.MovementSpeedDamageModifier = 0f;
                                b.InflictedDamage = 0;
                                b.SelfInflictedDamage = 0;
                                b.AbsorbedByArmor = 0f;



                                b.VictimBodyPart = BoneBodyPartType.None;
                                b.InflictedDamage = 0;
                                b.DamagedPercentage = 0;
                                b.DefenderStunPeriod = 0;


                                b.BlowFlag = BlowFlags.ShrugOff;


                                collisionData.InflictedDamage = 0;
                                collisionData.SelfInflictedDamage = 0;
                                collisionData.DefenderStunPeriod = 0;
                                collisionData.BaseMagnitude = 0f;
                                collisionData.MovementSpeedDamageModifier = 0f;
                                collisionData.AbsorbedByArmor = 0;


                                //Set Blows flags to the master flags instead of it's original
                                b.WeaponRecord.WeaponFlags = masterNewFlags;
                                
                                atd = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, true, true, true, false, false, false, false
                                    , CombatCollisionResult.Parried | CombatCollisionResult.Blocked, collisionData.AffectorWeaponSlotOrMissileIndex, collisionData.StrikeType, collisionData.DamageType
                                    , collisionData.CollisionBoneIndex
                                    , collisionData.VictimHitBodyPart
                                    , collisionData.AttackBoneIndex, collisionData.AttackDirection, collisionData.PhysicsMaterialIndex, CombatHitResultFlags.NormalHit
                                    , collisionData.AttackProgress, collisionData.CollisionDistanceOnWeapon, collisionData.AttackerStunPeriod, 0, 0, collisionData.MissileStartingBaseSpeed
                                    , collisionData.ChargeVelocity, collisionData.FallSpeed, collisionData.WeaponRotUp, collisionData.WeaponBlowDir, collisionData.CollisionGlobalPosition
                                    , collisionData.MissileVelocity, collisionData.MissileStartingPosition, collisionData.VictimAgentCurVelocity, victim.Position.ToWorldPosition().GetGroundVec3()
                                    );


                                atd.InflictedDamage = 0;
                                atd.SelfInflictedDamage = 0;
                                atd.DefenderStunPeriod = 0;
                                atd.BaseMagnitude = 0f;
                                atd.MovementSpeedDamageModifier = 0f;
                                atd.AbsorbedByArmor = 0;


                                //Dummy blow passing in the new collision atd
                                b = GenerateNewDummyMeleeBlow(victim, "Melee", null, atd);      //MAY NOT NEED THESE TWO
                                b.WeaponRecord.WeaponFlags = masterNewFlags;                              



                                base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);//   This approach DID allow deflecting but damage still coming through
                                DebugMessage(dbg, dbg, debugOutputMode, debugMode);


                            }
                            else
                            {                              
                                base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);
                            }

                        }
                        else
                        {                           
                            base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);
                        }

                    }
                    else
                    {
                        base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);
                    }

                }
                else
                {                    
                    base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);
                }

            }
            else
            {                
                base.OnRegisterBlow(attacker, victim, realHitEntity, b, ref atd, attackerWeapon);
            }

        }


        /// <summary>Checks whether this agent was doing a guard action while wielding a lightsaber. 
        /// if source = GetAttackCollisionResults: We determine deflect chance (whether deflect actually triggers via CalculateDeflectChance)
        /// and if success, set masterWasGuarding to TRUE</summary>
        /// <param name="agent"></param>
        /// <param name="source"></param>
        public bool AgentGuardCheck(Agent agent, string source)
        {
            bool result = false;//Default

            
            if (source == "GetAttackCollisionResults")
            {
                string dbg = "";

                if (ls.Anim_CheckAgentCurrentAction(agent.GetCurrentAction(1).GetName().ToString(), "AgentGuardCheck(GetAttackCollisionResults)") == true)
                {
                    

                    //DETERMINE IF DEFLECT OCCURS Chance To Even Deflect
                    masterDeflectChanceResult = CalculateDeflectChance(calculateDeflectChance_Enabled, agent);


                    //If We calculated deflect chance and returned false, set masterwas guarding = false
                    if (masterDeflectChanceResult == false)
                    {
                        dbg += "\n\n    [AgentGuardCheck]  - DEFLECT CHANCE FAILED (GetAttackCollisionResults)  \n\n";
                        masterWasGuarding = false;

                    }
                    else
                    {

                        // WE DETERMINED THAT THE AGENT WAS GUARDING, AND passed the deflect chance ALL TRUE;
                        masterWasGuarding = true;
                        masterCurrentAgentsGuardDirection = agent.GetCurrentAction(1).GetName().ToString();
                        dbg += "\n\n    [AgentGuardCheck]  - DEFLECT CHANCE SUCCEEDED (GetAttackCollisionResults)  \n\n";
                    }

                }
                else
                {
                    //We WERENT Doing a Defend Action so Master was Guarding = FALSE
                    masterWasGuarding = false;
                    dbg += "\n\n    [AgentGuardCheck]  - Agent was NOT Doing Defend Action (GetAttackCollisionResults)  \n\n";
                }
                DebugMessage(dbg, dbg, debugOutputMode, debugMode);
            }
            else if (source == "OnMeleeHit")
            {
                //If Meleee defending set the master guard/direction variables 
                //Version 3 (set MASTER boolean check to see if this agent was guarding
                masterWasGuarding = ls.Anim_CheckAgentCurrentAction(agent.GetCurrentAction(1).GetName().ToString(), "AgentGuardCheck(OnMeleeHit)");   
                masterCurrentAgentsGuardDirection = agent.GetCurrentAction(1).GetName().ToString();

            }


            result = masterWasGuarding;         
            return result;
        }



        ///<summary>[3] - Occurs on 3rd part of Execution for a Missile hitting Saber Wielder. Executes AFTER OnAgentHit() => On Register Blow()
        ///We're playing the Block sound here (could maybe be moved)</summary>
        /// <param name="collisionReaction"></param>
        /// <param name="attackerAgent">shooting agent</param>
        /// <param name="attachedAgent">agent hit with missile</param>
        /// <param name="attachedBoneIndex">Where the agent was shot in the body</param>
        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
        {
            
            if (attachedAgent is not null && attachedAgent.IsHuman)
            {

                MissionWeapon mw = new MissionWeapon();
                mw = attachedAgent.WieldedWeapon;

                if (mw.Item != null)
                {

                    ///If the victim is wielding a lightsaber and NOT wielding a shield
                    if (IsWieldingLightsaber(attachedAgent) == true
                        && IsUsingShield(attachedAgent) == false
                        )
                    {
                        if (AgentGuardCheck(attachedAgent, "OnMissileCollisionReaction") == true)  //if (victim.CurrentGuardMode != GuardMode.None)
                        {

                            string dbg = "";
                            // Try to set the Collision reaction to Bounce the Projectile
                            collisionReaction = Mission.MissileCollisionReaction.BounceBack;


                            /*  Try to Deflect the Projectile if missile collided and we passed all the checks*/                         
                            try
                            {
                                DeflectProjectile(attachedAgent, attackerAgent, masterBow, "OnMissileCollisionReaction()");

                            }
                            catch (Exception e)
                            {

                                dbg += "     [OnMissileCollisionReaction] - <ERROR> FAILED TO FIRE THE DEFLECTED PROJECTILE  <ERROR>";
                                dbg += "        StackTrace: " + e.StackTrace;
                                dbg += "        Error Msg:  " + e.Message.ToString();

                            }


                            try
                            {
                                dbg += "\n[OnMissileCollisionReaction] - Try to set the Bone Index to -1";                               
                                attachedBoneIndex = -1;                              
                            }
                            catch (Exception e)
                            {
                            }

                            //Play Block Sound
                            Get_LightsaberSounds_CheckAgentActions(attachedAgent, "Deflect");

                            DebugMessage(dbg, dbg, debugOutputMode, debugMode);


                        }
                    }

                }

            }

            base.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);

        }


        /// <summary>If the selected agent exists in the SaberWielderList(used to track IdleHum), return true </summary>
        /// <param name="agent"></param>
        public bool AgentExistsInSaberList(Agent agent)
        {
            bool result = false;

            try
            {
                result = idleSaberSoundsAgents.ContainsKey(agent);
            }
            catch (Exception e)
            {
                result = false;

            }
            return result;
        }

        /// <summary>This runs on MissionTick. Runs various checks to essentially handle the Idle Hum sound for saber wielders and not have it play repeatidly for one agent and disable it if sheathed</summary>
        public void SaberIdleHumHandler()
        {
            //Makes local copy of the dictionary so we can loop through it without worrying about modifying the original during runtime
            Dictionary<Agent, SoundEvent> testDictionary = new Dictionary<Agent, SoundEvent>(idleSaberSoundsAgents);

            List<Agent> agentList = new List<Agent>();
            agentList = idleSaberSoundsAgents.Keys.ToList();

            //Loop through a
            foreach (var s in testDictionary)
            {

                if (IsWieldingLightsaber(s.Key) == true)
                {
                    //Update Physical Position of Sound Player
                    SoundEvent_UpdatePosition(s.Key, idleSaberSoundsAgents[s.Key]);
                }

                if (IsWieldingLightsaber(s.Key) == true
                    && s.Value.IsValid == false)    //If wielding a Lightsaber but the soundplayer's valid value = false, re Add a sound event for the hum for that agent
                {
                    //if the value went to null, then try to 
                    idleSaberSoundsAgents[s.Key] = SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString(ls.saberHum), Mission.Scene);
                }


                //If Player is WieldingLightsaber/SoundEvent is valid/Souund is NOT playing already
                if (IsWieldingLightsaber(s.Key) == true
                        && idleSaberSoundsAgents[s.Key].IsValid == true
                        && (idleSaberSoundsAgents[s.Key].IsPlaying() == false
                                || idleSaberSoundsAgents[s.Key].IsPaused() == true)                     
                    )
                {

                    PlaySoundFromList(ls.saberHum, s.Key, idleSaberSoundsAgents[s.Key]);
                }
                else if (IsWieldingLightsaber(s.Key) == true
                    && s.Value.IsPlaying() == true
                    )
                {
                    //IsWieldingLightsaber(s.Value) == true     //Dont use for now
                }
                else
                {
                    //Release the Idle Hum
                    idleSaberSoundsAgents[s.Key].Stop();
                }
            }
        }


        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            string dbg = "";


            // hook into this action and add my logic
            agent.OnAgentWieldedItemChange += () =>
            {

                Get_LightsaberSounds_CheckAgentActions(agent, "Switch"); //This will run logic on weather to play saber ignite sound or not


                if (IsWieldingLightsaber(agent) == true)
                {
                    //IF Agent was in list already, clear them out and re add them
                    if (idleSaberSoundsAgents.ContainsKey(agent) == true)
                    {
                        idleSaberSoundsAgents[agent].Stop();
                        idleSaberSoundsAgents[agent].Release();

                        idleSaberSoundsAgents.Remove(agent);
                    }
                    
                    //Add them to SaberIdleList if not in it already
                    idleSaberSoundsAgents.Add(agent,
                    SoundEvent.CreateEvent(SoundEvent.GetEventIdFromString(ls.saberHum), Mission.Scene)
                    );

                    dbg += "    [WeaponSwitch] - AgentWieldingSaber. Adding to list";

                }
                else
                {
                    //Remove the Agent from the saberWielding list
                    if (idleSaberSoundsAgents.ContainsKey(agent) == true)
                    {

                        idleSaberSoundsAgents[agent].Stop();
                        idleSaberSoundsAgents[agent].Release();

                        idleSaberSoundsAgents.Remove(agent);
                        dbg += "\n  [WeaponSwitch] - removing agent from list ";
                    }

                }

                //Storing weapon description for previously wielded weapon in this variable.
                try
                {

                    if (agent?.WieldedWeapon.WeaponsCount > 0)
                    {
                        if (agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId != null)
                        {
                            lastWeaponDescriptionID = agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId.ToString();
                        }
                        else
                        {
                            lastWeaponDescriptionID = "";
                        }                       
                    }
                    else
                    {
                        lastWeaponDescriptionID = ""; //If no weapons equipped, set to blank
                    }

                }
                catch(Exception ex)
                {                   
                }

                DebugMessage(dbg, dbg, debugOutputMode, debugMode);

            };

        }

        public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            
            if (victim is not null && victim.IsHuman
                && IsUsingShield(victim) == false
                && IsWieldingLightsaber(victim)
                && AgentGuardCheck(victim, "OnMeleeHit") == true
                )
            {
                Get_LightsaberSounds_CheckAgentActions(victim, "Block");
            }

            else if (attacker is not null && attacker.IsHuman
                && IsWieldingLightsaber(attacker)
                )
            {

                if (collisionData.IsAlternativeAttack == false) //NOT a bash
                {
                    Get_LightsaberSounds_CheckAgentActions(attacker, "Melee");
                }

            }

            base.OnMeleeHit(attacker, victim, isCanceled, collisionData);
        }


        ///<summary>Primary Sound Handler method. Check if the Agent is Wielding a Lightsaber, and then play sounds </summary>
        /// <param name="agent">Target agent to play the sound at their position</param>
        public void Get_LightsaberSounds_CheckAgentActions(Agent agent, string source)
        {

            string dbg = "";

            if (agent is not null && agent.IsHuman)
            {
                if (agent.WieldedWeapon.WeaponsCount > 0) 
                {

                    if (agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "Lightsaber"
                            || agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberBastardSword"
                            || agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberTwoHanded")
                    {

                        if (source == "Switch")
                        {
                            PlaySound(ls.saberOn, Mission, agent);
                        }
                        if (source == "Melee")
                        {
                            PlaySound(ls.RandomSaberSound(ls.saberHitList), Mission, agent);
                        }
                        if (source == "Swing")
                        {

                            if (isSwinging == false)
                            {
                                isSwinging = true;//Used to prevent looping
                                PlaySoundWithDelay(ls.RandomSaberSound(ls.saberSwingList), Mission, agent, 1, isSwinging);
                            }
                            else
                            {
                                //DO NOT PLAY ANYTHING
                            }

                        }
                        if (source == "Idle")
                        {
                            if (isIdleSaber == false)
                            {
                                isIdleSaber = true;
                                PlaySoundWithDelay(ls.saberHum, Mission, agent, 10, isIdleSaber);
                            }

                        }
                        if (source == "Block")  //Melee Version
                        {
                            PlaySound(ls.RandomSaberSound(ls.saberHitList), Mission, agent);
                        }

                        if (source == "Deflect") //Projectile Block
                        {
                            PlaySound(ls.RandomSaberSound(ls.saberDeflectList), Mission, agent);
                        }                    

                    }
                    else
                    {
                        //dbg = "Actor equipped a non Lightsaber";
                    }

                }
                else
                {


                    int rightHandSlotIndex;
                    int rightHandUsageIndex;
                    int leftHandSlotIndex;
                    int leftHandUsageIndex;

                    agent.GetOldWieldedItemInfo(out rightHandSlotIndex, out rightHandUsageIndex, out leftHandSlotIndex, out leftHandUsageIndex);

                    //Determine which weapon was previously equipped using the rightHandSlotIndex                  
                    //If Saber previously equipped and CURRENTLY nothing equipped, play saber off sound
                    if (source == "Switch" && agent.WieldedWeapon.WeaponsCount == 0 && rightHandSlotIndex != -1)   //rightHandSlotIndex == -1              
                    {
                        EquipmentIndex ei = EquipmentIndex.None;
                        ei = (EquipmentIndex)rightHandSlotIndex; //Convert int to

                        string lastWeaponDesc = "";

                        try
                        {
                            lastWeaponDesc = agent.Equipment[rightHandSlotIndex].Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId;

                            DebugMessage("     [SaberOffCheck] - lastWeaponDesc = " + lastWeaponDesc, "", debugOutputMode, debugMode);
                        }
                        catch (Exception ex)
                        {
                            DebugMessage("     [SaberOffCheck] - lastWeaponDesc = <ERROR>", "", debugOutputMode, debugMode);
                            lastWeaponDesc = lastWeaponDescriptionID;
                        }


                      
                        if (lastWeaponDesc == "Lightsaber" || lastWeaponDesc == "LightsaberBastardSword" || lastWeaponDesc == "LightsaberTwoHanded")
                        {

                            PlaySound(ls.saberOff, Mission, agent);
                            dbg = "Agent sheathed lightsaber";
                            dbg += ":  " + agent.Name + "  |  lastWeaponDescriptionID = " + lastWeaponDescriptionID;
                            DebugMessage(dbg, dbg, 3, debugMode);

                        }

                    }
                    else
                    {
                        //dbg = "Actor did not have an item equipped";
                    }

                }

            }

        }





        /// <summary>Handles Agent Actions used in Mission Tick</summary>        
        /// <param name="agent"></param>
        public void GetAgentAction_OnTick(Agent agent)
        {
            if (agent.GetCurrentActionType(1) == ActionCodeType.ReleaseMelee)
            {
                Get_LightsaberSounds_CheckAgentActions(agent, "Swing"); //Saber Swing Sound

            }
           
            SaberIdleHumHandler();

        }


        public Blow GenerateNewDummyMeleeBlow(Agent victim, string meleeOrMissile, MissionWeapon? missileWeapon, AttackCollisionData attackCollisionData)
        {

            Blow blow = new Blow(victim.Index);

            if (meleeOrMissile == "Melee"
                || missileWeapon == null
                )
            {

                
                blow.DamageType = DamageTypes.Blunt;
                blow.BoneIndex = victim.Monster.HeadLookDirectionBoneIndex; //Original
                blow.BaseMagnitude = 0;// 10f;
                blow.GlobalPosition = victim.Position;
                blow.GlobalPosition.z += victim.GetEyeGlobalHeight();
                blow.DamagedPercentage = 0f;// 1f;
                blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                blow.SwingDirection = victim.LookDirection;
                blow.Direction = blow.SwingDirection;
                blow.InflictedDamage = 0;
                blow.DamageCalculated = true;
                sbyte mainHandItemBoneIndex = victim.Monster.MainHandItemBoneIndex;


                blow.WeaponRecord.WeaponFlags = masterNewFlags;   //may crash


                blow.VictimBodyPart = BoneBodyPartType.None;
                blow.BoneIndex = victim.Monster.MainHandItemBoneIndex;


                AttackCollisionData collisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(_attackBlockedWithShield: true, _correctSideShieldBlock: true, _isAlternativeAttack: false
                , _isColliderAgent: true, _collidedWithShieldOnBack: false, _isMissile: true, _isMissileBlockedWithWeapon: true, _missileHasPhysics: true, _entityExists: false, _thrustTipHit: false
                , _missileGoneUnderWater: false, _missileGoneOutOfBorder: false
                , CombatCollisionResult.None        //Original was Strike Agent
                , -1, 0, 2
                , -1 
                , blow.VictimBodyPart//      BoneBodyPartType.None       
                , mainHandItemBoneIndex
                , Agent.UsageDirection.AttackLeft, -1
                , CombatHitResultFlags.NormalHit
                , 0.5f      //Attack Progress
                , 1f        //CollisionDistanceOn Weapon?
                , 0f        //Attacker Stun
                , 0f        //Defender Stun
                , 0f        //MissileTOTAL DAMAGE***********************
                , 0f        //Missile Initial Speed
                , 0f        //Charge Velocity
                , 0f        //Fall Speed
                , Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, victim.Velocity, Vec3.Up);

                attackCollisionData = collisionData;    //update the input variable

                if (victim.Controller == AgentControllerType.AI)
                {
                    Vec3 acceleration = new Vec3(0f, 0f, -20f);
                    victim.AddAcceleration(in acceleration);
                }

            }
            
            //DebugMessage(dbg, dbg, debugOutputMode, debugMode);

            return blow;
        }


        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            //Check whether Agent's action is a melee swing and then call lightsaber check
            if (Mission.MainAgent is not null)
            {
                GetAgentAction_OnTick(Mission.MainAgent); //Probably need to change for Not Main Agent.
            }

        }



        /// <summary>This result if true is telling you that 
        ///     a) Agent IS wielding a lightsaber
        ///     b) Agent also is NOT using a shield</summary>
        /// <param name="agent"></param>
        /// <returns>True = Agent has lightsaber equipped in hand</returns>
        public bool IsWieldingLightsaber(Agent agent)
        {
            bool result = false; 

            try
            {
                if (agent?.IsHuman == true)
                {
                    if (agent?.WieldedWeapon != null &&
                        agent?.WieldedWeapon.WeaponsCount > 0)
                    {

                        if (agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "Lightsaber"
                                || agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberBastardSword"
                                || agent.WieldedWeapon.Item.WeaponComponent.PrimaryWeapon.WeaponDescriptionId == "LightsaberTwoHanded")
                        {
                            result = true;  //they ARE wielding a lightsaber

                        }
                        else
                        {
                            result = false;  //they are NOT wielding a lightsaber
                        }

                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
                System.Diagnostics.Debug.WriteLine("    [IsWieldingLightsaber] - ERROR: \n   " + error);
                result = false;
            }

            return result;
        }

        /// <summary>Is Agent actively wielding a shield?</summary>
        /// <param name="agent"></param>
        public bool IsUsingShield(Agent agent)
        {
            bool result = false; 

            if (agent?.IsHuman == true)
            {

                if (agent?.WieldedOffhandWeapon.WeaponsCount > 0) 
                {
                    try
                    {

                        if (//agent?.WieldedOffhandWeapon.WeaponsCount > 0 &&
                            agent.WieldedOffhandWeapon.IsShield() == true)
                        {                       
                            result = true; //Agent IS wielding a shield in thier offhand

                        }
                        else
                        {
                            result = false; //Agent is NOT wielding a shield in offhand
                        }


                    }
                    catch (Exception e)
                    {
                        result = false; //If errors for some reason, return false
                    }

                }
            }

            return result;
        }


        /// <summary> Save the Attacking agent's Bow/Crossbow/Javelin and Ammo as copies to "master variables" </summary>
        /// <param name="affectorAgent">Agent shot</param>
        /// <param name="affectorWeapon">Bow/Crossbow/Javelin</param>
        public void AssignMasterBowAndMissile(Agent affectorAgent, MissionWeapon affectorWeapon)
        {

            MissionWeapon localMissile = new MissionWeapon();
            localMissile = affectorWeapon;

            /*      ORIGINALS       */
            masterMissile = new MissionWeapon();
            masterMissile = localMissile;  

            //Save the original Missiles Flags here to restore at end
            originalWeaponFlags = masterMissile.Item.PrimaryWeapon.WeaponFlags;


            //Set the master missile's flags temporarily (will be reverted in OnMissileHit())
            masterMissile.Item.PrimaryWeapon.WeaponFlags = masterNewFlags;


            //Reset the Master bow and copy the weapon the agent was shot with
            masterBow = new MissionWeapon();
            masterBow = affectorAgent.WieldedWeapon;

            DebugMessage("\n       [AssignMasterBowAndMissile] - Assigned Master Bow and Missile", "", debugOutputMode, debugMode);
        }


        public void Reset_AllMasterVariables()
        {
            masterWasGuarding = false;//Reset the Master variable 


            //UPDATING NOW AT END OF HARMONY MissileCollisionReaction
            masterCurrentAgentsGuardDirection = ""; //Reset Stored Animation after all executions 


            originalWeaponFlags = WeaponFlags.Consumable;


            masterBow = new MissionWeapon();        //BE CAREFUL OF THIS ERASING THE ORIGINAL
            masterMissile = new MissionWeapon();
        }



        /**********************     Sound Methods     **********************/
        public void SoundEvent_UpdatePosition(Agent targetAgent, SoundEvent soundEvent)
        {
            soundEvent.SetPosition(targetAgent.Position);
        }

        public void PlaySoundFromList(string soundID, Agent targetAgent, SoundEvent soundEvent)
        {
            soundEvent.SetPosition(targetAgent.Position);
            soundEvent.Play();

            DelayedStop(soundEvent);

        }

        public void PlaySound(string soundID, Mission m, Agent targetAgent)
        {
            int soundIndex = SoundEvent.GetEventIdFromString(soundID); //to avoid string operations in runtime soundIndex can be cached.
            SoundEvent eventRef = SoundEvent.CreateEvent(soundIndex, m.Scene); //get a reference to sound and update parameters later.

            eventRef.SetPosition(targetAgent.Position);
            eventRef.Play();

            DelayedStop(eventRef);

        }

        public void PlaySoundWithDelay(string soundID, Mission m, Agent targetAgent, int delaySeconds, bool saberState)
        {
            int soundIndex = SoundEvent.GetEventIdFromString(soundID); //to avoid string operations in runtime soundIndex can be cached.

            SoundEvent eventRef = SoundEvent.CreateEvent(soundIndex, m.Scene); //get a reference to sound and update parameters later.
            eventRef.SetPosition(targetAgent.Position);
            eventRef.Play();

            DelayedStop2(delaySeconds, saberState);

        }

        private async void DelayedStop(SoundEvent eventRef)//May need to change timespan from 10 seconds    *****
        {
            TimeSpan ts = new TimeSpan(0, 0, 10);

            await Task.Delay(ts);//soundDuration
            eventRef.Stop();
        }

        private async void DelayedStop2(int sec, bool saberStateTest)
        {
            TimeSpan ts = new TimeSpan(0, 0, sec);
            await Task.Delay(ts);//soundDuration

            saberStateTest = false;
            isSwinging = false;

        }

        private async void DelayedStop3(int mSec) //Millisecond version
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, mSec);
            await Task.Delay(ts);//soundDuration
        }


        /// <summary>Just a Debug Message handler that helps swap debug modes and turn them off and on across all the SaberBehaviors module</summary>
        /// <param name="msg">Actual Message to write to debug log/console (depending on output mode)</param>
        /// <param name="head">If Msgbox,this is the header for the popup</param>
        /// <param name="outputMode">LightsaberBehaviors.debugModeOutput [Options: 0-4] - if DebugMode is enabled, this controls how the debug messages will output.</param>
        /// <param name="debug">LightsaberBehaviors.debugMode - If true, Debug messages will be enabled</param>
        public void DebugMessage(string msg, string head, int outputMode, bool debug)
        {

            if (debugMode == true)
            {
                if (outputMode == 0) // Debug Log only
                {
                    System.Diagnostics.Debug.WriteLine(msg);
                }
                else if (outputMode == 1) // Message Box Only
                {
                    TaleWorlds.Library.Debug.ShowMessageBox(msg, head, 1);
                }
                else if (outputMode == 2) // Message Box and Debug Log
                {
                    TaleWorlds.Library.Debug.ShowMessageBox(msg, head, 1);
                    System.Diagnostics.Debug.WriteLine(msg);
                }
                else if (outputMode == 3) // In-Game Message and Debug Log
                {
                    InformationManager.DisplayMessage(new InformationMessage(msg, StdTextColor));
                    System.Diagnostics.Debug.WriteLine(msg);
                }
                else if (outputMode == 4) //TaleWorld Debug Print
                {
                    Debug.Print(msg);
                }
            }
        }

    }



    public class LightsaberSounds
    {
        internal static readonly Color StdTextColor = Color.FromUint(0x00F16D26); // Orange

        public string saberOn = "sabersounds/ltsaberon01";

        public string saberOff = "sabersounds/ltsaberoff01";

        public string saberHum = "sabersounds/ltsaberhum01";//"sabersounds/lightsaberpulse"; //ambient lightsaber on

        public List<BoneBodyPartType> BoneRegionUp = new List<BoneBodyPartType>()
        {
            BoneBodyPartType.Head,
            BoneBodyPartType.Neck
        };
        public List<BoneBodyPartType> BoneRegionCenter = new List<BoneBodyPartType>()
        {
            BoneBodyPartType.Chest,
            BoneBodyPartType.Abdomen,
            BoneBodyPartType.ShoulderLeft,
            BoneBodyPartType.ShoulderRight
        };

        public List<string> saberActionIndexCacheStrings = new List<string>()
        {
            
            ////  Non Left Parry Actions
            "act_defend_up_1h_parry_light",
            "act_defend_forward_1h_parry_light",
            "act_defend_right_1h_parry_light",
            "act_defend_left_1h_parry_light",

            "act_defend_up_2h_parry_light",
            "act_defend_forward_2h_parry_light",
            "act_defend_right_2h_parry_light",
            "act_defend_left_2h_parry_light",


            //All the Left PARRY Actions
            "act_defend_up_1h_parry_light_left_stance",
            "act_defend_forward_1h_parry_light_left_stance",
            "act_defend_right_1h_parry_light_left_stance",
            "act_defend_left_1h_parry_light_left_stance",

            "act_defend_up_2h_parry_light_left_stance",
            "act_defend_forward_2h_parry_light_left_stance",
            "act_defend_right_2h_parry_light_left_stance",
            "act_defend_left_2h_parry_light_left_stance",


        };

        /// <summary>Saber Hit Sound List from Module_Sounds.xml</summary>
        public List<string> saberHitList = new List<string>() {
            "sabersounds/ltsaberhit02"
            , "sabersounds/ltsaberhit03"
            , "sabersounds/ltsaberhit07"
            , "sabersounds/ltsaberhit15"
            , "sabersounds/ltsaberbodyhit01" };
        
        /// <summary>Saber Hit Swing List from Module_Sounds.xml</summary>
        public List<string> saberSwingList = new List<string>() {
            "sabersounds/ltsaberswing01"
            ,"sabersounds/ltsaberswing02"
            ,"sabersounds/ltsaberswing03"
            ,"sabersounds/ltsaberswing04" //favorite 
            ,"sabersounds/ltsaberswing05"
            ,"sabersounds/ltsaberswing06"
            ,"sabersounds/ltsaberswing07"
            ,"sabersounds/ltsaberswing08"
            ,"sabersounds/ltsaberswingdbl01" };

        /// <summary>Saber Deflect Sound List from Module_Sounds.xml</summary>
        public List<string> saberDeflectList = new List<string>() {
            "sabersounds/ltsaberdeflect01"
            ,"sabersounds/ltsaberdeflect02"
            ,"sabersounds/ltsaberdeflect03"
            ,"sabersounds/ltsaberdeflect04"
            ,"sabersounds/ltsaberdeflect05"
            ,"sabersounds/ltsaberdeflect06"
            ,"sabersounds/ltsaberdeflect07"
            ,"sabersounds/ltsaberdeflect08"
            ,"sabersounds/ltsaberdeflect09"
            ,"sabersounds/ltsaberdeflect10"
        };

        /// <summary>The 3 Weapon Descriptions that Lightsabers can be. Used to help identify if Agent is wielding Lightsaber (defined in WeaponDescriptions.xml and Crafting_templates.xml</summary>
        public List<string> saberWeaponDescriptions = new List<string>()
        {
            "Lightsaber",
            "LightsaberBastardSword",
            "LightsaberTwoHanded"
        };

        public LightsaberSounds()
        {
            //ConstructLightsaberSoundList(Mission.Current);

        }

        /// <summary> Take the agent's defense action str and convert that str to a specific animation. Animation's were selected based on what looked most like a jedi blocking blasters from base game animations </summary>
        /// <param name="parryActionStr">ex) act_defend_forward_2h_parry_light_left_stance  </param>
        /// <returns>New Animation name for deflecting </returns>
        private string Anim_ManuallyAssignDeflectAnimation(string parryActionStr)
        {

            string result = "";


            Random rnd = new Random();

            //Get random choice from list            
            int r = rnd.Next(0, 2);

            
            //If either Left or Right guard, replace with DESIRED specific animations
            if (parryActionStr.Contains("defend_left"))
            {

                if (r == 0)
                {
                    result = "act_defend_forward_2h_parry_light_left_stance";   //Left
                }
                else
                {
                    result = "act_defend_forward_2h_active";    //Right
                }


            }
            else if (parryActionStr.Contains("defend_right"))
            {


                if (r == 0)
                {
                    result = "act_defend_up_2h_active_left_stance";     //Left
                }
                else
                {
                    result = "act_defend_forward_2h_parry_light";       //Right
                }

            }
            else if (parryActionStr.Contains("defend_up"))
            {
                if (r == 0)
                {
                    result = "act_defend_forward_2h_parry_light_left_stance";    //Left
                }
                else
                {
                    result = "act_defend_forward_2h_parry_light";    //Right
                }

            }
            else if (parryActionStr.Contains("defend_forward")) 
            {
                if (r == 0)
                {
                    result = "act_defend_forward_2h_active_left_stance";    //Left
                }
                else
                {
                    result = "act_defend_forward_2h_active";    //Right
                }
               
            }
            else
            {
                //dbg += "    <ERROR> using fall back of Converting original action to ACTION string ";
                result = Anim_ConvertParryToActiveAction(parryActionStr);
            }

            //System.Diagnostics.Debug.WriteLine(dbg);

            return result;
        }



        /// <summary>If wanted, converts a parry animation to an "active" one.</summary>
        /// <param name="parryActionStr"></param>
        private string Anim_ConvertParryToActiveAction(string parryActionStr)
        {
            string result = "";
            string result2 = "";

            //string dbg = "\n      [Anim_ConvertParryToACTIVEAction] - ";

            //  ex) (actual Parry output) act_defend_left_2h_parry_light_left_stance
            //  ex) (from master var)    act_defend_up_2h_passive_left_stance

            result2 = parryActionStr;



            //If either Left or Right guard, replace with DESIRED specific animations
            if (parryActionStr.Contains("defend_left"))
            {
                result2 = parryActionStr.Replace("defend_left", "defend_forward");


            }
            else if (parryActionStr.Contains("defend_right"))
            {
                result2 = parryActionStr.Replace("defend_right", "defend_up");
            }
            

            result = result2;

            //Replace parry/passive -> active
            if (result2.Contains("_parry_light"))
            {
                result = result2.Replace("_parry_light", "_active");

            }
            else if (result2.Contains("_passive"))
            {
                result = result2.Replace("_passive", "_active");

            }

            //System.Diagnostics.Debug.WriteLine(dbg);

            return result;
        }


        private string Anim_ConvertParryToPassiveAction(string parryActionStr)
        {
            string result = "";
            string dbg = "";

            //  ex) (actual Parry output) act_defend_left_2h_parry_light_left_stance
            //  ex) (from master var)    act_defend_up_2h_passive_left_stance

            dbg += "\n[Anim_ConvertParryToPASSIVEAction] - "
                ;

            result = parryActionStr;

            if (parryActionStr.Contains("_parry_light"))
            {


                result = parryActionStr.Replace("_parry_light", "_passive");

                dbg += " Replace parry Result = " + result;
            }
            else if (parryActionStr.Contains("_active"))
            {
                result = parryActionStr.Replace("_active", "_passive");

                dbg += " Replace active Result = " + result;

            }

            //act_defend
            System.Diagnostics.Debug.WriteLine(dbg);

            return result;
        }

        /// <summary>Confirm whether the agent's current action is a defend action </summary>
        /// <param name="currentActionName"></param>
        /// <param name="source">Method used to invoke this (used for debug mostly)</param>
        public bool Anim_CheckAgentCurrentAction(string currentActionName, string source)
        {
            bool result = false;

            if (currentActionName.Contains("act_defend"))
            {
                result = true;
            }

            return result;
        }

        /// <summary>If Agent was defending, forces the agent to switch animation to new/different defense animation</summary>        
        /// <param name="deflectingAgent"></param>
        /// <param name="originalActionString">Original Agent's "Action" they were using. If guarding action with lightsaber, change the guard direction</param>
        public void ChangeGuardDirection(Agent deflectingAgent, string originalActionString)//, bool isDeflectingAnimation )
        {

            if (deflectingAgent is not null && deflectingAgent.IsHuman)// deflectingAgent?.CurrentGuardMode != GuardMode.None)
            {
            
                string oldAction = "";  //Try to get the MasterOriginalAnimation for reference, but if needed, fall back to the current strike action

                if (originalActionString != "")
                {
                    oldAction = originalActionString;               
                }
                else
                {
                    // This is a fallback if the original action stream is blank
                    oldAction = deflectingAgent.GetCurrentAction(1).GetName().ToString();
            
                }


                //  Try to get the ACTIVE animation version of the parry
                string newActiveAnimation = "";

                //a = Anim_ConvertParryToActiveAction(oldAction);   //Non Randomized version of the Parry
                newActiveAnimation = Anim_ManuallyAssignDeflectAnimation(oldAction);


                //If Want to Randomly Change 2h animations to 1H //Probs Based on Weapon Usage
                //Saber_ReverseNewActionGrip_Check(a,deflectingAgent);    //Causing no animation? / Causing projectile to thunk like a tree

                if (
                    originalActionString.Contains("defend") == true &&
                    newActiveAnimation != ""
                    )
                {

                    /**********************************  
                     Activate the Deflect defend animation
                     * **********************************/

                    ActionIndexCache newSwingActionIndexCache = ActionIndexCache.Create(newActiveAnimation);// act_defend_left_2h_passive_left_stance

                    /*  Activate the animation   */
                    deflectingAgent.SetActionChannel(1, newSwingActionIndexCache, ignorePriority: true, AnimFlags.amf_priority_defend_parry);

                }
                else
                {
                    //Default to just a random Parry animation if failed to convert Old action 
                    //Assign the Random new Guard
                    string u = RandomNewGuardDirection(oldAction);

                    /*  This is Where we have the action ASSIGNED */
                    ActionIndexCache newParryActionIndexCache = ActionIndexCache.Create(u);

                    /*  Activate the animation   */
                    deflectingAgent.SetActionChannel(1, newParryActionIndexCache, ignorePriority: true, AnimFlags.amf_priority_defend_parry);
          
                }
              
            }        

        }

        /// <summary> Select a random Guard direction that doesnt match the original one the agent was doing 
        /// (Note: this is used for fallback if for some reason deflecting agent's action SHOULD be guarding but passed as stagger.
        /// SHOULD usually use the Manual deflection assignment method)</summary>
        /// <param name="currentUsageDirection">Deflecting Agent's original action (Should be a guard)</param>
        /// <returns>New Randomly selected guard direction</returns>
        public string RandomNewGuardDirection(string currentUsageDirection)//string activeGuardMode)
        {
            
            string newDirection = "";

            //Find 1h or 2h, if error, then default to 2h
            string grip = "";

            if (currentUsageDirection.Contains("1h"))
            {
                grip = "1h";
            }
            else if (currentUsageDirection.Contains("2h"))
            {
                grip = "2h";
            }
            else //default if no match
            {
                grip = "2h";
            }

            string direction = "";

            if (currentUsageDirection.Contains("act_defend_left"))
            {
                direction = "act_defend_left";
            }
            else if (currentUsageDirection.Contains("act_defend_right"))
            {
                direction = "act_defend_right";
            }
            else if (currentUsageDirection.Contains("act_defend_forward"))
            {
                direction = "act_defend_forward";
            }
            else
            {
                direction = "act_defend_up";
            }


            //All available defend animations EXCEPT the any that match the direction we were already doing
            List<string> allDiffDefendUsageDirections = saberActionIndexCacheStrings.Where(x =>
                 x != currentUsageDirection &&
                 x.Contains(grip) &&
                 !x.Contains(direction)
            ).ToList();


            //create random
            Random rnd = new Random();

            //Get random choice from list
            int r = rnd.Next(allDiffDefendUsageDirections.Count);
          
            if (allDiffDefendUsageDirections.Count() > 0)
            {
                newDirection = allDiffDefendUsageDirections[r]; //Pick Random Animation
            }
            else
            {
                newDirection = currentUsageDirection;   //fallback to original if fails               
            }

            return newDirection;
        }

        /// <summary>Selects a random saber sound name from the provided list. </summary>
        /// <param name="soundNameList">A list of available saber sound names to choose from. Options are: 
        /// - saberHitList (for hit sounds) | - saberSwingList (for swing sounds) | - saberDeflectList (for deflect sounds) | any other group of sounds from string List of the sound names</param>
        /// <returns>A string containing the randomly selected saber sound name from the list.</returns>
        public string RandomSaberSound(List<string> soundNameList)
        {
            string hitSound = "";

            //create random
            Random rnd = new Random();

            //Get random choice from list
            int r = rnd.Next(soundNameList.Count);

            //get random hit sound result from list
            hitSound = soundNameList[r];

            return hitSound;
        }




    }


}
