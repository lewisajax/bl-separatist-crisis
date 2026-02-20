using HarmonyLib;
using SandBox.ViewModelCollection.Input;
using SeparatistCrisis.CustomSandBox;
using SeparatistCrisis.MissionManagers;
using SeparatistCrisis.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.View.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.CustomSandBox
{
    public class SCCustomSandBoxMenuVM : ViewModel
    {
        private CustomSandBoxState _customSandBoxState;

        private InputKeyItemVM _startInputKey = null!;

        private InputKeyItemVM _cancelInputKey = null!;

        private InputKeyItemVM _resetInputKey = null!;

        private InputKeyItemVM _randomizeInputKey = null!;

        private string _randomizeButtonText = null!;

        private string _backButtonText = null!;

        private string _startButtonText = null!;

        private string _switchButtonText;

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

        [DataSourceProperty]
        public string SwitchButtonText
        {
            get => this._switchButtonText;
            set
            {
                if (!(value != this._switchButtonText))
                    return;
                this._switchButtonText = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(SwitchButtonText));
            }
        }

        [DataSourceProperty]
        public string RandomizeButtonText
        {
            get
            {
                return _randomizeButtonText;
            }
            set
            {
                if (value != _randomizeButtonText)
                {
                    _randomizeButtonText = value;
                    OnPropertyChangedWithValue(value, "RandomizeButtonText");
                }
            }
        }

        [DataSourceProperty]
        public string BackButtonText
        {
            get
            {
                return _backButtonText;
            }
            set
            {
                if (value != _backButtonText)
                {
                    _backButtonText = value;
                    OnPropertyChangedWithValue(value, "BackButtonText");
                }
            }
        }

        [DataSourceProperty]
        public string StartButtonText
        {
            get
            {
                return _startButtonText;
            }
            set
            {
                if (value != _startButtonText)
                {
                    _startButtonText = value;
                    OnPropertyChangedWithValue(value, "StartButtonText");
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
            this.RandomizeButtonText = GameTexts.FindText("str_randomize", null).ToString();
            this.StartButtonText = GameTexts.FindText("str_start", null).ToString();
            this.BackButtonText = GameTexts.FindText("str_back", null).ToString();
            this.SwitchButtonText = GameTexts.FindText("str_switch", null).ToString();
        }

        public void ExecuteStart()
        {
            this.PrepareBattleData();
            Debug.Print("EXECUTE START - PRESSED", 0, Debug.DebugColor.Green, 17592186044416UL);
        }

        private void PrepareBattleData()
        {
            Settlement hideout = Settlement.All.Find((Settlement settlement) => settlement.IsHideout);
            //MethodInfo eventInit =  AccessTools.Method(typeof(MapEvent), "Initialize", new Type[] { typeof(PartyBase), typeof(PartyBase), typeof(MapEventComponent), typeof(MapEvent.BattleTypes) });
            //HideoutEventComponent component = HideoutEventComponent.CreateHideoutEvent(PartyBase.MainParty, hideout.Party, )

            //MapEvent mapEvent = (MapEvent)eventInit.Invoke(null, new object[] { PartyBase.MainParty, hideout.Party, null, MapEvent.BattleTypes.Hideout });
            //((Campaign)Game.Current.GameType).CampaignMissionManager.OpenHideoutBattleMission("desert_hideout_004_sv", 
            //    PartyBase.MainParty.MemberRoster.ToFlattenedRoster(), false);

            EncounterManager.StartSettlementEncounter(MobileParty.MainParty, hideout);
            ((Campaign)Game.Current.GameType).CampaignMissionManager.OpenHideoutBattleMission("desert_hideout_004_sv",
                PartyBase.MainParty.MemberRoster.ToFlattenedRoster(), false);
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
