using SandBox.ViewModelCollection.Input;
using SeparatistCrisis.CustomSandBox;
using SeparatistCrisis.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.View.CustomBattle;

namespace SeparatistCrisis.CustomSandBox
{
    public class SCCustomSandBoxMenuVM : ViewModel
    {
        private CustomSandBoxState _customSandBoxState;

        private InputKeyItemVM _startInputKey = null!;

        private InputKeyItemVM _cancelInputKey = null!;

        private InputKeyItemVM _resetInputKey = null!;

        private InputKeyItemVM _randomizeInputKey = null!;

        public InputKeyItemVM StartInputKey
        {
            get
            {
                return _startInputKey;
            }
            set
            {
                if (value != _startInputKey)
                {
                    _startInputKey = value;
                    OnPropertyChangedWithValue(value, "StartInputKey");
                }
            }
        }

        public InputKeyItemVM CancelInputKey
        {
            get
            {
                return _cancelInputKey;
            }
            set
            {
                if (value != _cancelInputKey)
                {
                    _cancelInputKey = value;
                    OnPropertyChangedWithValue(value, "CancelInputKey");
                }
            }
        }

        public InputKeyItemVM ResetInputKey
        {
            get
            {
                return _resetInputKey;
            }
            set
            {
                if (value != _resetInputKey)
                {
                    _resetInputKey = value;
                    OnPropertyChangedWithValue(value, "ResetInputKey");
                }
            }
        }

        public InputKeyItemVM RandomizeInputKey
        {
            get
            {
                return _randomizeInputKey;
            }
            set
            {
                if (value != _randomizeInputKey)
                {
                    _randomizeInputKey = value;
                    OnPropertyChangedWithValue(value, "RandomizeInputKey");
                }
            }
        }

        public SCCustomSandBoxMenuVM(CustomSandBoxState customSandBoxState)
        {
            this._customSandBoxState = customSandBoxState;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void SetActiveState(bool isActive)
        {
        }

        public void SetStartInputKey(HotKey hotkey)
        {
            this.StartInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetCancelInputKey(HotKey hotkey)
        {
            CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetResetInputKey(HotKey hotkey)
        {
            ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetRandomizeInputKey(HotKey hotkey)
        {
            RandomizeInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }
    }
}
