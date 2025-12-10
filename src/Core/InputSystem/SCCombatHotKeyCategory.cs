using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace SeparatistCrisis.InputSystem
{
    public sealed class SCCombatHotKeyCategory: GameKeyContext
    {
        public SCCombatHotKeyCategory() : base("SCCombatHotKeyCategory", 111, GameKeyContext.GameKeyContextType.Default)
        {
            this.RegisterHotKeys();
            this.RegisterGameKeys();
            this.RegisterGameAxisKeys();
        }

        private void RegisterHotKeys()
        {
            base.RegisterHotKey(new HotKey("DeploymentCameraIsActive", "SCCombatHotKeyCategory", InputKey.MiddleMouseButton, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ToggleZoom", "SCCombatHotKeyCategory", InputKey.ControllerRThumb, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipDropWeapon1", "SCCombatHotKeyCategory", InputKey.ControllerRRight, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipDropWeapon2", "SCCombatHotKeyCategory", InputKey.ControllerRUp, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipDropWeapon3", "SCCombatHotKeyCategory", InputKey.ControllerRLeft, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipDropWeapon4", "SCCombatHotKeyCategory", InputKey.ControllerRDown, HotKey.Modifiers.None, HotKey.Modifiers.None), true);

            base.RegisterHotKey(new HotKey("ControllerEquipAbility1", "SCCombatHotKeyCategory", InputKey.ControllerRRight, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipAbility2", "SCCombatHotKeyCategory", InputKey.ControllerRUp, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipAbility3", "SCCombatHotKeyCategory", InputKey.ControllerRLeft, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerEquipAbility4", "SCCombatHotKeyCategory", InputKey.ControllerRDown, HotKey.Modifiers.None, HotKey.Modifiers.None), true);

            base.RegisterHotKey(new HotKey("ControllerEquipDropExtraWeapon", "SCCombatHotKeyCategory", InputKey.ControllerRThumb, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkSelectFirstCategory", "SCCombatHotKeyCategory", new List<Key>
            {
                new Key(InputKey.LeftMouseButton),
                new Key(InputKey.ControllerRLeft)
            }, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkSelectSecondCategory", "SCCombatHotKeyCategory", new List<Key>
            {
                new Key(InputKey.RightMouseButton),
                new Key(InputKey.ControllerRRight)
            }, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkCloseMenu", "SCCombatHotKeyCategory", InputKey.ControllerRThumb, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkItem1", "SCCombatHotKeyCategory", InputKey.ControllerRUp, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkItem2", "SCCombatHotKeyCategory", InputKey.ControllerRRight, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkItem3", "SCCombatHotKeyCategory", InputKey.ControllerRDown, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("CheerBarkItem4", "SCCombatHotKeyCategory", InputKey.ControllerRLeft, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ForfeitSpawn", "SCCombatHotKeyCategory", new List<Key>
            {
                new Key(InputKey.X),
                new Key(InputKey.ControllerRLeft)
            }, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControlModeToggle", "SCCombatHotKeyCategory", InputKey.ControllerLDown, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerToggleWalk", "SCCombatHotKeyCategory", InputKey.ControllerRUp, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
            base.RegisterHotKey(new HotKey("ControllerToggleCrouch", "SCCombatHotKeyCategory", InputKey.ControllerRDown, HotKey.Modifiers.None, HotKey.Modifiers.None), true);
        }

        private void RegisterGameKeys()
        {
            base.RegisterGameKey(new GameKey(9, "Attack", "SCCombatHotKeyCategory", InputKey.LeftMouseButton, InputKey.ControllerRTrigger, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(10, "Defend", "SCCombatHotKeyCategory", InputKey.RightMouseButton, InputKey.ControllerLTrigger, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(11, "EquipPrimaryWeapon", "SCCombatHotKeyCategory", InputKey.MouseScrollUp, InputKey.Invalid, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(12, "EquipSecondaryWeapon", "SCCombatHotKeyCategory", InputKey.MouseScrollDown, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(13, "Action", "SCCombatHotKeyCategory", InputKey.F, InputKey.ControllerRUp, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(14, "Jump", "SCCombatHotKeyCategory", InputKey.Space, InputKey.ControllerRDown, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(15, "Crouch", "SCCombatHotKeyCategory", InputKey.Z, InputKey.ControllerLDown, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(16, "Kick", "SCCombatHotKeyCategory", InputKey.E, InputKey.ControllerRLeft, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(17, "ToggleWeaponMode", "SCCombatHotKeyCategory", InputKey.X, InputKey.Invalid, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(18, "EquipWeapon1", "SCCombatHotKeyCategory", InputKey.Numpad1, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(19, "EquipWeapon2", "SCCombatHotKeyCategory", InputKey.Numpad2, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(20, "EquipWeapon3", "SCCombatHotKeyCategory", InputKey.Numpad3, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(21, "EquipWeapon4", "SCCombatHotKeyCategory", InputKey.Numpad4, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(22, "DropWeapon", "SCCombatHotKeyCategory", InputKey.G, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(23, "SheathWeapon", "SCCombatHotKeyCategory", InputKey.BackSlash, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(24, "Zoom", "SCCombatHotKeyCategory", InputKey.LeftShift, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(25, "ViewCharacter", "SCCombatHotKeyCategory", InputKey.Tilde, InputKey.ControllerLLeft, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(26, "LockTarget", "SCCombatHotKeyCategory", InputKey.MiddleMouseButton, InputKey.ControllerRThumb, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(27, "CameraToggle", "SCCombatHotKeyCategory", InputKey.R, InputKey.ControllerLThumb, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(28, "MissionScreenHotkeyCameraZoomIn", "SCCombatHotKeyCategory", InputKey.NumpadPlus, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(29, "MissionScreenHotkeyCameraZoomOut", "SCCombatHotKeyCategory", InputKey.NumpadMinus, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(30, "ToggleWalkMode", "SCCombatHotKeyCategory", InputKey.CapsLock, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(31, "Cheer", "SCCombatHotKeyCategory", InputKey.O, InputKey.ControllerLUp, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(33, "PushToTalk", "SCCombatHotKeyCategory", InputKey.V, InputKey.ControllerLRight, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(34, "EquipmentSwitch", "SCCombatHotKeyCategory", InputKey.U, InputKey.ControllerRBumper, GameKeyMainCategories.ActionCategory), true);

            base.RegisterGameKey(new GameKey(37, "AbilitySwitch", "SCCombatHotKeyCategory", InputKey.C, InputKey.ControllerLBumper, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(38, "EquipAbility1", "SCCombatHotKeyCategory", InputKey.Numpad5, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(39, "EquipAbility2", "SCCombatHotKeyCategory", InputKey.Numpad6, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(40, "EquipAbility3", "SCCombatHotKeyCategory", InputKey.Numpad7, GameKeyMainCategories.ActionCategory), true);
            base.RegisterGameKey(new GameKey(41, "EquipAbility4", "SCCombatHotKeyCategory", InputKey.Numpad8, GameKeyMainCategories.ActionCategory), true);
        }

        private void RegisterGameAxisKeys()
        {
        }

        public const string CategoryId = "SCCombatHotKeyCategory";

        public const int MissionScreenHotkeyCameraZoomIn = 28;

        public const int MissionScreenHotkeyCameraZoomOut = 29;

        public const int Action = 13;

        public const int Jump = 14;

        public const int Crouch = 15;

        public const int Attack = 9;

        public const int Defend = 10;

        public const int Kick = 16;

        public const int ToggleWeaponMode = 17;

        public const int ToggleWalkMode = 30;

        public const int EquipWeapon1 = 18;

        public const int EquipWeapon2 = 19;

        public const int EquipWeapon3 = 20;

        public const int EquipWeapon4 = 21;

        public const int EquipPrimaryWeapon = 11;

        public const int EquipSecondaryWeapon = 12;

        public const int DropWeapon = 22;

        public const int SheathWeapon = 23;

        public const int Zoom = 24;

        public const int ViewCharacter = 25;

        public const int LockTarget = 26;

        public const int CameraToggle = 27;

        public const int Cheer = 31;

        public const int PushToTalk = 33;

        public const int EquipmentSwitch = 34;

        public const string DeploymentCameraIsActive = "DeploymentCameraIsActive";

        public const string ToggleZoom = "ToggleZoom";

        public const string ControllerEquipDropRRight = "ControllerEquipDropWeapon1";

        public const string ControllerEquipDropRUp = "ControllerEquipDropWeapon2";

        public const string ControllerEquipDropRLeft = "ControllerEquipDropWeapon3";

        public const string ControllerEquipDropRDown = "ControllerEquipDropWeapon4";

        public const string ControllerEquipDropRThumb = "ControllerEquipDropExtraWeapon";

        public const string CheerBarkSelectFirstCategory = "CheerBarkSelectFirstCategory";

        public const string CheerBarkSelectSecondCategory = "CheerBarkSelectSecondCategory";

        public const string CheerBarkCloseMenu = "CheerBarkCloseMenu";

        public const string CheerBarkItem1 = "CheerBarkItem1";

        public const string CheerBarkItem2 = "CheerBarkItem2";

        public const string CheerBarkItem3 = "CheerBarkItem3";

        public const string CheerBarkItem4 = "CheerBarkItem4";

        public const string ControlModeToggle = "ControlModeToggle";

        public const string ControllerToggleWalk = "ControllerToggleWalk";

        public const string ControllerToggleCrouch = "ControllerToggleCrouch";

        public const string ForfeitSpawn = "ForfeitSpawn";
    }
}
