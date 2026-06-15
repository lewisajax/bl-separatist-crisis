using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.Scenes.ScriptComponents
{
    public class SCRotateSubMesh: ScriptComponentBehavior
    {
        private Mat3 _currentRotaton;

        [EditableScriptComponentVariable(true, "X")]
        private float _xRotation;

        [EditableScriptComponentVariable(true, "Y")]
        private float _yRotation;

        [EditableScriptComponentVariable(true, "Z")]
        private float _zRotation;

        [EditableScriptComponentVariable(true, "Mesh Tag")]
        private string _subMeshTag = "";

        public Mesh[]? Meshes { get; set; }

        public bool IsEntityVisible { get; set; }

        protected override void OnInit()
        {
            base.SetScriptComponentToTick(this.GetTickRequirement());
        }

        private void Rotate(float dt)
        {
            if (this.Meshes != null && this.Meshes.Length > 0)
            {
                Mesh baseMesh = this.Meshes[0].GetBaseMesh();

                this._currentRotaton.RotateAboutForward(this._yRotation * dt);
                this._currentRotaton.RotateAboutSide(this._xRotation * dt);
                this._currentRotaton.RotateAboutUp(this._zRotation * dt);

                //MatrixFrame frame = baseMesh.GetLocalFrame();
                //frame.rotation.RotateAboutForward(this._yRotation * dt);
                //frame.rotation.RotateAboutSide(this._xRotation * dt);
                //frame.rotation.RotateAboutUp(this._zRotation * dt);

                for (int i = 0; i < this.Meshes.Length; i++)
                {
                    this.Meshes[i].SetLocalFrame(new MatrixFrame(this._currentRotaton, baseMesh.GetLocalFrame().origin));
                }

                base.GameEntity.UpdateTriadFrameForEditorForAllChildren();
            }
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.TickParallel;
        }

        protected override void OnTickParallel(float dt)
        {
            if (this.IsEntityVisible)
            {
                this.Rotate(dt);
            }
        }

        protected override void OnEditorInit()
        {
            base.OnEditorInit();
            this.IsEntityVisible = true;
        }

        protected override void OnEditorTick(float dt)
        {
            base.OnEditorTick(dt);
            if (this.IsEntityVisible)
            {
                this.Rotate(dt);
            }
        }

        protected override void OnEditorVariableChanged(string variableName)
        {
            base.OnEditorVariableChanged(variableName);
            if (variableName == "Mesh Tag")
            {
                if (this.GameEntity != null)
                {
                    IEnumerable<Mesh> meshes = base.GameEntity.GetAllMeshesWithTag(this._subMeshTag);
                    this.Meshes = meshes.ToArray();

                    if (this.Meshes.Length > 0)
                        this._currentRotaton = this.Meshes[0].GetBaseMesh().GetLocalFrame().rotation.ToQuaternion().ToMat3();
                }
            }
        }
    }
}
