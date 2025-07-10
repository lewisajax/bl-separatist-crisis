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
    public class ForcePushAbility : IAbility
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

        public ForcePushAbility(AbilityAgent abilityAgent, Ability options)
        {
            this._abilityAgent = abilityAgent;
            this._options = options;
            this._boxLength = 5f;
            this._boxSize = 3f;
            this.LastCheck = Mission.Current.CurrentTime;
            this.IsActive = false;
        }

        /// <summary>
        /// Builds the mesh that we'll use to find agents inside
        /// </summary>
        /// <returns></returns>
        public Mesh CreateMesh()
        {
            // Mesh cube = MeshBuilder.CreateUnitMesh(); // It's a 2d unit mesh not 3d
            Mesh cube = Mesh.GetFromResource("editor_cube").CreateCopy();

            UIntPtr uintPtr = cube.LockEditDataWrite();
            ManagedMeshEditOperations edit = ManagedMeshEditOperations.Create(cube);

            // Trying to make a frustum of a pyramid or something like it.
            // I don't think I can do it with a matrix for the editor cube as it's origin is at the center of the cube.

            Vec3 v1 = edit.GetPositionOfVertex(0);
            Vec3 p1 = v1 * .25f;
            p1.z = v1.z - (v1.z * .5f);

            Vec3 v2 = edit.GetPositionOfVertex(2);
            Vec3 p2 = v2 * .25f;
            p2.z = v1.z * .25f;

            Vec3 v3 = edit.GetPositionOfVertex(6);
            Vec3 p3 = v3 * .25f;

            Vec3 v4 = edit.GetPositionOfVertex(7);
            Vec3 p4 = v4 * .25f;
            p3.z = v4.z * .25f;
            p4.z = v4.z - (v4.z * .5f);

            edit.SetPositionOfVertex(0, p1);
            edit.SetPositionOfVertex(2, p2);
            edit.SetPositionOfVertex(6, p3);
            edit.SetPositionOfVertex(7, p4);

            // When we want different scales for different levels of this ability, we just plonk it in here.
            MatrixFrame transform = new MatrixFrame(
                this.BoxSize, 0f, 0f, 0f,
                0f, this.BoxLength, 0f, 0f,
                0f, 0f, this.BoxSize, 0f,
                0f, 1f, 0f, 1f);

            edit.TransformVerticesToParent(transform);

            cube.ComputeNormals();
            cube.ComputeTangents();
            cube.UpdateBoundingBox();
            cube.UnlockEditDataWrite(uintPtr);
            return cube;
        }

        public void OnTick(float dt)
        {
            if (Mission.Current.InputManager.IsKeyReleased(TaleWorlds.InputSystem.InputKey.B))
            {
                if (Mission.Current.CurrentTime > ((this.LastCheck + dt) + 4f))
                {
                    this.IsActive = false;
                    Agent agent = this.AbilityAgent.Agent;

                    // AgentProximityMap.ProximityMapSearchStruct result = AgentProximityMap.BeginSearch(Mission.Current, this.AbilityAgent.Agent.Position.AsVec2, 10f);

                    // We don't even need this mesh, we could just work with the UnitMesh
                    Mesh cube = this.CreateMesh();

                    // I don't think we'll be moving the entity in the force push ability but we might for other abilities
                    // It might be the case that when we add the mesh, it squishes it
                    GameEntity entity = GameEntity.CreateEmptyDynamic(Mission.Current.Scene);
                    entity.AddMesh(cube);

                    // Set it to the agent's right hand's global position
                    MatrixFrame globalFrame = agent.AgentVisuals.GetGlobalFrame();
                    MatrixFrame boneEntitialFrameWithIndex = agent.AgentVisuals.GetSkeleton().GetBoneEntitialFrameWithIndex(agent.Monster.MainHandItemBoneIndex);
                    MatrixFrame matrixFrame = globalFrame.TransformToParent(boneEntitialFrameWithIndex);

                    // We leave it unattached to the agent entity
                    entity.SetGlobalFrame(new MatrixFrame(agent.LookRotation, matrixFrame.origin));

                    // Play around with this stuff. Don't know if we even need some of these
                    // Pretty sure we'll need to resize the physicsshape or we can use the entity's bounding box maybe
                    entity.SetBodyShape(PhysicsShape.GetFromResource("capsule"));
                    entity.AddPhysics(1, entity.CenterOfMass, entity.GetBodyShape(), agent.LookDirection * 10, agent.LookDirection * 10, PhysicsMaterial.GetFromName("boulder_stone"), false, 1);
                    entity.SetPhysicsState(true, true);
                    entity.EnableDynamicBody();

                    // https://discord.com/channels/411286129317249035/677511186295685150/1201805648988475394
                    // UsableMachine
                    // ScriptComponentBehavior

                    entity.CreateAndAddScriptComponent(ForcePushProjectile.Name);
                    entity.GetFirstScriptOfType<ForcePushProjectile>().Agent = agent;
                    entity.CallScriptCallbacks();

                    this.ActiveEntity = entity;
                    this.IsActive = true;
                    this.LastCheck = Mission.Current.CurrentTime;
                }
            }
        }
    }
}
