using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics.BehaviorComponents
{
    public class BehaviorUseCover: BehaviorComponent
    {
        public override float NavmeshlessTargetPositionPenalty
        {
            get
            {
                return 1f;
            }
        }

        public BehaviorUseCover(Formation formation) : base(formation)
        {
            this.CalculateCurrentOrder();
            base.BehaviorCoherence = 0.5f;
        }

        protected override void CalculateCurrentOrder()
        {
            base.CurrentOrder = ((base.Formation.CachedClosestEnemyFormation == null) ? 
                MovementOrder.MovementOrderCharge : 
                MovementOrder.MovementOrderChargeToTarget(base.Formation.CachedClosestEnemyFormation.Formation));
        }

        public override void TickOccasionally()
        {
            base.TickOccasionally();
            TeamAISiegeComponent teamAISiegeComponent;
            if ((teamAISiegeComponent = (base.Formation.Team.TeamAI as TeamAISiegeComponent)) != null && teamAISiegeComponent.OuterGate != null && !teamAISiegeComponent.OuterGate.IsGateOpen && teamAISiegeComponent.InnerGate != null && !teamAISiegeComponent.InnerGate.IsGateOpen)
            {
                CastleGate castleGate = teamAISiegeComponent.InnerGate ?? teamAISiegeComponent.OuterGate;
                if (castleGate != null && !castleGate.IsUsedByFormation(base.Formation))
                {
                    base.Formation.StartUsingMachine(castleGate, false);
                }
            }
            this.CalculateCurrentOrder();
            base.Formation.SetMovementOrder(base.CurrentOrder);
        }

        protected override void OnBehaviorActivatedAux()
        {
            base.Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
            if (base.Formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall)
            {
                base.Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
            }
        }

        protected override float GetAiWeight()
        {
            bool flag = base.Formation.Team.TeamAI is TeamAISiegeComponent;
            float result = 0f;
            FormationQuerySystem cachedClosestEnemyFormation = base.Formation.CachedClosestEnemyFormation;
            if (cachedClosestEnemyFormation == null)
            {
                if (base.Formation.Team.HasAnyEnemyTeamsWithAgents(false))
                {
                    result = 0.2f;
                }
            }
            else
            {
                bool flag2 = false;
                bool flag3;
                if (!flag)
                {
                    flag3 = true;
                }
                else if ((base.Formation.Team.TeamAI as TeamAISiegeComponent).CalculateIsChargePastWallsApplicable(base.Formation.AI.Side))
                {
                    flag3 = true;
                }
                else
                {
                    flag2 = TeamAISiegeComponent.IsFormationInsideCastle(cachedClosestEnemyFormation.Formation, true, 0.51f);
                    flag3 = (flag2 == TeamAISiegeComponent.IsFormationInsideCastle(base.Formation, true, flag2 ? 0.9f : 0.1f));
                }
                if (flag3)
                {
                    result = this.CalculateAIWeight(flag, flag2);
                }
            }
            return result;
        }

        private float CalculateAIWeight(bool isSiege, bool isInsideCastle)
        {
            FormationQuerySystem querySystem = base.Formation.QuerySystem;
            FormationQuerySystem cachedClosestEnemyFormation = base.Formation.CachedClosestEnemyFormation;
            float num = base.Formation.CachedAveragePosition.Distance(cachedClosestEnemyFormation.Formation.CachedMedianPosition.AsVec2) / querySystem.MovementSpeedMaximum;
            float num3;
            if (!querySystem.IsCavalryFormation && !querySystem.IsRangedCavalryFormation)
            {
                float num2 = MBMath.ClampFloat(num, 4f, 10f);
                num3 = MBMath.Lerp(0.1f, 1f, 1f - (num2 - 4f) / 6f, 1E-05f);
            }
            else if (num <= 4f)
            {
                float num4 = MBMath.ClampFloat(num, 0f, 4f);
                num3 = MBMath.Lerp(0.1f, 1.4f, num4 / 4f, 1E-05f);
            }
            else
            {
                float num5 = MBMath.ClampFloat(num, 4f, 10f);
                num3 = MBMath.Lerp(0.1f, 1.4f, 1f - (num5 - 4f) / 6f, 1E-05f);
            }
            float num6 = 0f;
            foreach (Team team in Mission.Current.Teams)
            {
                if (team.IsEnemyOf(base.Formation.Team))
                {
                    foreach (Formation formation in team.FormationsIncludingSpecialAndEmpty)
                    {
                        if (formation.CountOfUnits > 0 && cachedClosestEnemyFormation.Formation != formation && (!isSiege || TeamAISiegeComponent.IsFormationInsideCastle(formation, true, 0.4f) == isInsideCastle))
                        {
                            float num7 = formation.CachedMedianPosition.AsVec2.Distance(cachedClosestEnemyFormation.Formation.CachedMedianPosition.AsVec2) / formation.QuerySystem.MovementSpeedMaximum;
                            if (num7 <= num + 4f && (num > 8f || formation.CachedClosestEnemyFormation == base.Formation.QuerySystem))
                            {
                                bool flag = false;
                                if (num <= 8f)
                                {
                                    foreach (Team team2 in base.Formation.Team.Mission.Teams)
                                    {
                                        if (team2.IsFriendOf(base.Formation.Team))
                                        {
                                            foreach (Formation formation2 in team2.FormationsIncludingSpecialAndEmpty)
                                            {
                                                if (formation2.CountOfUnits > 0 && formation2 != base.Formation && formation2.CachedClosestEnemyFormation == formation.QuerySystem && formation2.CachedMedianPosition.AsVec2.DistanceSquared(base.Formation.CachedAveragePosition) / formation2.QuerySystem.MovementSpeedMaximum < num7 + 4f)
                                                {
                                                    flag = true;
                                                    break;
                                                }
                                            }
                                            if (flag)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!flag)
                                {
                                    num6 += formation.QuerySystem.FormationMeleeFightingPower * formation.QuerySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f);
                                }
                            }
                        }
                    }
                }
            }
            float num8 = 0f;
            foreach (Team team3 in Mission.Current.Teams)
            {
                if (team3.IsFriendOf(base.Formation.Team))
                {
                    foreach (Formation formation3 in team3.FormationsIncludingSpecialAndEmpty)
                    {
                        if (formation3 != base.Formation && formation3.CountOfUnits > 0 && cachedClosestEnemyFormation == formation3.CachedClosestEnemyFormation && (!isSiege || TeamAISiegeComponent.IsFormationInsideCastle(formation3, true, 0.4f) == isInsideCastle) && formation3.CachedMedianPosition.AsVec2.Distance(formation3.CachedClosestEnemyFormation.Formation.CachedMedianPosition.AsVec2) / formation3.QuerySystem.MovementSpeedMaximum < 4f)
                        {
                            num8 += formation3.QuerySystem.FormationMeleeFightingPower * formation3.QuerySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f);
                        }
                    }
                }
            }
            float num9 = (base.Formation.QuerySystem.FormationMeleeFightingPower * querySystem.GetClassWeightedFactor(1f, 1f, 1f, 1f) + num8 + 1f) / (1f + num6 + cachedClosestEnemyFormation.Formation.QuerySystem.FormationMeleeFightingPower * cachedClosestEnemyFormation.GetClassWeightedFactor(1f, 1f, 1f, 1f));
            num9 /= ((!isSiege) ? MBMath.ClampFloat(querySystem.Team.RemainingPowerRatio, 0.2f, 3f) : MBMath.ClampFloat(querySystem.Team.RemainingPowerRatio, 0.5f, 3f));
            if (num9 > 1f)
            {
                num9 = (num9 - 1f) / 3f;
                num9 += 1f;
            }
            num9 = MBMath.ClampFloat(num9, 0.1f, 1.25f);
            float num10 = 1f;
            if (num <= 4f)
            {
                float length = (base.Formation.CachedAveragePosition - cachedClosestEnemyFormation.Formation.CachedMedianPosition.AsVec2).Length;
                if (length > 1E-45f)
                {
                    WorldPosition cachedMedianPosition = base.Formation.CachedMedianPosition;
                    cachedMedianPosition.SetVec2(base.Formation.CachedAveragePosition);
                    float navMeshZ = cachedMedianPosition.GetNavMeshZ();
                    if (!float.IsNaN(navMeshZ))
                    {
                        float value = (navMeshZ - cachedClosestEnemyFormation.Formation.CachedMedianPosition.GetNavMeshZ()) / length;
                        num10 = MBMath.Lerp(0.9f, 1.1f, (MBMath.ClampFloat(value, -0.58f, 0.58f) + 0.58f) / 1.16f, 1E-05f);
                    }
                }
            }
            float num11 = 1f;
            if (num <= 4f && num >= 1.5f)
            {
                num11 = 1.2f;
            }
            float num12 = 1f;
            if (num <= 4f && cachedClosestEnemyFormation.Formation.CachedClosestEnemyFormation != querySystem)
            {
                num12 = 1.2f;
            }
            float num13 = (!isSiege) ? (querySystem.GetClassWeightedFactor(1f, 1f, 1.5f, 1.5f) * cachedClosestEnemyFormation.GetClassWeightedFactor(1f, 1f, 0.5f, 0.5f)) : (querySystem.GetClassWeightedFactor(1f, 1f, 1.2f, 1.2f) * cachedClosestEnemyFormation.GetClassWeightedFactor(1f, 1f, 0.3f, 0.3f));
            return num3 * num9 * num10 * num11 * num12 * num13;
        }
    }
}
