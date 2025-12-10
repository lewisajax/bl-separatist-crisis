using SandBox.ViewModelCollection.Input;
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

namespace SeparatistCrisis.CustomBattle
{
    public class SCCustomBattleMenuVM : ViewModel
    {
        private readonly ICustomBattleProvider _nextCustomBattleProvider;

        private CustomBattleState _customBattleState;

        private SCTroopSelectionPopUpVM _troopTypeSelectionPopUp = null!;

        private SCCustomBattleMenuSideVM _enemySide = null!;

        private SCCustomBattleMenuSideVM _playerSide = null!;

        private bool _isAttackerCustomMachineSelectionEnabled;

        private bool _isDefenderCustomMachineSelectionEnabled;

        private GameTypeSelectionGroupVM _gameTypeSelectionGroup = null!;

        private MapSelectionGroupVM _mapSelectionGroup = null!;

        private string _randomizeButtonText = null!;

        private string _backButtonText = null!;

        private string _startButtonText = null!;

        private string _titleText = null!;

        private string _switchButtonText;

        private bool _CanSwitchMode;

        private HintViewModel _switchHint;

        private MBBindingList<CustomBattleSiegeMachineVM> _attackerMeleeMachines = null!;

        private MBBindingList<CustomBattleSiegeMachineVM> _attackerRangedMachines = null!;

        private MBBindingList<CustomBattleSiegeMachineVM> _defenderMachines = null!;

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
        public bool CanSwitchMode
        {
            get => this._CanSwitchMode;
            set
            {
                if (value == this._CanSwitchMode)
                    return;
                this._CanSwitchMode = value;
                this.OnPropertyChangedWithValue(value, nameof(CanSwitchMode));
            }
        }

        [DataSourceProperty]
        public HintViewModel SwitchHint
        {
            get => this._switchHint;
            set
            {
                if (value == this._switchHint)
                    return;
                this._switchHint = value;
                this.OnPropertyChangedWithValue<HintViewModel>(value, nameof(SwitchHint));
            }
        }

        [DataSourceProperty]
        public SCTroopSelectionPopUpVM TroopTypeSelectionPopUp
        {
            get
            {
                return _troopTypeSelectionPopUp;
            }
            set
            {
                if (value != _troopTypeSelectionPopUp)
                {
                    _troopTypeSelectionPopUp = value;
                    OnPropertyChangedWithValue(value, "TroopTypeSelectionPopUp");
                }
            }
        }

        [DataSourceProperty]
        public bool IsAttackerCustomMachineSelectionEnabled
        {
            get
            {
                return _isAttackerCustomMachineSelectionEnabled;
            }
            set
            {
                if (value != _isAttackerCustomMachineSelectionEnabled)
                {
                    _isAttackerCustomMachineSelectionEnabled = value;
                    OnPropertyChangedWithValue(value, "IsAttackerCustomMachineSelectionEnabled");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDefenderCustomMachineSelectionEnabled
        {
            get
            {
                return _isDefenderCustomMachineSelectionEnabled;
            }
            set
            {
                if (value != _isDefenderCustomMachineSelectionEnabled)
                {
                    _isDefenderCustomMachineSelectionEnabled = value;
                    OnPropertyChangedWithValue(value, "IsDefenderCustomMachineSelectionEnabled");
                }
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
        public string TitleText
        {
            get
            {
                return _titleText;
            }
            set
            {
                if (value != _titleText)
                {
                    _titleText = value;
                    OnPropertyChangedWithValue(value, "TitleText");
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

        [DataSourceProperty]
        public SCCustomBattleMenuSideVM EnemySide
        {
            get
            {
                return _enemySide;
            }
            set
            {
                if (value != _enemySide)
                {
                    _enemySide = value;
                    OnPropertyChangedWithValue(value, "EnemySide");
                }
            }
        }

        [DataSourceProperty]
        public SCCustomBattleMenuSideVM PlayerSide
        {
            get
            {
                return _playerSide;
            }
            set
            {
                if (value != _playerSide)
                {
                    _playerSide = value;
                    OnPropertyChangedWithValue(value, "PlayerSide");
                }
            }
        }

        [DataSourceProperty]
        public GameTypeSelectionGroupVM GameTypeSelectionGroup
        {
            get
            {
                return _gameTypeSelectionGroup;
            }
            set
            {
                if (value != _gameTypeSelectionGroup)
                {
                    _gameTypeSelectionGroup = value;
                    OnPropertyChangedWithValue(value, "GameTypeSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public MapSelectionGroupVM MapSelectionGroup
        {
            get
            {
                return _mapSelectionGroup;
            }
            set
            {
                if (value != _mapSelectionGroup)
                {
                    _mapSelectionGroup = value;
                    OnPropertyChangedWithValue(value, "MapSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerMeleeMachines
        {
            get
            {
                return _attackerMeleeMachines;
            }
            set
            {
                if (value != _attackerMeleeMachines)
                {
                    _attackerMeleeMachines = value;
                    OnPropertyChangedWithValue(value, "AttackerMeleeMachines");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> AttackerRangedMachines
        {
            get
            {
                return _attackerRangedMachines;
            }
            set
            {
                if (value != _attackerRangedMachines)
                {
                    _attackerRangedMachines = value;
                    OnPropertyChangedWithValue(value, "AttackerRangedMachines");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomBattleSiegeMachineVM> DefenderMachines
        {
            get
            {
                return _defenderMachines;
            }
            set
            {
                if (value != _defenderMachines)
                {
                    _defenderMachines = value;
                    OnPropertyChangedWithValue(value, "DefenderMachines");
                }
            }
        }

        public SCCustomBattleMenuVM(CustomBattleState battleState)
        {
            _customBattleState = battleState;
            IsAttackerCustomMachineSelectionEnabled = false;
            TroopTypeSelectionPopUp = new SCTroopSelectionPopUpVM();
            PlayerSide = new SCCustomBattleMenuSideVM(new TextObject("{=BC7n6qxk}PLAYER", null), true, TroopTypeSelectionPopUp);
            EnemySide = new SCCustomBattleMenuSideVM(new TextObject("{=35IHscBa}ENEMY", null), false, TroopTypeSelectionPopUp);
            PlayerSide.OppositeSide = EnemySide;
            EnemySide.OppositeSide = PlayerSide;
            MapSelectionGroup = new MapSelectionGroupVM();
            GameTypeSelectionGroup = new GameTypeSelectionGroupVM(new Action<CustomBattlePlayerType>(OnPlayerTypeChange), new Action<string>(OnGameTypeChange));

            AttackerMeleeMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (int i = 0; i < 3; i++)
            {
                AttackerMeleeMachines.Add(new CustomBattleSiegeMachineVM(null, new Action<CustomBattleSiegeMachineVM>(OnMeleeMachineSelection), new Action<CustomBattleSiegeMachineVM>(OnResetMachineSelection)));
            }

            AttackerRangedMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (int j = 0; j < 4; j++)
            {
                AttackerRangedMachines.Add(new CustomBattleSiegeMachineVM(null, new Action<CustomBattleSiegeMachineVM>(OnAttackerRangedMachineSelection), new Action<CustomBattleSiegeMachineVM>(OnResetMachineSelection)));
            }

            DefenderMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
            for (int k = 0; k < 4; k++)
            {
                DefenderMachines.Add(new CustomBattleSiegeMachineVM(null, new Action<CustomBattleSiegeMachineVM>(OnDefenderRangedMachineSelection), new Action<CustomBattleSiegeMachineVM>(OnResetMachineSelection)));
            }

            this.CanSwitchMode = CustomBattleFactory.GetProviderCount() > 1;
            if (this.CanSwitchMode)
            {
                this._nextCustomBattleProvider = CustomBattleFactory.CollectNextProvider(typeof(CustomBattleProvider));
                this.SwitchHint = new HintViewModel(new TextObject("{=Jfe53wbr}Switch to {PROVIDER_NAME}").SetTextVariable("PROVIDER_NAME", this._nextCustomBattleProvider.GetName()));
            }

            RefreshValues();
            SetDefaultSiegeMachines();
        }

        private static CustomBattleCompositionData GetBattleCompositionDataFromCompositionGroup(SCArmyCompositionGroupVM compositionGroup)
        {
            return new CustomBattleCompositionData(compositionGroup.RangedInfantryComposition.CompositionValue / 100f, compositionGroup.MeleeCavalryComposition.CompositionValue / 100f, compositionGroup.RangedCavalryComposition.CompositionValue / 100f);
        }

        private static List<BasicCharacterObject>[] GetTroopSelections(SCArmyCompositionGroupVM armyComposition)
        {
            List<BasicCharacterObject>[] array = new List<BasicCharacterObject>[4];
            array[0] = (from x in armyComposition.MeleeInfantryComposition.TroopTypes
                        where x.IsSelected
                        select x.Character).ToList();
            array[1] = (from x in armyComposition.RangedInfantryComposition.TroopTypes
                        where x.IsSelected
                        select x.Character).ToList();
            array[2] = (from x in armyComposition.MeleeCavalryComposition.TroopTypes
                        where x.IsSelected
                        select x.Character).ToList();
            array[3] = (from x in armyComposition.RangedCavalryComposition.TroopTypes
                        where x.IsSelected
                        select x.Character).ToList();
            return array;
        }

        private static void FillSiegeMachines(List<MissionSiegeWeapon> machines, MBBindingList<CustomBattleSiegeMachineVM> vmMachines)
        {
            foreach (CustomBattleSiegeMachineVM customBattleSiegeMachineVM in vmMachines)
            {
                if (customBattleSiegeMachineVM.SiegeEngineType != null)
                {
                    machines.Add(MissionSiegeWeapon.CreateDefaultWeapon(customBattleSiegeMachineVM.SiegeEngineType));
                }
            }
        }

        private void SetDefaultSiegeMachines()
        {
            AttackerMeleeMachines[0].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
            AttackerMeleeMachines[1].SetMachineType(DefaultSiegeEngineTypes.Ram);
            AttackerMeleeMachines[2].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
            AttackerRangedMachines[0].SetMachineType(DefaultSiegeEngineTypes.Trebuchet);
            AttackerRangedMachines[1].SetMachineType(DefaultSiegeEngineTypes.Onager);
            AttackerRangedMachines[2].SetMachineType(DefaultSiegeEngineTypes.Onager);
            AttackerRangedMachines[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
            DefenderMachines[0].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
            DefenderMachines[1].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
            DefenderMachines[2].SetMachineType(DefaultSiegeEngineTypes.Catapult);
            DefenderMachines[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
        }

        public void SetActiveState(bool isActive)
        {
            if (isActive)
            {
                EnemySide.UpdateCharacterVisual();
                PlayerSide.UpdateCharacterVisual();
                return;
            }
            EnemySide.CurrentSelectedCharacter = null;
            PlayerSide.CurrentSelectedCharacter = null;
        }

        private void OnPlayerTypeChange(CustomBattlePlayerType playerType)
        {
            PlayerSide.OnPlayerTypeChange(playerType);
        }

        private void OnGameTypeChange(string gameTypeStringId)
        {
            MapSelectionGroup.OnGameTypeChange(gameTypeStringId);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            RandomizeButtonText = GameTexts.FindText("str_randomize", null).ToString();
            StartButtonText = GameTexts.FindText("str_start", null).ToString();
            BackButtonText = GameTexts.FindText("str_back", null).ToString();
            this.SwitchButtonText = GameTexts.FindText("str_switch").ToString();
            TitleText = GameTexts.FindText("str_custom_battle", null).ToString();
            EnemySide.RefreshValues();
            PlayerSide.RefreshValues();
            AttackerMeleeMachines.ApplyActionOnAllItems(delegate (CustomBattleSiegeMachineVM x)
            {
                x.RefreshValues();
            });
            AttackerRangedMachines.ApplyActionOnAllItems(delegate (CustomBattleSiegeMachineVM x)
            {
                x.RefreshValues();
            });
            DefenderMachines.ApplyActionOnAllItems(delegate (CustomBattleSiegeMachineVM x)
            {
                x.RefreshValues();
            });
            MapSelectionGroup.RefreshValues();
            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
            {
                return;
            }
            troopTypeSelectionPopUp.RefreshValues();
        }

        private void OnResetMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            selectedSlot.SetMachineType(null);
        }

        private void OnMeleeMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> list = new List<InquiryElement>();
            list.Add(new InquiryElement(null, GameTexts.FindText("str_empty", null).ToString(), null));
            foreach (SiegeEngineType siegeEngineType in CustomBattleData.GetAllAttackerMeleeMachines())
            {
                list.Add(new InquiryElement(siegeEngineType, siegeEngineType.Name.ToString(), null));
            }
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=MVOWsP48}Select a Melee Machine", null).ToString(), string.Empty, list, true, 1, 1, GameTexts.FindText("str_done", null).ToString(), "", delegate (List<InquiryElement> selectedElements)
            {
                CustomBattleSiegeMachineVM selectedSlot2 = selectedSlot;
                InquiryElement? inquiryElement = selectedElements.FirstOrDefault();
                selectedSlot2.SetMachineType((inquiryElement != null ? inquiryElement.Identifier : null) as SiegeEngineType);
            }, null, "", false), false, false);
        }

        private void OnAttackerRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> list = new List<InquiryElement>();
            list.Add(new InquiryElement(null, GameTexts.FindText("str_empty", null).ToString(), null));
            foreach (SiegeEngineType siegeEngineType in CustomBattleData.GetAllAttackerRangedMachines())
            {
                list.Add(new InquiryElement(siegeEngineType, siegeEngineType.Name.ToString(), null));
            }
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine", null).ToString(), string.Empty, list, true, 1, 1, GameTexts.FindText("str_done", null).ToString(), "", delegate (List<InquiryElement> selectedElements)
            {
                CustomBattleSiegeMachineVM selectedSlot2 = selectedSlot;
                InquiryElement? inquiryElement = selectedElements.FirstOrDefault();
                selectedSlot2.SetMachineType((inquiryElement != null ? inquiryElement.Identifier : null) as SiegeEngineType);
            }, null, "", false), false, false);
        }

        private void OnDefenderRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
        {
            List<InquiryElement> list = new List<InquiryElement>();
            list.Add(new InquiryElement(null, GameTexts.FindText("str_empty", null).ToString(), null));
            foreach (SiegeEngineType siegeEngineType in CustomBattleData.GetAllDefenderRangedMachines())
            {
                list.Add(new InquiryElement(siegeEngineType, siegeEngineType.Name.ToString(), null));
            }
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=SLZzfNPr}Select a Ranged Machine", null).ToString(), string.Empty, list, true, 1, 1, GameTexts.FindText("str_done", null).ToString(), "", delegate (List<InquiryElement> selectedElements)
            {
                CustomBattleSiegeMachineVM selectedSlot2 = selectedSlot;
                InquiryElement? inquiryElement = selectedElements.FirstOrDefault();
                selectedSlot2.SetMachineType((inquiryElement != null ? inquiryElement.Identifier : null) as SiegeEngineType);
            }, null, "", false), false, false);
        }

        private void ExecuteRandomizeAttackerSiegeEngines()
        {
            MBList<SiegeEngineType?> mblist = new MBList<SiegeEngineType?>();
            mblist.AddRange(CustomBattleData.GetAllAttackerMeleeMachines());
            mblist.Add(null);
            foreach (CustomBattleSiegeMachineVM customBattleSiegeMachineVM in _attackerMeleeMachines)
            {
                customBattleSiegeMachineVM.SetMachineType(mblist.GetRandomElement());
            }
            mblist.Clear();
            mblist.AddRange(CustomBattleData.GetAllAttackerRangedMachines());
            mblist.Add(null);
            foreach (CustomBattleSiegeMachineVM customBattleSiegeMachineVM2 in _attackerRangedMachines)
            {
                customBattleSiegeMachineVM2.SetMachineType(mblist.GetRandomElement());
            }
        }

        private void ExecuteRandomizeDefenderSiegeEngines()
        {
            MBList<SiegeEngineType?> mblist = new MBList<SiegeEngineType?>();
            mblist.AddRange(CustomBattleData.GetAllDefenderRangedMachines());
            mblist.Add(null);
            foreach (CustomBattleSiegeMachineVM customBattleSiegeMachineVM in _defenderMachines)
            {
                customBattleSiegeMachineVM.SetMachineType(mblist.GetRandomElement());
            }
        }

        public void ExecuteBack()
        {
            Debug.Print("EXECUTE BACK - PRESSED", color: Debug.DebugColor.Green);
            Game.Current.GameStateManager.PopState();
        }

        private CustomBattleData PrepareBattleData()
        {
            BasicCharacterObject selectedCharacter = PlayerSide.SelectedCharacter;
            BasicCharacterObject selectedCharacter2 = EnemySide.SelectedCharacter;
            int num = PlayerSide.CompositionGroup.ArmySize;
            int armySize = EnemySide.CompositionGroup.ArmySize;
            bool isPlayerAttacker = GameTypeSelectionGroup.SelectedPlayerSide == CustomBattlePlayerSide.Attacker;
            bool flag = GameTypeSelectionGroup.SelectedPlayerType == CustomBattlePlayerType.Commander;
            BasicCharacterObject? playerSideGeneralCharacter = null;

            if (!flag)
            {
                MBList<BasicCharacterObject> mblist = CustomBattleData.Characters.ToMBList();
                mblist.Remove(selectedCharacter);
                mblist.Remove(selectedCharacter2);
                playerSideGeneralCharacter = mblist.GetRandomElement();
                num--;
            }

            int[] troopCounts = CustomBattleHelper.GetTroopCounts(num, GetBattleCompositionDataFromCompositionGroup(PlayerSide.CompositionGroup));
            int[] troopCounts2 = CustomBattleHelper.GetTroopCounts(armySize, GetBattleCompositionDataFromCompositionGroup(EnemySide.CompositionGroup));
            
            List<BasicCharacterObject>[] troopSelections = GetTroopSelections(PlayerSide.CompositionGroup);
            List<BasicCharacterObject>[] troopSelections2 = GetTroopSelections(EnemySide.CompositionGroup);
            BasicCultureObject faction = PlayerSide.FactionSelectionGroup.SelectedItem.Faction;
            BasicCultureObject faction2 = EnemySide.FactionSelectionGroup.SelectedItem.Faction;

            CustomBattleCombatant[] customBattleParties = CustomBattleHelper.GetCustomBattleParties(selectedCharacter, playerSideGeneralCharacter, 
                selectedCharacter2, faction, troopCounts, troopSelections, faction2, troopCounts2, troopSelections2, isPlayerAttacker);

            List<MissionSiegeWeapon>? list = null;
            List<MissionSiegeWeapon>? list2 = null;
            float[]? wallHitPointsPercentages = null;

            if (this.GameTypeSelectionGroup.SelectedGameTypeString == "Siege")
            {
                list = new List<MissionSiegeWeapon>();
                list2 = new List<MissionSiegeWeapon>();
                FillSiegeMachines(list, _attackerMeleeMachines);
                FillSiegeMachines(list, _attackerRangedMachines);
                FillSiegeMachines(list2, _defenderMachines);
                wallHitPointsPercentages = CustomBattleHelper.GetWallHitpointPercentages(MapSelectionGroup.SelectedWallBreachedCount);
            }

            return CustomBattleHelper.PrepareBattleData(selectedCharacter, playerSideGeneralCharacter, 
                customBattleParties[0], customBattleParties[1], GameTypeSelectionGroup.SelectedPlayerSide, 
                GameTypeSelectionGroup.SelectedPlayerType, this.GameTypeSelectionGroup.SelectedGameTypeString, 
                MapSelectionGroup.SelectedMap.MapId, MapSelectionGroup.SelectedSeasonId, 
                MapSelectionGroup.SelectedTimeOfDay, list, list2, wallHitPointsPercentages, 
                MapSelectionGroup.SelectedSceneLevel, MapSelectionGroup.IsSallyOutSelected);
        }

        public void ExecuteStart()
        {
            SCCustomBattles.StartGame(PrepareBattleData());
            Debug.Print("EXECUTE START - PRESSED", 0, Debug.DebugColor.Green, 17592186044416UL);
        }

        public void ExecuteRandomize()
        {
            GameTypeSelectionGroup.RandomizeAll();
            MapSelectionGroup.RandomizeAll();
            PlayerSide.Randomize();
            EnemySide.Randomize();
            ExecuteRandomizeAttackerSiegeEngines();
            ExecuteRandomizeDefenderSiegeEngines();
            Debug.Print("EXECUTE RANDOMIZE - PRESSED", 0, Debug.DebugColor.Green, 17592186044416UL);
        }

        private void ExecuteDoneDefenderCustomMachineSelection()
        {
            IsDefenderCustomMachineSelectionEnabled = false;
        }

        private void ExecuteDoneAttackerCustomMachineSelection()
        {
            IsAttackerCustomMachineSelectionEnabled = false;
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            StartInputKey.OnFinalize();
            CancelInputKey.OnFinalize();
            ResetInputKey.OnFinalize();
            RandomizeInputKey.OnFinalize();
            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
            {
                return;
            }
            troopTypeSelectionPopUp.OnFinalize();
        }

        public void ExecuteSwitchToNextCustomBattle()
        {
            if (!this.CanSwitchMode)
                return;
            this.ExecuteBack();
            GameStateManager.Current = Module.CurrentModule.GlobalGameStateManager;
            this._nextCustomBattleProvider.StartCustomBattle();
        }

        public void SetStartInputKey(HotKey hotkey)
        {
            StartInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }

        public void SetCancelInputKey(HotKey hotkey)
        {
            CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
            {
                return;
            }
            troopTypeSelectionPopUp.SetCancelInputKey(hotkey);
        }

        public void SetResetInputKey(HotKey hotkey)
        {
            ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
            {
                return;
            }
            troopTypeSelectionPopUp.SetResetInputKey(hotkey);
        }

        public void SetRandomizeInputKey(HotKey hotkey)
        {
            RandomizeInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
        }
    }
}
