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

namespace SeparatistCrisis.CustomBattle
{
    public class SCCustomBattleMenuSideVM : ViewModel
    {
        private readonly TextObject _sideName;

        private readonly bool _isPlayerSide;

        private SCArmyCompositionGroupVM _compositionGroup = null!;

        private SelectorVM<SCFactionItemVM> _factionSelectionGroup = null!;

        private SelectorVM<CharacterItemVM> _characterSelectionGroup = null!;

        private CharacterViewModel _currentSelectedCharacter = null!;

        private string _characterName = null!;

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
                return _currentSelectedCharacter;
            }
            set
            {
                if (value != _currentSelectedCharacter)
                {
                    _currentSelectedCharacter = value;
                    OnPropertyChangedWithValue(value, "CurrentSelectedCharacter");
                }
            }
        }

        [DataSourceProperty]
        public string CharacterName
        {
            get
            {
                return _characterName;
            }
            set
            {
                if (value != _characterName)
                {
                    _characterName = value;
                    OnPropertyChangedWithValue(value, "CharacterName");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CharacterEquipmentItemVM> ArmorsList
        {
            get
            {
                return _armorsList;
            }
            set
            {
                if (value != _armorsList)
                {
                    _armorsList = value;
                    OnPropertyChangedWithValue(value, "ArmorsList");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CharacterEquipmentItemVM> WeaponsList
        {
            get
            {
                return _weaponsList;
            }
            set
            {
                if (value != _weaponsList)
                {
                    _weaponsList = value;
                    OnPropertyChangedWithValue(value, "WeaponsList");
                }
            }
        }

        [DataSourceProperty]
        public string FactionText
        {
            get
            {
                return _factionText;
            }
            set
            {
                if (value != _factionText)
                {
                    _factionText = value;
                    OnPropertyChangedWithValue(value, "FactionText");
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
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<CharacterItemVM> CharacterSelectionGroup
        {
            get
            {
                return _characterSelectionGroup;
            }
            set
            {
                if (value != _characterSelectionGroup)
                {
                    _characterSelectionGroup = value;
                    OnPropertyChangedWithValue(value, "CharacterSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public SCArmyCompositionGroupVM CompositionGroup
        {
            get
            {
                return _compositionGroup;
            }
            set
            {
                if (value != _compositionGroup)
                {
                    _compositionGroup = value;
                    OnPropertyChangedWithValue(value, "CompositionGroup");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SCFactionItemVM> FactionSelectionGroup
        {
            get
            {
                return _factionSelectionGroup;
            }
            set
            {
                if (value != _factionSelectionGroup)
                {
                    _factionSelectionGroup = value;
                    OnPropertyChangedWithValue(value, "FactionSelectionGroup");
                }
            }
        }

        [DataSourceProperty]
        public string SelectedFactionName
        {
            get
            {
                return _selectedFactionName;
            }
            set
            {
                if (value != _selectedFactionName)
                {
                    _selectedFactionName = value;
                    OnPropertyChangedWithValue(value, "SelectedFactionName");
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
            _sideName = sideName;
            _isPlayerSide = isPlayerSide;
            CompositionGroup = new SCArmyCompositionGroupVM(_isPlayerSide, troopTypeSelectionPopUp);
            FactionSelectionGroup = new SelectorVM<SCFactionItemVM>(0, new Action<SelectorVM<SCFactionItemVM>>(OnCultureSelection));
            CharacterSelectionGroup = new SelectorVM<CharacterItemVM>(0, new Action<SelectorVM<CharacterItemVM>>(OnCharacterSelection));
            ArmorsList = new MBBindingList<CharacterEquipmentItemVM>();
            WeaponsList = new MBBindingList<CharacterEquipmentItemVM>();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = _sideName.ToString();
            FactionText = GameTexts.FindText("str_faction", null).ToString();

            if (_isPlayerSide)
            {
                TitleText = new TextObject("{=bLXleed8}Player Character", null).ToString();
            }
            else
            {
                TitleText = new TextObject("{=QAYngoNQ}Enemy Character", null).ToString();
            }

            CharacterSelectionGroup.ItemList.Clear();
            foreach (BasicCharacterObject character in CustomBattleData.Characters)
            {
                CharacterSelectionGroup.AddItem(new CharacterItemVM(character));
            }
            CharacterSelectionGroup.SelectedIndex = _isPlayerSide ? 0 : 1;

            FactionSelectionGroup.ItemList.Clear();
            foreach (BasicCultureObject faction in Game.Current.ObjectManager.GetObjectTypeList<BasicCultureObject>().Where(x => x.IsMainCulture).ToArray())
            {
                FactionSelectionGroup.AddItem(new SCFactionItemVM(faction));
            }
            FactionSelectionGroup.SelectedIndex = _isPlayerSide ? 0 : 1;

            UpdateCharacterVisual();

            CompositionGroup.RefreshValues();
            CharacterSelectionGroup.RefreshValues();
            FactionSelectionGroup.RefreshValues();
        }

        public void OnPlayerTypeChange(CustomBattlePlayerType playerType)
        {
            CompositionGroup.OnPlayerTypeChange(playerType);
        }

        public void OnHeroSelection()
        {
            List<InquiryElement> options = new List<InquiryElement>();
            // options.Add(new InquiryElement(null, GameTexts.FindText("str_empty", null).ToString(), null));

            foreach (BasicCharacterObject hero in Game.Current.ObjectManager.GetObjectTypeList<BasicCharacterObject>()
                .Where(x => x.IsHero && x.Culture == FactionSelectionGroup.SelectedItem.Faction))
            {
                options.Add(new InquiryElement(new CharacterItemVM(hero), hero.Name.ToString(), null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("Select a Hero", null).ToString(), 
                string.Empty, options, true, 1, 1, GameTexts.FindText("str_done", null).ToString(), GameTexts.FindText("str_cancel", null).ToString(), delegate (List<InquiryElement> selectedElements)
            {
                InquiryElement? inquiryElement = selectedElements.FirstOrDefault();
                
                if (inquiryElement != null)
                {
                    SelectedCharacter = ((CharacterItemVM)inquiryElement.Identifier).Character;
                    CharacterName = SelectedCharacter.Name.ToString();
                    UpdateCharacterVisual();
                }
            }, delegate (List<InquiryElement> target) { }, "", true), false, false);
        }

        private void OnCultureSelection(SelectorVM<SCFactionItemVM> selector)
        {
            BasicCultureObject faction = selector.SelectedItem.Faction;
            SelectedFaction = faction;
            CompositionGroup.SetCurrentSelectedCulture(faction);

            if (CurrentSelectedCharacter != null)
            {
                CurrentSelectedCharacter.ArmorColor1 = faction.Color;
                CurrentSelectedCharacter.ArmorColor2 = faction.Color2;
                CurrentSelectedCharacter.BannerCodeText = faction.BannerKey;
            }

            if (faction != null)
            {
                SelectedFactionName = faction.Name.ToString();
            }
            else
            {
                SelectedFactionName = string.Empty;
            }

            if (OppositeSide != null)
            {
                int num = 0;
                foreach (SCFactionItemVM characterItemVM in selector.ItemList)
                {
                    SCFactionItemVM characterItemVM2 = OppositeSide.FactionSelectionGroup.ItemList[num];
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
            SelectedCharacter = character;
            CharacterName = character.Name.ToString();
            UpdateCharacterVisual();
            if (OppositeSide != null)
            {
                int num = 0;
                foreach (CharacterItemVM characterItemVM in selector.ItemList)
                {
                    CharacterItemVM characterItemVM2 = OppositeSide.CharacterSelectionGroup.ItemList[num];
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
            CurrentSelectedCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.EmphasizeFace);
            CurrentSelectedCharacter.FillFrom(SelectedCharacter, -1);
            SelectorVM<SCFactionItemVM> factionSelectionGroup = FactionSelectionGroup;
            if ((factionSelectionGroup != null ? factionSelectionGroup.SelectedItem : null) != null)
            {
                CurrentSelectedCharacter.ArmorColor1 = FactionSelectionGroup.SelectedItem.Faction.Color;
                CurrentSelectedCharacter.ArmorColor2 = FactionSelectionGroup.SelectedItem.Faction.Color2;
                CurrentSelectedCharacter.BannerCodeText = FactionSelectionGroup.SelectedItem.Faction.BannerKey;
            }
            ArmorsList.Clear();
            ArmorsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.NumAllWeaponSlots].Item));
            ArmorsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Cape].Item));
            ArmorsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Body].Item));
            ArmorsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Gloves].Item));
            ArmorsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Leg].Item));
            WeaponsList.Clear();
            WeaponsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.WeaponItemBeginSlot].Item));
            WeaponsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Weapon1].Item));
            WeaponsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Weapon2].Item));
            WeaponsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.Weapon3].Item));
            WeaponsList.Add(new CharacterEquipmentItemVM(SelectedCharacter.Equipment[EquipmentIndex.ExtraWeaponSlot].Item));
        }

        public void Randomize()
        {
            FactionSelectionGroup.ExecuteRandomize();
            // this.CharacterSelectionGroup.ExecuteRandomize();

            BasicCharacterObject[] cultureHeroes = Game.Current.ObjectManager.GetObjectTypeList<BasicCharacterObject>()
                .Where(x => x.IsHero && x.Culture == FactionSelectionGroup.SelectedItem.Faction).ToArray();

            if (cultureHeroes != null)
            {
                int randNum = cultureHeroes.Length > 1 ? MBRandom.RandomInt(0, cultureHeroes.Length - 1) : 0;
                SelectedCharacter = cultureHeroes[randNum];
                CharacterName = SelectedCharacter.Name.ToString();
                UpdateCharacterVisual();
            }

            CompositionGroup.ExecuteRandomize();
        }
    }
}
