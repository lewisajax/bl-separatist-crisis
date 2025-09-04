using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace SeparatistCrisis.ViewModels
{
    public class SCCustomBattleMenuSideVM : ViewModel
    {
        private readonly TextObject _sideName;

        private readonly bool _isPlayerSide;

        private SCArmyCompositionGroupVM _compositionGroup = null!;

        private SelectorVM<SCFactionItemVM> _factionSelectionGroup = null!;

        private SelectorVM<CharacterItemVM> _characterSelectionGroup = null!;

        private CharacterViewModel _currentSelectedCharacter = null!;

        private string _selectedFactionName = null!;

        private MBBindingList<CharacterEquipmentItemVM> _armorsList = null!;

        private MBBindingList<CharacterEquipmentItemVM> _weaponsList = null!;

        private string _name = null!;

        private string _factionText = null!;

        private string _titleText = null!;

        // private ImageIdentifierVM _factionVisual;

        public BasicCultureObject SelectedFaction { get; private set; } = null!;
        public BasicCharacterObject SelectedCharacter { get; private set; } = null!;

        public SCCustomBattleMenuSideVM OppositeSide { get; set; } = null!;

        [DataSourceProperty]
        public CharacterViewModel CurrentSelectedCharacter
        {
            get
            {
                return this._currentSelectedCharacter;
            }
            set
            {
                if (value != this._currentSelectedCharacter)
                {
                    this._currentSelectedCharacter = value;
                    base.OnPropertyChangedWithValue<CharacterViewModel>(value, "CurrentSelectedCharacter");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CharacterEquipmentItemVM> ArmorsList
        {
            get
            {
                return this._armorsList;
            }
            set
            {
                if (value != this._armorsList)
                {
                    this._armorsList = value;
                    base.OnPropertyChangedWithValue<MBBindingList<CharacterEquipmentItemVM>>(value, "ArmorsList");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CharacterEquipmentItemVM> WeaponsList
        {
            get
            {
                return this._weaponsList;
            }
            set
            {
                if (value != this._weaponsList)
                {
                    this._weaponsList = value;
                    base.OnPropertyChangedWithValue<MBBindingList<CharacterEquipmentItemVM>>(value, "WeaponsList");
                }
            }
        }

        [DataSourceProperty]
        public string FactionText
        {
            get
            {
                return this._factionText;
            }
            set
            {
                if (value != this._factionText)
                {
                    this._factionText = value;
                    base.OnPropertyChangedWithValue<string>(value, "FactionText");
                }
            }
        }

        [DataSourceProperty]
        public string TitleText
        {
            get
            {
                return this._titleText;
            }
            set
            {
                if (value != this._titleText)
                {
                    this._titleText = value;
                    base.OnPropertyChangedWithValue<string>(value, "TitleText");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue<string>(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<CharacterItemVM> CharacterSelectionGroup
        {
            get
            {
                return this._characterSelectionGroup;
            }
            set
            {
                if (value != this._characterSelectionGroup)
                {
                    this._characterSelectionGroup = value;
                    base.OnPropertyChangedWithValue<SelectorVM<CharacterItemVM>>(value, "CharacterSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public SCArmyCompositionGroupVM CompositionGroup
        {
            get
            {
                return this._compositionGroup;
            }
            set
            {
                if (value != this._compositionGroup)
                {
                    this._compositionGroup = value;
                    base.OnPropertyChangedWithValue<SCArmyCompositionGroupVM>(value, "CompositionGroup");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SCFactionItemVM> FactionSelectionGroup
        {
            get
            {
                return this._factionSelectionGroup;
            }
            set
            {
                if (value != this._factionSelectionGroup)
                {
                    this._factionSelectionGroup = value;
                    base.OnPropertyChangedWithValue<SelectorVM<SCFactionItemVM>>(value, "FactionSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public string SelectedFactionName
        {
            get
            {
                return this._selectedFactionName;
            }
            set
            {
                if (value != this._selectedFactionName)
                {
                    this._selectedFactionName = value;
                    base.OnPropertyChangedWithValue<string>(value, "SelectedFactionName");
                }
            }
        }

        //[DataSourceProperty]
        //public ImageIdentifierVM FactionVisual
        //{
        //    get
        //    {
        //        return this._factionVisual;
        //    }
        //    set
        //    {
        //        if (value != this._factionVisual)
        //        {
        //            this._factionVisual = value;
        //            base.OnPropertyChangedWithValue<ImageIdentifierVM>(value, "FactionVisual");
        //        }
        //    }
        //}

        public SCCustomBattleMenuSideVM(TextObject sideName, bool isPlayerSide, SCTroopSelectionPopUpVM troopTypeSelectionPopUp)
        {
            this._sideName = sideName;
            this._isPlayerSide = isPlayerSide;
            this.CompositionGroup = new SCArmyCompositionGroupVM(this._isPlayerSide, troopTypeSelectionPopUp);
            this.FactionSelectionGroup = new SelectorVM<SCFactionItemVM>(0, new Action<SelectorVM<SCFactionItemVM>>(this.OnCultureSelection));
            this.CharacterSelectionGroup = new SelectorVM<CharacterItemVM>(0, new Action<SelectorVM<CharacterItemVM>>(this.OnCharacterSelection));
            this.ArmorsList = new MBBindingList<CharacterEquipmentItemVM>();
            this.WeaponsList = new MBBindingList<CharacterEquipmentItemVM>();
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Name = this._sideName.ToString();
            this.FactionText = GameTexts.FindText("str_faction", null).ToString();

            if (this._isPlayerSide)
            {
                this.TitleText = new TextObject("{=bLXleed8}Player Character", null).ToString();
            }
            else
            {
                this.TitleText = new TextObject("{=QAYngoNQ}Enemy Character", null).ToString();
            }

            this.CharacterSelectionGroup.ItemList.Clear();
            foreach (BasicCharacterObject character in CustomBattleData.Characters)
            {
                this.CharacterSelectionGroup.AddItem(new CharacterItemVM(character));
            }
            this.CharacterSelectionGroup.SelectedIndex = (this._isPlayerSide ? 0 : 1);

            this.FactionSelectionGroup.ItemList.Clear();
            foreach (BasicCultureObject faction in Game.Current.ObjectManager.GetObjectTypeList<BasicCultureObject>().Where(x => x.IsMainCulture).ToArray())
            {
                this.FactionSelectionGroup.AddItem(new SCFactionItemVM(faction));
            }
            this.FactionSelectionGroup.SelectedIndex = (this._isPlayerSide ? 0 : 1);

            this.UpdateCharacterVisual();

            this.CompositionGroup.RefreshValues();
            this.CharacterSelectionGroup.RefreshValues();
            this.FactionSelectionGroup.RefreshValues();
        }

        public void OnPlayerTypeChange(CustomBattlePlayerType playerType)
        {
            this.CompositionGroup.OnPlayerTypeChange(playerType);
        }

        private void OnCultureSelection(SelectorVM<SCFactionItemVM> selector)
        {
            BasicCultureObject faction = selector.SelectedItem.Faction;
            this.SelectedFaction = faction;
            this.CompositionGroup.SetCurrentSelectedCulture(faction);

            if (this.CurrentSelectedCharacter != null)
            {
                this.CurrentSelectedCharacter.ArmorColor1 = faction.Color;
                this.CurrentSelectedCharacter.ArmorColor2 = faction.Color2;
                this.CurrentSelectedCharacter.BannerCodeText = faction.BannerKey;
            }

            if (faction != null)
            {
                this.SelectedFactionName = faction.Name.ToString();
            }
            else
            {
                this.SelectedFactionName = string.Empty;
            }

            if (this.OppositeSide != null)
            {
                int num = 0;
                foreach (SCFactionItemVM characterItemVM in selector.ItemList)
                {
                    SCFactionItemVM characterItemVM2 = this.OppositeSide.FactionSelectionGroup.ItemList[num];
                    if (num == selector.SelectedIndex)
                    {
                        characterItemVM2.CanBeSelected = false;
                    }
                    else
                    {
                        characterItemVM2.CanBeSelected = true;
                    }
                    num++;
                }
            }
        }

        private void OnCharacterSelection(SelectorVM<CharacterItemVM> selector)
        {
            BasicCharacterObject character = selector.SelectedItem.Character;
            this.SelectedCharacter = character;
            this.UpdateCharacterVisual();
            if (this.OppositeSide != null)
            {
                int num = 0;
                foreach (CharacterItemVM characterItemVM in selector.ItemList)
                {
                    CharacterItemVM characterItemVM2 = this.OppositeSide.CharacterSelectionGroup.ItemList[num];
                    if (num == selector.SelectedIndex)
                    {
                        characterItemVM2.CanBeSelected = false;
                    }
                    else
                    {
                        characterItemVM2.CanBeSelected = true;
                    }
                    num++;
                }
            }
        }

        public void UpdateCharacterVisual()
        {
            this.CurrentSelectedCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.EmphasizeFace);
            this.CurrentSelectedCharacter.FillFrom(this.SelectedCharacter, -1);
            SelectorVM<SCFactionItemVM> factionSelectionGroup = this.FactionSelectionGroup;
            if (((factionSelectionGroup != null) ? factionSelectionGroup.SelectedItem : null) != null)
            {
                this.CurrentSelectedCharacter.ArmorColor1 = this.FactionSelectionGroup.SelectedItem.Faction.Color;
                this.CurrentSelectedCharacter.ArmorColor2 = this.FactionSelectionGroup.SelectedItem.Faction.Color2;
                this.CurrentSelectedCharacter.BannerCodeText = this.FactionSelectionGroup.SelectedItem.Faction.BannerKey;
            }
            this.ArmorsList.Clear();
            this.ArmorsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.NumAllWeaponSlots].Item));
            this.ArmorsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Cape].Item));
            this.ArmorsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Body].Item));
            this.ArmorsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Gloves].Item));
            this.ArmorsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Leg].Item));
            this.WeaponsList.Clear();
            this.WeaponsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.WeaponItemBeginSlot].Item));
            this.WeaponsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Weapon1].Item));
            this.WeaponsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Weapon2].Item));
            this.WeaponsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.Weapon3].Item));
            this.WeaponsList.Add(new CharacterEquipmentItemVM(this.SelectedCharacter.Equipment[EquipmentIndex.ExtraWeaponSlot].Item));
        }

        public void Randomize()
        {
            this.CharacterSelectionGroup.ExecuteRandomize();
            this.FactionSelectionGroup.ExecuteRandomize();
            this.CompositionGroup.ExecuteRandomize();
        }
    }
}
