using SeparatistCrisis.Tactics.BehaviorComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics.TacticComponents
{
    public class TacticUseCover: TacticComponent
    {
        // We'll need to make our own class
        private readonly List<SiegeLane> _lanes;

        // BehaviorUseCover.ArcherPosition needs to be set in here.

        public TacticUseCover(Team team) : base(team)
        {
        }

        public override void TickOccasionally()
        {
            foreach (Formation formation in base.FormationsIncludingEmpty)
            {
                if (formation.CountOfUnits > 0)
                {
                    formation.AI.ResetBehaviorWeights();
                    TacticComponent.SetDefaultBehaviorWeights(formation);
                    formation.AI.SetBehaviorWeight<BehaviorUseCover>(1f);
                }
            }
            base.TickOccasionally();
        }

        protected override float GetTacticWeight()
        {
            float num = base.Team.QuerySystem.RemainingPowerRatio / base.Team.QuerySystem.TotalPowerRatio;
            float num2 = TaleWorlds.Library.MathF.Max(base.Team.QuerySystem.InfantryRatio, TaleWorlds.Library.MathF.Max(base.Team.QuerySystem.RangedRatio, base.Team.QuerySystem.CavalryRatio)) + ((base.Team.Side == BattleSideEnum.Defender) ? 0.33f : 0f);
            float num3 = (base.Team.Side == BattleSideEnum.Defender) ? 0.33f : 0.5f;
            float num4 = 0f;
            float num5 = 0f;
            CasualtyHandler missionBehavior = Mission.Current.GetMissionBehavior<CasualtyHandler>();
            foreach (Team team in base.Team.Mission.Teams)
            {
                if (team == base.Team || team.IsEnemyOf(base.Team))
                {
                    for (int i = 0; i < Math.Min(team.FormationsIncludingSpecialAndEmpty.Count, 8); i++)
                    {
                        Formation formation = team.FormationsIncludingSpecialAndEmpty[i];
                        if (formation.CountOfUnits > 0)
                        {
                            num4 += formation.QuerySystem.FormationPower;
                            num5 += missionBehavior.GetCasualtyPowerLossOfFormation(formation);
                        }
                    }
                }
            }
            float num6 = num4 + num5;
            float num7 = num4 / num6;
            num7 = ((base.Team.Side == BattleSideEnum.Attacker && num < 0.5f) ? 0f : MBMath.LinearExtrapolation(0f, 1.6f * num2, (1f - num7) / (1f - num3)));
            float b = MBMath.LinearExtrapolation(0f, 1.6f * num2, base.Team.QuerySystem.RemainingPowerRatio * num3 * 0.5f);
            return TaleWorlds.Library.MathF.Max(num7, b);
        }

        /*protected override bool CheckAndSetAvailableFormationsChanged()
        {
            int aicontrolledFormationCount = base.Team.GetAIControlledFormationCount();
            bool flag2 = aicontrolledFormationCount != this._AIControlledFormationCount;
            if (flag2)
            {
                this._AIControlledFormationCount = aicontrolledFormationCount;
                this.IsTacticReapplyNeeded = true;
            }
            return flag2;
        }*/

        /*private void DistributeRangedFormations()
        {
            List<Tuple<Formation, ArcherPosition>> list = this._rangedFormations.CombineWith(this._teamAISiegeDefender.ArcherPositions);
            while (list.Count > 0)
            {
                Tuple<Formation, ArcherPosition> tuple = list.MinBy((Tuple<Formation, ArcherPosition> c) => c.Item1.CachedMedianPosition.AsVec2.DistanceSquared(c.Item2.Entity.GlobalPosition.AsVec2));
                Formation bestFormation = tuple.Item1;
                ArcherPosition bestArcherPosition = tuple.Item2;
                bestFormation.AI.ResetBehaviorWeights();
                TacticComponent.SetDefaultBehaviorWeights(bestFormation);
                bestFormation.AI.SetBehaviorWeight<BehaviorShootFromCastleWalls>(1f);
                bestFormation.AI.GetBehavior<BehaviorShootFromCastleWalls>().ArcherPosition = bestArcherPosition.Entity;
                list.RemoveAll((Tuple<Formation, ArcherPosition> c) => c.Item1 == bestFormation || c.Item2 == bestArcherPosition);
            }
        }*/

        private void ArcherShiftAround(List<Formation> p_RangedFormations)
        {
            List<Formation> list = (from rf in p_RangedFormations
                                    where rf.AI.ActiveBehavior is BehaviorShootFromCastleWalls
                                    select rf).ToList<Formation>();
            if (list.Count < 2)
            {
                return;
            }
            float smallerFormationUnitPercentage = 0.1f;
            float mediumFormationUnitPercentage = 0.2f;
            float largerFormationUnitPercentage = 0.4f;
            float num = list.Sum(delegate (Formation f)
            {
                if ((f.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("many"))
                {
                    return largerFormationUnitPercentage;
                }
                if (!(f.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("few"))
                {
                    return mediumFormationUnitPercentage;
                }
                return smallerFormationUnitPercentage;
            });
            smallerFormationUnitPercentage /= num;
            mediumFormationUnitPercentage /= num;
            largerFormationUnitPercentage /= num;
            int num2 = list.Sum((Formation f) => f.CountOfUnitsWithoutDetachedOnes);
            int smallFormationCount = TaleWorlds.Library.MathF.Max((int)((float)num2 * smallerFormationUnitPercentage), 1);
            int mediumFormationCount = TaleWorlds.Library.MathF.Max((int)((float)num2 * mediumFormationUnitPercentage), 1);
            int largeFormationCount = TaleWorlds.Library.MathF.Max((int)((float)num2 * largerFormationUnitPercentage), 1);
            int num3 = TaleWorlds.Library.MathF.Max((int)((float)num2 * 0.1f), 1);
            Func<Formation, int>? selector = null;
            Func<Formation, bool>? predicate = null;
            foreach (Formation formation in list)
            {
                int num4 = (formation.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("many") ? largeFormationCount : ((formation.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("few") ? smallFormationCount : mediumFormationCount);
                int num5 = 0;
                while (num4 - formation.CountOfUnitsWithoutDetachedOnes > num3)
                {
                    IEnumerable<Formation> source = list;
                    if (predicate == null)
                    {
                        predicate = (((Formation rf) => rf.CountOfUnitsWithoutDetachedOnes > ((rf.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("many") ? largeFormationCount : ((rf.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("few") ? smallFormationCount : mediumFormationCount))));
                    }
                    if (!source.Any(predicate) || num5 >= list.Count)
                    {
                        break;
                    }
                    int num6 = num4 - formation.CountOfUnitsWithoutDetachedOnes;
                    IEnumerable<Formation> source2 = list;
                    if (selector == null)
                    {
                        selector = (((Formation rf) => rf.CountOfUnitsWithoutDetachedOnes - ((rf.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("many") ? largeFormationCount : ((rf.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("few") ? smallFormationCount : mediumFormationCount))));
                    }
                    Formation formation2 = TaleWorlds.Core.Extensions.MaxBy<Formation, int>(source2, selector);
                    num6 = TaleWorlds.Library.MathF.Min(num6, formation2.CountOfUnitsWithoutDetachedOnes - ((formation2.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("many") ? largeFormationCount : ((formation2.AI.ActiveBehavior as BehaviorShootFromCastleWalls).ArcherPosition.HasTag("few") ? smallFormationCount : mediumFormationCount)));
                    formation2.TransferUnits(formation, num6);
                    num5++;
                }
            }
        }

        private void CheckAndChangeState()
        {
            bool flag = false;
            // We could check if there's an enemy formation behind this formation
            if (this._invadingEnemyFormation != null)
            {
                flag = TeamAISiegeComponent.IsFormationInsideCastle(this._invadingEnemyFormation, true, 0.4f);
                if (!flag)
                {
                    this._invadingEnemyFormation = null;
                }
            }

            List<SiegeLane> list = (from l in this._lanes
                                    where l.LaneState == SiegeLane.LaneStateEnum.Conceited
                                    select l).ToList<SiegeLane>();
            List<SiegeLane> activeLanes = (from l in this._lanes.Except(list)
                                            where l.GetDefenseState() > SiegeLane.LaneDefenseStates.Empty
                                            select l).ToList<SiegeLane>();
            if (flag)
            {
                list.Clear();
            }

            bool flag2 = list.Count > 0;
            if (!flag2 && !flag && activeLanes.Count == 0)
            {
                activeLanes = (from l in this._lanes
                                where l.HasGate
                                select l).ToList<SiegeLane>();
            }

            if (flag2 && activeLanes.Count > 0)
            {
                SiegeLane item = list.MinBy((SiegeLane cl) => activeLanes.Min((SiegeLane al) => SiegeQuerySystem.SideDistance(1 << (int)al.LaneSide, 1 << (int)cl.LaneSide)));
                list.Clear();
                list.Add(item);
            }

            bool flag3 = flag2 || flag;
            this._meleeFormations = (from mf in this._meleeFormations
                                        where mf.CountOfUnits > 0
                                        select mf).ToList<Formation>();
            this._rangedFormations = (from rf in this._rangedFormations
                                        where rf.CountOfUnits > 0
                                        select rf).ToList<Formation>();

            int num = MathF.Max(this._meleeFormations.Sum((Formation mf) => mf.CountOfUnits), 1);
            int num2 = MathF.Max(this._rangedFormations.Sum((Formation rf) => rf.CountOfUnits), 1);
            int num3 = num + num2;

            if (!this._areRangedNeededForLaneDefense)
            {
                this._areRangedNeededForLaneDefense = ((float)num < (float)num3 * 0.33f);
            }

            int num4 = 0;
            if (flag3)
            {
                float num5 = (float)num - this._lanes.Sum((SiegeLane l) => l.CalculateLaneCapacity());
                if (flag)
                {
                    num4 = ((num5 >= 15f) ? 1 : 0);
                }
                else
                {
                    num4 = MathF.Min((int)num5 / 15, list.Count);
                }
            }

            if (activeLanes.Count + list.Count + num4 <= 0)
            {
                this._isTacticFailing = true;
                num4 = 1;
            }

            bool flag4;
            this.CarryOutDefense(activeLanes, list, flag && num4 > 0, this._areRangedNeededForLaneDefense, out flag4);
            if (!flag4)
            {
                this.BalanceLaneDefenders((from ldf in this._laneDefendingFormations
                                            where ldf.IsAIControlled && ldf.AI.ActiveBehavior is BehaviorDefendCastleKeyPosition
                                            select ldf).ToList<Formation>(), out flag4);
                if (!flag4)
                {
                    this.ArcherShiftAround(this._rangedFormations);
                }
            }
        }
    }
}
