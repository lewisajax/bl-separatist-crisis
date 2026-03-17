using SeparatistCrisis.Tactics.OrderStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Tactics.BehaviorComponents
{
    public class BehaviorUseCover: BehaviorComponent
    {
        private SCFormation? _formationDecorator;

        private GameEntity _archerPosition;

        private TacticalPosition _tacticalArcherPosition;

        private bool _areStrategicArcherAreasAbandoned;

        private Formation _mainFormation;

        private CoverOrder _currentCoverOrder;

        public CoverOrder CurrentCoverOrder
        {
            get
            {
                return this._currentCoverOrder;
            }
            protected set
            {
                this._currentCoverOrder = value;
                // this.IsCurrentOrderChanged = true; // Look at using our own toggle for SC orders as well
            }
        }

        public SCFormation? FormationDecorator
        {
            get { return _formationDecorator; }
            protected set { _formationDecorator = value; }
        }

        public override float NavmeshlessTargetPositionPenalty
        {
            get
            {
                return 1f;
            }
        }

        // Will get set in TacticUserCover
        public GameEntity ArcherPosition
        {
            get
            {
                return this._archerPosition;
            }
            set
            {
                if (this._archerPosition != value)
                {
                    this.OnArcherPositionSet(value);
                }
            }
        }

        public BehaviorUseCover(Formation formation, SCFormation? decorator) : base(formation)
        {
            this._mainFormation = formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
            this.CalculateCurrentOrder();
            if (this.ArcherPosition != null)
                this.OnArcherPositionSet(this.ArcherPosition);
            base.BehaviorCoherence = 0f;

            if (decorator != null)
                this._formationDecorator = decorator;
            else
                Debug.Print("BehaviorUseCover's formation decorator is null");
        }

        private void OnArcherPositionSet(GameEntity value)
        {
            this._archerPosition = value;
            if (!(this._archerPosition != null))
            {
                this._tacticalArcherPosition = null;
                WorldPosition cachedMedianPosition = base.Formation.CachedMedianPosition;
                cachedMedianPosition.SetVec2(base.Formation.CurrentPosition);
                base.CurrentOrder = MovementOrder.MovementOrderMove(cachedMedianPosition);
                this.CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
                return;
            }
            this._tacticalArcherPosition = this._archerPosition.GetFirstScriptOfType<TacticalPosition>();
            if (this._tacticalArcherPosition != null)
            {
                base.CurrentOrder = MovementOrder.MovementOrderMove(this._tacticalArcherPosition.Position);
                this.CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(this._tacticalArcherPosition.Direction);
                return;
            }
            base.CurrentOrder = MovementOrder.MovementOrderMove(this._archerPosition.GlobalPosition.ToWorldPosition());
            this.CurrentFacingOrder = FacingOrder.FacingOrderLookAtDirection(this._archerPosition.GetGlobalFrame().rotation.f.AsVec2);
        }

        protected override void CalculateCurrentOrder()
        {
            base.CurrentOrder = MovementOrder.MovementOrderStop;
        }

        public override void TickOccasionally()
        {
            base.Formation.SetMovementOrder(base.CurrentOrder);
            base.Formation.SetFacingOrder(this.CurrentFacingOrder);
            if (this._tacticalArcherPosition != null)
            {
                base.Formation.SetFormOrder(FormOrder.FormOrderCustom(this._tacticalArcherPosition.Width), true);
            }
            foreach (Team team in base.Formation.Team.Mission.Teams)
            {
                if (team.IsEnemyOf(base.Formation.Team))
                {
                    if (!this._areStrategicArcherAreasAbandoned)
                    {
                        if (team.QuerySystem.InsideWallsRatio > 0.6f)
                        {
                            base.Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
                            this._areStrategicArcherAreasAbandoned = true;
                            break;
                        }
                        break;
                    }
                    else
                    {
                        if (team.QuerySystem.InsideWallsRatio <= 0.4f)
                        {
                            base.Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderScatter);
                            this._areStrategicArcherAreasAbandoned = false;
                            break;
                        }
                        break;
                    }
                }
            }
        }

        protected override void OnBehaviorActivatedAux()
        {
            base.Formation.SetMovementOrder(base.CurrentOrder);
            base.Formation.SetFacingOrder(this.CurrentFacingOrder);
            base.Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderScatter);
            base.Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
            base.Formation.SetFormOrder(FormOrder.FormOrderWide, true);
        }

        protected override float GetAiWeight()
        {
            if (this._mainFormation == null || !this._mainFormation.AI.IsMainFormation)
            {
                this._mainFormation = base.Formation.Team.FormationsIncludingEmpty.FirstOrDefaultQ((Formation f) => f.CountOfUnits > 0 && f.AI.IsMainFormation);
            }
            if (this._mainFormation == null || base.Formation.AI.IsMainFormation || base.Formation.CachedClosestEnemyFormation == null || !base.Formation.QuerySystem.IsRangedFormation)
            {
                return 0f;
            }
            return 2f;
        }
    }
}
