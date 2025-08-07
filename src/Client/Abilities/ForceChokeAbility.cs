using SeparatistCrisis.Entities;
using SeparatistCrisis.ObjectTypes;
using SeparatistCrisis.ScriptComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.Abilities
{
    public class ForceChokeAbility : IAbility
    {
        private Ability _options;
        private AbilityAgent _abilityAgent;
        private float _boxLength;
        private float _boxSize;

        public float BoxLength
        {
            get { return _boxLength; }
            set { this._boxLength = value; }
        }

        public float BoxSize
        {
            get { return _boxSize; }
            set { this._boxSize = value; }
        }

        public float LastCheck { get; set; }

        public Ability Options
        {
            get
            {
                return this._options;
            }
        }

        public AbilityAgent AbilityAgent
        {
            get
            {
                return this._abilityAgent;
            }
        }

        public bool IsActive { get; set; }

        public GameEntity? ActiveEntity { get; set; }

        public ForceChokeAbility(AbilityAgent abilityAgent, Ability options)
        {
            this._abilityAgent = abilityAgent;
            this._options = options;
            this._boxLength = 8f;
            this._boxSize = 2f;
            this.LastCheck = Mission.Current.CurrentTime;
            this.IsActive = false;
        }

        public Mesh CreateMesh()
        {
            Mesh cube = Mesh.GetFromResource("editor_cube").CreateCopy();
            cube.SetMaterial(Material.GetFromResource("editor_gizmo"));

            UIntPtr uintPtr = cube.LockEditDataWrite();
            ManagedMeshEditOperations edit = ManagedMeshEditOperations.Create(cube);

            MatrixFrame transform = new MatrixFrame(
                this.BoxSize, 0f, 0f, 0f,
                0f, this.BoxLength, 0f, 0f,
                0f, 0f, this.BoxSize, 0f,
                0f, this.BoxLength / 2, 0f, 1f);

            edit.TransformVerticesToParent(transform);
            edit.Weld();
            edit.EnsureTransformedVertices();
            edit.FinalizeEditing();

            cube.ComputeNormals();
            cube.ComputeTangents();
            cube.UnlockEditDataWrite(uintPtr);
            cube.RecomputeBoundingBox();
            cube.UpdateBoundingBox();
            return cube;
        }

        public void OnTick(float dt)
        {
            if (Mission.Current.InputManager.IsKeyReleased(TaleWorlds.InputSystem.InputKey.B))
            {
                this.ActiveEntity?.Remove(0);
                this.ActiveEntity = null;
                this.LastCheck = Mission.Current.CurrentTime;
                this.IsActive = false;

                return;
            }

            if (Mission.Current.InputManager.IsKeyDown(TaleWorlds.InputSystem.InputKey.B))
            {
                if (!this.IsActive && Mission.Current.CurrentTime > ((this.LastCheck + dt) + 4f))
                {
                    Agent agent = this.AbilityAgent.Agent;

                    Mesh cube = this.CreateMesh();

                    GameEntity entity = GameEntity.CreateEmptyDynamic(Mission.Current.Scene);
                    PhysicsShape body = PhysicsShape.GetFromResource("bo_editor_cube");

                    object obj = new object();
                    lock (obj)
                    {
                        body.Prepare();
                        CapsuleData capsule = new CapsuleData(this.BoxSize, cube.GetBoundingBoxMin(), cube.GetBoundingBoxMax());
                        body.AddCapsule(capsule);
                    }

                    entity.SetBodyShape(body);
                    entity.AddPhysics(1, entity.CenterOfMass, entity.GetBodyShape(), agent.LookDirection * 10, agent.LookDirection * 10, PhysicsMaterial.GetFromName("boulder_stone"), false, 1);

                    entity.AddMesh(cube);

                    MatrixFrame globalFrame = agent.AgentVisuals.GetGlobalFrame();
                    MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(agent.Monster.MainHandItemBoneIndex);
                    MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

                    entity.SetGlobalFrame(new MatrixFrame(agent.LookRotation, matrixFrame.origin));

                    entity.SetFrameChanged();
                    entity.RecomputeBoundingBox();

                    entity.CreateAndAddScriptComponent(ForceChokeProjectile.Name);
                    entity.GetFirstScriptOfType<ForceChokeProjectile>().AbilityAgent = this.AbilityAgent;
                    entity.CallScriptCallbacks();

                    GameEntity entity2 = GameEntity.CreateEmptyDynamic(Mission.Current.Scene);
                    Mesh cube2 = Mesh.GetFromResource("editor_cube").CreateCopy();
                    cube2.Color = 0xFF0000;
                    entity2.AddMesh(cube2);
                    entity2.SetGlobalFrame(new MatrixFrame(entity.GetFrame().rotation, entity.GlobalBoxMax));

                    GameEntity entity3 = GameEntity.CreateEmptyDynamic(Mission.Current.Scene);
                    Mesh cube3 = Mesh.GetFromResource("editor_cube").CreateCopy();
                    cube3.Color = 0x00F0FF;
                    entity3.AddMesh(cube2);
                    entity3.SetGlobalFrame(new MatrixFrame(entity.GetFrame().rotation, entity.GlobalBoxMin));

                    if (this.ActiveEntity == null)
                    {
                        this.ActiveEntity = entity;
                        this.IsActive = true;
                    }
                }
            }
        }
    }
}
