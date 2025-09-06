using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace SeparatistCrisis.CustomBattle
{
    public class SCArmyCompositionItemVM : ViewModel
    {
        public enum CompositionType
        {
            MeleeInfantry,
            RangedInfantry,
            MeleeCavalry,
            RangedCavalry
        }

        private readonly MBReadOnlyList<SkillObject> _allSkills;

        private readonly List<BasicCharacterObject> _allCharacterObjects;

        private readonly Action<int, int> _onCompositionValueChanged;

        private readonly SCTroopSelectionPopUpVM _troopTypeSelectionPopUp;

        private BasicCultureObject? _culture;

        private readonly StringItemWithHintVM _typeIconData;

        private readonly CompositionType _type;

        private readonly int[] _compositionValues;

        private MBBindingList<CustomBattleTroopTypeVM> _troopTypes = null!;

        private HintViewModel _invalidHint = null!;

        private HintViewModel _addTroopTypeHint = null!;

        private bool _isLocked;

        private bool _isValid;

        public Action? OnDone { get; set; }

        public SCTroopSelectionItemListVM? ItemListVM { get; private set; }

        [DataSourceProperty]
        public MBBindingList<CustomBattleTroopTypeVM> TroopTypes
        {
            get
            {
                return _troopTypes;
            }
            set
            {
                if (value != _troopTypes)
                {
                    _troopTypes = value;
                    OnPropertyChangedWithValue(value, "TroopTypes");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel InvalidHint
        {
            get
            {
                return _invalidHint;
            }
            set
            {
                if (value != _invalidHint)
                {
                    _invalidHint = value;
                    OnPropertyChangedWithValue(value, "InvalidHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel AddTroopTypeHint
        {
            get
            {
                return _addTroopTypeHint;
            }
            set
            {
                if (value != _addTroopTypeHint)
                {
                    _addTroopTypeHint = value;
                    OnPropertyChangedWithValue(value, "AddTroopTypeHint");
                }
            }
        }

        [DataSourceProperty]
        public bool IsLocked
        {
            get
            {
                return _isLocked;
            }
            set
            {
                if (value != _isLocked)
                {
                    _isLocked = value;
                    OnPropertyChangedWithValue(value, "IsLocked");
                }
            }
        }

        [DataSourceProperty]
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    OnPropertyChangedWithValue(value, "IsValid");
                }
                OnValidityChanged(value);
            }
        }

        [DataSourceProperty]
        public int CompositionValue
        {
            get
            {
                return _compositionValues[(int)_type];
            }
            set
            {
                if (value != _compositionValues[(int)_type])
                {
                    _onCompositionValueChanged(value, (int)_type);
                }
            }
        }

        public SCArmyCompositionItemVM(CompositionType type, List<BasicCharacterObject> allCharacterObjects, MBReadOnlyList<SkillObject> allSkills, Action<int, int> onCompositionValueChanged, Action onPopUpDone, SCTroopSelectionPopUpVM troopTypeSelectionPopUp, SCTroopSelectionItemListVM? itemList, int[] compositionValues)
        {
            _allCharacterObjects = allCharacterObjects;
            _allSkills = allSkills;
            _onCompositionValueChanged = onCompositionValueChanged;
            _troopTypeSelectionPopUp = troopTypeSelectionPopUp;
            _type = type;
            _compositionValues = compositionValues;
            _typeIconData = GetTroopTypeIconData(type, false);
            TroopTypes = new MBBindingList<CustomBattleTroopTypeVM>();
            InvalidHint = new HintViewModel(new TextObject("{=iSQTtNUD}This faction doesn't have this troop type.", null), null);
            AddTroopTypeHint = new HintViewModel(new TextObject("{=eMbuGGus}Select troops to spawn in formation.", null), null);
            ItemListVM = itemList;
            OnDone = onPopUpDone;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void SetCurrentSelectedCulture(BasicCultureObject culture)
        {
            IsLocked = false;
            _culture = culture;
            PopulateTroopTypes();
        }

        public void ExecuteRandomize(int compositionValue)
        {
            IsValid = true;
            IsLocked = false;
            CompositionValue = compositionValue;
            IsValid = TroopTypes.Count > 0;
            TroopTypes.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.ExecuteRandomize();
            });
            if (!TroopTypes.Any((x) => x.IsSelected) && IsValid)
            {
                TroopTypes[0].IsSelected = true;
            }
        }

        // This is called when we click the '+' button under each slider.
        public void ExecuteAddTroopTypes()
        {
            string title = GameTexts.FindText("str_custom_battle_choose_troop", _type.ToString()).ToString();

            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = _troopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
                return;

            ResetFilters(ItemListVM);

            // Since there's always 2 instances of ArmyCompositionGroupVMs but only 1 popup instance, we inject references to the popup through the individual slider buttons.
            troopTypeSelectionPopUp.ItemList = ItemListVM;
            troopTypeSelectionPopUp.OnDone = OnDone;
            troopTypeSelectionPopUp.OpenPopUp(title);
        }

        // When we click on the infantry/ranged/cav slider's add button, we set the relevant toggles and clear any other filters on start.
        public void ResetFilters(SCTroopSelectionItemListVM? itemList)
        {
            TextObject compName;
            switch(_type)
            {
                case CompositionType.MeleeCavalry:
                    compName = SCTroopSelectionItemListVM.FilterClassCavalary;
                    break;
                case CompositionType.RangedCavalry:
                    compName = SCTroopSelectionItemListVM.FilterClassMountedArcher;
                    break;
                case CompositionType.RangedInfantry:
                    compName = SCTroopSelectionItemListVM.FilterClassRanged;
                    break;
                case CompositionType.MeleeInfantry:
                default:
                    compName = SCTroopSelectionItemListVM.FilterClassInfantry;
                    break;
            }

            if (_culture == null)
                return; // Not a big deal if we cant reset the filters

            IEnumerable<TextObject> filterNames = new TextObject[3]
            {
                SCTroopSelectionItemListVM.FilterTypeSoldier,
                _culture.Name,
                compName
            };

            itemList?.ReCheckFilters(filterNames);
        }

        public void RefreshCompositionValue()
        {
            OnPropertyChanged("CompositionValue");
        }

        private void OnValidityChanged(bool value)
        {
            IsLocked = false;
            if (!value)
            {
                CompositionValue = 0;
            }
            IsLocked = !value;
        }

        private void PopulateTroopTypes()
        {
            TroopTypes.Clear();
            MBReadOnlyList<BasicCharacterObject> defaultCharacters = GetDefaultCharacters();
            foreach (BasicCharacterObject basicCharacterObject in _allCharacterObjects)
            {
                if (IsValidUnitItem(basicCharacterObject))
                {
                    TroopTypes.Add(new CustomBattleTroopTypeVM(basicCharacterObject, new Action<CustomBattleTroopTypeVM>(_troopTypeSelectionPopUp.OnItemSelectionToggled), _typeIconData, _allSkills, defaultCharacters.Contains(basicCharacterObject)));
                }
            }
            IsValid = TroopTypes.Count > 0;
            if (IsValid)
            {
                if (!TroopTypes.Any((x) => x.IsDefault))
                {
                    TroopTypes[0].IsDefault = true;
                }
            }
            TroopTypes.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.IsSelected = x.IsDefault;
            });
        }

        private bool IsValidUnitItem(BasicCharacterObject o)
        {
            if (o == null || _culture != o.Culture)
            {
                return false;
            }
            switch (_type)
            {
                case CompositionType.MeleeInfantry:
                    return o.DefaultFormationClass == FormationClass.Infantry || o.DefaultFormationClass == FormationClass.HeavyInfantry;
                case CompositionType.RangedInfantry:
                    return o.DefaultFormationClass == FormationClass.Ranged;
                case CompositionType.MeleeCavalry:
                    return o.DefaultFormationClass == FormationClass.Cavalry || o.DefaultFormationClass == FormationClass.HeavyCavalry || o.DefaultFormationClass == FormationClass.LightCavalry;
                case CompositionType.RangedCavalry:
                    return o.DefaultFormationClass == FormationClass.HorseArcher;
                default:
                    return false;
            }
        }

        private MBReadOnlyList<BasicCharacterObject> GetDefaultCharacters()
        {
            MBList<BasicCharacterObject> mblist = new MBList<BasicCharacterObject>();
            FormationClass formation = FormationClass.NumberOfAllFormations;
            switch (_type)
            {
                case CompositionType.MeleeInfantry:
                    formation = FormationClass.Infantry;
                    break;
                case CompositionType.RangedInfantry:
                    formation = FormationClass.Ranged;
                    break;
                case CompositionType.MeleeCavalry:
                    formation = FormationClass.Cavalry;
                    break;
                case CompositionType.RangedCavalry:
                    formation = FormationClass.HorseArcher;
                    break;
            }
            mblist.Add(CustomBattleHelper.GetDefaultTroopOfFormationForFaction(_culture, formation));
            return mblist;
        }

        public static StringItemWithHintVM GetTroopTypeIconData(CompositionType type, bool isBig = false)
        {
            TextObject textObject = TextObject.Empty;
            string str;
            switch (type)
            {
                case CompositionType.MeleeInfantry:
                    str = isBig ? "infantry_big" : "infantry";
                    textObject = GameTexts.FindText("str_troop_type_name", "Infantry");
                    break;
                case CompositionType.RangedInfantry:
                    str = isBig ? "bow_big" : "bow";
                    textObject = GameTexts.FindText("str_troop_type_name", "Ranged");
                    break;
                case CompositionType.MeleeCavalry:
                    str = isBig ? "cavalry_big" : "cavalry";
                    textObject = GameTexts.FindText("str_troop_type_name", "Cavalry");
                    break;
                case CompositionType.RangedCavalry:
                    str = isBig ? "horse_archer_big" : "horse_archer";
                    textObject = GameTexts.FindText("str_troop_type_name", "HorseArcher");
                    break;
                default:
                    return new StringItemWithHintVM("", TextObject.Empty);
            }
            return new StringItemWithHintVM("General\\TroopTypeIcons\\icon_troop_type_" + str, new TextObject("{=!}" + textObject.ToString(), null));
        }
    }
}
