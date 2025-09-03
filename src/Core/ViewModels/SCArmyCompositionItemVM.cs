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

namespace SeparatistCrisis.ViewModels
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

        private readonly SCArmyCompositionItemVM.CompositionType _type;

        private readonly int[] _compositionValues;

        private MBBindingList<CustomBattleTroopTypeVM> _troopTypes = null!;

        private HintViewModel _invalidHint = null!;

        private HintViewModel _addTroopTypeHint = null!;

        private bool _isLocked;

        private bool _isValid;

        public SCTroopSelectionItemListVM? ItemListVM { get; private set; }

        [DataSourceProperty]
        public MBBindingList<CustomBattleTroopTypeVM> TroopTypes
        {
            get
            {
                return this._troopTypes;
            }
            set
            {
                if (value != this._troopTypes)
                {
                    this._troopTypes = value;
                    base.OnPropertyChangedWithValue<MBBindingList<CustomBattleTroopTypeVM>>(value, "TroopTypes");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel InvalidHint
        {
            get
            {
                return this._invalidHint;
            }
            set
            {
                if (value != this._invalidHint)
                {
                    this._invalidHint = value;
                    base.OnPropertyChangedWithValue<HintViewModel>(value, "InvalidHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel AddTroopTypeHint
        {
            get
            {
                return this._addTroopTypeHint;
            }
            set
            {
                if (value != this._addTroopTypeHint)
                {
                    this._addTroopTypeHint = value;
                    base.OnPropertyChangedWithValue<HintViewModel>(value, "AddTroopTypeHint");
                }
            }
        }

        [DataSourceProperty]
        public bool IsLocked
        {
            get
            {
                return this._isLocked;
            }
            set
            {
                if (value != this._isLocked)
                {
                    this._isLocked = value;
                    base.OnPropertyChangedWithValue(value, "IsLocked");
                }
            }
        }

        [DataSourceProperty]
        public bool IsValid
        {
            get
            {
                return this._isValid;
            }
            set
            {
                if (value != this._isValid)
                {
                    this._isValid = value;
                    base.OnPropertyChangedWithValue(value, "IsValid");
                }
                this.OnValidityChanged(value);
            }
        }

        [DataSourceProperty]
        public int CompositionValue
        {
            get
            {
                return this._compositionValues[(int)this._type];
            }
            set
            {
                if (value != this._compositionValues[(int)this._type])
                {
                    this._onCompositionValueChanged(value, (int)this._type);
                }
            }
        }

        public SCArmyCompositionItemVM(SCArmyCompositionItemVM.CompositionType type, List<BasicCharacterObject> allCharacterObjects, MBReadOnlyList<SkillObject> allSkills, Action<int, int> onCompositionValueChanged, SCTroopSelectionPopUpVM troopTypeSelectionPopUp, SCTroopSelectionItemListVM? itemList, int[] compositionValues)
        {
            this._allCharacterObjects = allCharacterObjects;
            this._allSkills = allSkills;
            this._onCompositionValueChanged = onCompositionValueChanged;
            this._troopTypeSelectionPopUp = troopTypeSelectionPopUp;
            this._type = type;
            this._compositionValues = compositionValues;
            this._typeIconData = SCArmyCompositionItemVM.GetTroopTypeIconData(type, false);
            this.TroopTypes = new MBBindingList<CustomBattleTroopTypeVM>();
            this.InvalidHint = new HintViewModel(new TextObject("{=iSQTtNUD}This faction doesn't have this troop type.", null), null);
            this.AddTroopTypeHint = new HintViewModel(new TextObject("{=eMbuGGus}Select troops to spawn in formation.", null), null);
            this.ItemListVM = itemList;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void SetCurrentSelectedCulture(BasicCultureObject culture)
        {
            this.IsLocked = false;
            this._culture = culture;
            this.PopulateTroopTypes();
        }

        public void ExecuteRandomize(int compositionValue)
        {
            this.IsValid = true;
            this.IsLocked = false;
            this.CompositionValue = compositionValue;
            this.IsValid = (this.TroopTypes.Count > 0);
            this.TroopTypes.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.ExecuteRandomize();
            });
            if (!this.TroopTypes.Any((CustomBattleTroopTypeVM x) => x.IsSelected) && this.IsValid)
            {
                this.TroopTypes[0].IsSelected = true;
            }
        }

        public void ExecuteAddTroopTypes()
        {
            string title = GameTexts.FindText("str_custom_battle_choose_troop", this._type.ToString()).ToString();
            SCTroopSelectionPopUpVM troopTypeSelectionPopUp = this._troopTypeSelectionPopUp;
            if (troopTypeSelectionPopUp == null)
            {
                return;
            }
            this.ResetFilters(this.ItemListVM);
            troopTypeSelectionPopUp.ItemList = this.ItemListVM;
            troopTypeSelectionPopUp.OpenPopUp(title, this.TroopTypes);
        }

        public void ResetFilters(SCTroopSelectionItemListVM? itemList)
        {
            TextObject compName;
            switch(this._type)
            {
                case SCArmyCompositionItemVM.CompositionType.MeleeCavalry:
                    compName = SCTroopSelectionItemListVM.FilterClassCavalary;
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedCavalry:
                    compName = SCTroopSelectionItemListVM.FilterClassMountedArcher;
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedInfantry:
                    compName = SCTroopSelectionItemListVM.FilterClassRanged;
                    break;
                case SCArmyCompositionItemVM.CompositionType.MeleeInfantry:
                default:
                    compName = SCTroopSelectionItemListVM.FilterClassInfantry;
                    break;
            }

            if (this._culture == null)
                return; // Not a big deal if we cant reset the filters

            IEnumerable<TextObject> filterNames = new TextObject[3]
            {
                SCTroopSelectionItemListVM.FilterTypeSoldier,
                this._culture.Name,
                compName
            };

            itemList?.ReCheckFilters(filterNames);
        }

        public void RefreshCompositionValue()
        {
            base.OnPropertyChanged("CompositionValue");
        }

        private void OnValidityChanged(bool value)
        {
            this.IsLocked = false;
            if (!value)
            {
                this.CompositionValue = 0;
            }
            this.IsLocked = !value;
        }

        private void PopulateTroopTypes()
        {
            this.TroopTypes.Clear();
            MBReadOnlyList<BasicCharacterObject> defaultCharacters = this.GetDefaultCharacters();
            foreach (BasicCharacterObject basicCharacterObject in this._allCharacterObjects)
            {
                if (this.IsValidUnitItem(basicCharacterObject))
                {
                    this.TroopTypes.Add(new CustomBattleTroopTypeVM(basicCharacterObject, new Action<CustomBattleTroopTypeVM>(this._troopTypeSelectionPopUp.OnItemSelectionToggled), this._typeIconData, this._allSkills, defaultCharacters.Contains(basicCharacterObject)));
                }
            }
            this.IsValid = (this.TroopTypes.Count > 0);
            if (this.IsValid)
            {
                if (!this.TroopTypes.Any((CustomBattleTroopTypeVM x) => x.IsDefault))
                {
                    this.TroopTypes[0].IsDefault = true;
                }
            }
            this.TroopTypes.ApplyActionOnAllItems(delegate (CustomBattleTroopTypeVM x)
            {
                x.IsSelected = x.IsDefault;
            });
        }

        private bool IsValidUnitItem(BasicCharacterObject o)
        {
            if (o == null || this._culture != o.Culture)
            {
                return false;
            }
            switch (this._type)
            {
                case SCArmyCompositionItemVM.CompositionType.MeleeInfantry:
                    return o.DefaultFormationClass == FormationClass.Infantry || o.DefaultFormationClass == FormationClass.HeavyInfantry;
                case SCArmyCompositionItemVM.CompositionType.RangedInfantry:
                    return o.DefaultFormationClass == FormationClass.Ranged;
                case SCArmyCompositionItemVM.CompositionType.MeleeCavalry:
                    return o.DefaultFormationClass == FormationClass.Cavalry || o.DefaultFormationClass == FormationClass.HeavyCavalry || o.DefaultFormationClass == FormationClass.LightCavalry;
                case SCArmyCompositionItemVM.CompositionType.RangedCavalry:
                    return o.DefaultFormationClass == FormationClass.HorseArcher;
                default:
                    return false;
            }
        }

        private MBReadOnlyList<BasicCharacterObject> GetDefaultCharacters()
        {
            MBList<BasicCharacterObject> mblist = new MBList<BasicCharacterObject>();
            FormationClass formation = FormationClass.NumberOfAllFormations;
            switch (this._type)
            {
                case SCArmyCompositionItemVM.CompositionType.MeleeInfantry:
                    formation = FormationClass.Infantry;
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedInfantry:
                    formation = FormationClass.Ranged;
                    break;
                case SCArmyCompositionItemVM.CompositionType.MeleeCavalry:
                    formation = FormationClass.Cavalry;
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedCavalry:
                    formation = FormationClass.HorseArcher;
                    break;
            }
            mblist.Add(CustomBattleHelper.GetDefaultTroopOfFormationForFaction(this._culture, formation));
            return mblist;
        }

        public static StringItemWithHintVM GetTroopTypeIconData(SCArmyCompositionItemVM.CompositionType type, bool isBig = false)
        {
            TextObject textObject = TextObject.Empty;
            string str;
            switch (type)
            {
                case SCArmyCompositionItemVM.CompositionType.MeleeInfantry:
                    str = (isBig ? "infantry_big" : "infantry");
                    textObject = GameTexts.FindText("str_troop_type_name", "Infantry");
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedInfantry:
                    str = (isBig ? "bow_big" : "bow");
                    textObject = GameTexts.FindText("str_troop_type_name", "Ranged");
                    break;
                case SCArmyCompositionItemVM.CompositionType.MeleeCavalry:
                    str = (isBig ? "cavalry_big" : "cavalry");
                    textObject = GameTexts.FindText("str_troop_type_name", "Cavalry");
                    break;
                case SCArmyCompositionItemVM.CompositionType.RangedCavalry:
                    str = (isBig ? "horse_archer_big" : "horse_archer");
                    textObject = GameTexts.FindText("str_troop_type_name", "HorseArcher");
                    break;
                default:
                    return new StringItemWithHintVM("", TextObject.Empty);
            }
            return new StringItemWithHintVM("General\\TroopTypeIcons\\icon_troop_type_" + str, new TextObject("{=!}" + textObject.ToString(), null));
        }
    }
}
