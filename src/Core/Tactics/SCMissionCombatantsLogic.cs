using SeparatistCrisis.Tactics.TeamComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics
{
    public class SCMissionCombatantsLogic: MissionCombatantsLogic
    {
        public SCMissionCombatantsLogic(IEnumerable<IBattleCombatant> battleCombatants, IBattleCombatant playerBattleCombatant, IBattleCombatant defenderLeaderBattleCombatant, IBattleCombatant attackerLeaderBattleCombatant, Mission.MissionTeamAITypeEnum teamAIType, bool isPlayerSergeant) : 
            base(battleCombatants, playerBattleCombatant, defenderLeaderBattleCombatant, attackerLeaderBattleCombatant, teamAIType, isPlayerSergeant)
        {
        }

        public override void EarlyStart()
        {
            // Mission.Current.MissionTeamAIType = this.TeamAIType;
            switch (this.TeamAIType)
            {
                case Mission.MissionTeamAITypeEnum.FieldBattle:
                    this.AddFieldBattleTeamComponent();
                    break;
                case Mission.MissionTeamAITypeEnum.Siege:
                    this.AddSiegeTeamComponent();
                    break;
                case Mission.MissionTeamAITypeEnum.SallyOut:
                    this.AddSallyOutTeamComponent();
                    break;
                default:
                    break;
            }

            if (Mission.Current.Teams.Count > 0)
            {
                switch (Mission.Current.MissionTeamAIType)
                {
                    case Mission.MissionTeamAITypeEnum.NoTeamAI:
                        using (List<Team>.Enumerator enumerator = Mission.Current.Teams.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Team team = enumerator.Current;
                                if (team.HasTeamAi)
                                {
                                    team.AddTacticOption(new TacticCharge(team));
                                }
                            }
                        }
                        break;
                    case Mission.MissionTeamAITypeEnum.FieldBattle:
                        this.AddFieldBattleTactics();
                        break;
                    case Mission.MissionTeamAITypeEnum.Siege:
                        this.AddSiegeTactics();
                        break;
                    case Mission.MissionTeamAITypeEnum.SallyOut:
                        this.AddSallyOutTactics();
                        break;
                    default:
                        break;
                }

                foreach (Team team in base.Mission.Teams)
                {
                    team.QuerySystem.Expire();
                    team.ResetTactic();
                }
            }
        }

        private void AddFieldBattleTeamComponent()
        {
            using (List<Team>.Enumerator enumerator = Mission.Current.Teams.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Team team = enumerator.Current;
                    // team.AddTeamAI(new TeamAIGeneral(base.Mission, team, 10f, 1f), false);
                    team.AddTeamAI(new SCTeamAIGeneral(base.Mission, team, 10f, 1f), false);

                }
            }
        }

        private void AddFieldBattleTactics()
        {
            using (List<Team>.Enumerator enumerator = Mission.Current.Teams.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Team team = enumerator.Current;
                    if (team.HasTeamAi)
                    {
                        // Gets the highest tactics skill from the troops/player on each side
                        int num = (from bc in this.BattleCombatants
                                   where bc.Side == team.Side
                                   select bc).Max((IBattleCombatant bcs) => bcs.GetTacticsSkillAmount());

                        team.AddTacticOption(new TacticCharge(team));

                        if ((float)num >= 20f)
                        {
                            team.AddTacticOption(new TacticFullScaleAttack(team));

                            if (team.Side == BattleSideEnum.Defender)
                            {
                                team.AddTacticOption(new TacticDefensiveEngagement(team));
                                team.AddTacticOption(new TacticDefensiveLine(team));
                            }
                            if (team.Side == BattleSideEnum.Attacker)
                            {
                                team.AddTacticOption(new TacticRangedHarrassmentOffensive(team));
                            }
                        }
                        if ((float)num >= 50f)
                        {
                            team.AddTacticOption(new TacticFrontalCavalryCharge(team));
                        }
                    }
                }
            }
        }

        private void AddSiegeTeamComponent()
        {
            using (List<Team>.Enumerator enumerator = Mission.Current.Teams.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Team team = enumerator.Current;
                    if (team.Side == BattleSideEnum.Attacker)
                    {
                        team.AddTeamAI(new TeamAISiegeAttacker(base.Mission, team, 5f, 1f), false);
                    }
                    if (team.Side == BattleSideEnum.Defender)
                    {
                        team.AddTeamAI(new TeamAISiegeDefender(base.Mission, team, 5f, 1f), false);
                    }
                }
            }
        }

        private void AddSiegeTactics()
        {
            using (List<Team>.Enumerator enumerator = Mission.Current.Teams.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Team team = enumerator.Current;
                    if (team.HasTeamAi)
                    {
                        if (team.Side == BattleSideEnum.Attacker)
                        {
                            team.AddTacticOption(new TacticBreachWalls(team));
                        }
                        if (team.Side == BattleSideEnum.Defender)
                        {
                            team.AddTacticOption(new TacticDefendCastle(team));
                        }
                    }
                }
            }
        }

        private void AddSallyOutTeamComponent()
        {
            foreach (Team team in Mission.Current.Teams)
            {
                if (team.Side == BattleSideEnum.Attacker)
                {
                    team.AddTeamAI(new TeamAISallyOutDefender(base.Mission, team, 5f, 1f), false);
                }
                else
                {
                    team.AddTeamAI(new TeamAISallyOutAttacker(base.Mission, team, 5f, 1f), false);
                }
            }
        }

        private void AddSallyOutTactics()
        {
            foreach (Team team in Mission.Current.Teams)
            {
                if (team.HasTeamAi)
                {
                    if (team.Side == BattleSideEnum.Defender)
                    {
                        team.AddTacticOption(new TacticSallyOutHitAndRun(team));
                    }

                    if (team.Side == BattleSideEnum.Attacker)
                    {
                        team.AddTacticOption(new TacticSallyOutDefense(team));
                    }

                    team.AddTacticOption(new TacticCharge(team));
                }
            }
        }
    }
}
