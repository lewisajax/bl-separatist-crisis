using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.SetOverride
{
    public class EquipmentSetOverrideLogic: MissionLogic
    {
        private MissionState? _state;

        // We'll need a patch
        // We'd want to go into Mission.SpawnAgent and then look at agentBuildData.AgentOrigin to determine what party and culture they're from

        // CustomGame
        public EquipmentSetOverrideLogic(CustomBattleCombatant attackerSide, CustomBattleCombatant defenderSide)
        {
            // The BasicCharacters all share the same memory address, so this is not the way to do this
            Console.WriteLine("Hello");
            IEnumerator<BasicCharacterObject> enumerator = attackerSide.Characters.GetEnumerator();
            for (int i = 0; i < attackerSide.CountOfCharacters; i++)
            {
                BasicCharacterObject character = enumerator.Current;
                enumerator.MoveNext();
            }
        }

        // Campaign
        public EquipmentSetOverrideLogic(MapEventSide attackerSide, MapEventSide defenderSide)
        {
            Console.WriteLine("Hello");
        }

        public override void OnBehaviorInitialize()
        {
        }

        public override void EarlyStart()
        {
            this._state = Game.Current.GameStateManager.ActiveState as MissionState;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
        }
    }
}
