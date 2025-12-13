using HarmonyLib;
using SeparatistCrisis.Abilities;
using SeparatistCrisis.Entities;
using SeparatistCrisis.ObjectTypes;
using SeparatistCrisis.ViewModels;
using SeparatistCrisis.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

/*
 * Mission.MissileAreaDamageCallback - Is internal
 * AgentProximityMap - Static methods to be used  
 * PhysicsShape - ctor is internal though
 * GameEntity.CreateEmpty or .CreateEmptyDynamic
 *  - Maybe we work with the primitive cube
 * MeshBuilder
 *  - Mesh.CreateMeshWithMaterial(Material.GetDefaultMaterial());
 * ManagedMeshEditOperations
 *  - Could use MoveVerticesAlongNormal and then scale up
 * 
 * Decal.CreateDecal - If we wanted to do something like burning on the floor
 * 
 * For AI:
 * RangedSiegeWeaponAi - This has some targetting methods that we can use
 * 
 * GameEntityPhysicsExtensions.DisableGravity
 * 
 * Could do a horse charge effect on each agent in the zone
 */

namespace SeparatistCrisis.Behaviors
{
    public class AbilitiesLogic: MissionLogic
    {
        private MissionState? _state;
        private Dictionary<Agent, BaseAbility> _abilities = null!;
        private Dictionary<Agent, AbilityAgent> _abilityAgents = null!;

        // This will change when we add it to the campaign. We shouldn't store this here.
        private MissionGauntletMainAgentAbilityEquipView _abilityView { get; set; } = null!;
        private int _prevWieldedIndex = -1;

        public override void OnBehaviorInitialize()
        {
            this._abilityView = Mission.Current.GetMissionBehavior<MissionGauntletMainAgentAbilityEquipView>();
        }

        public override void EarlyStart()
        {
            this._state = Game.Current.GameStateManager.ActiveState as MissionState;
            this._abilities = new Dictionary<Agent, BaseAbility>();
            this._abilityAgents = new Dictionary<Agent, AbilityAgent>();

            // We could go through all the abilities and do .EarlyStart in them as well
            // So that we can add internal delegates to a static prop in each ability class if needed

            // Do we want each ability to have MissionBehavior callbacks i.e. OnRegisterBlow etc?
        }

        private void AssignAbility(Agent agent)
        {
            // We'll just get a few abilities out of the way and then we'll look at cleaning up the code since I don't know what we'll need
            AbilityAgent abiAgent = this._abilityAgents[agent];
            if (abiAgent.Hero == null)
                return;

            Ability? abiOptions = null;
            if (this._abilityView.WieldedIndex != null && this._abilityView.WieldedIndex != -1)
            {
                abiOptions = abiAgent.Hero.Abilities[(int)this._abilityView.WieldedIndex];
                this._prevWieldedIndex = this._abilityView?.WieldedIndex ?? -1;
            }

            // Ability? options =  Ability.FindFirst(x => x.StringId == "abi_force_push");
            if (abiOptions != null)
            {
                Type? actionType = abiOptions.Action;
                if (actionType != null)
                {
                    // This is dangerous. There are some checks when we collect the types but if an ability class adds more params, we're fucked.
                    BaseAbility? action = Activator.CreateInstance(actionType, new object[] { abiAgent, abiOptions }) as BaseAbility;
                    if (action != null)
                    {
                        if (this._abilities.TryGetValue(agent, out _))
                        {
                            this._abilities[agent].Dispose();
                            this._abilities[agent] = action;
                        }
                        else
                        {
                            this._abilities.Add(agent, action);
                        }
                    }
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            for (int i = 0; i < base.Mission.Agents.Count; i++)
            {
                Agent agent = base.Mission.Agents[i];
                if (agent != null && agent.IsActive() && agent.Health > 1f)
                {
                    if (!agent.IsHuman)
                        continue;

                    // We're not gonna do anything for AI just yet
                    if (!agent.IsMainAgent)
                        continue;

                    // We'll control an entity that gets shared between any equipped abilities
                    // This is so that any passive effects etc, will remain after the player changes abilities
                    if (!this._abilityAgents.ContainsKey(agent))
                        this._abilityAgents.Add(agent, new AbilityAgent(agent));

                    if (!this._abilities.ContainsKey(agent))
                    {
                        this.AssignAbility(agent);
                    }

                    if (this._abilityView != null && this._abilityView.WieldedIndex != this._prevWieldedIndex)
                    {
                        this.AssignAbility(agent);
                    }

                    // Gonna let the ability itself decide on what key activates or how long the delay is etc
                    // Probably gonna be repeating ourselves in most of the ability classes but we can always re-factor later
                    if (this._abilities.TryGetValue(agent, out BaseAbility? ability))
                        ability?.OnTick(dt);

                }
            }
        }
    }
}
