using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace SeparatistCrisis.CustomBattle
{
    public class SCCustomBattleFactionSelectionVM : ViewModel
    {
        private Action<BasicCultureObject> _onSelectionChanged;

        private MBBindingList<FactionItemVM> _factions = null!;

        private string _selectedFactionName = null!;

        public FactionItemVM SelectedItem { get; set; }

        [DataSourceProperty]
        public MBBindingList<FactionItemVM> Factions
        {
            get
            {
                return _factions;
            }
            set
            {
                if (value != _factions)
                {
                    _factions = value;
                    OnPropertyChangedWithValue(value, "Factions");
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

        public SCCustomBattleFactionSelectionVM(Action<BasicCultureObject> onSelectionChanged)
        {
            _onSelectionChanged = onSelectionChanged;
            Factions = new MBBindingList<FactionItemVM>();
            foreach (BasicCultureObject faction in CustomBattleData.Factions)
            {
                Factions.Add(new FactionItemVM(faction, new Action<FactionItemVM>(OnFactionSelected)));
            }
            SelectedItem = Factions[0];
            SelectFaction(0);
            Factions.ApplyActionOnAllItems(delegate (FactionItemVM x)
            {
                x.RefreshValues();
            });
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FactionItemVM selectedItem = SelectedItem;

            if (selectedItem != null)
                SelectedFactionName = selectedItem.Faction.Name.ToString();
            else
                SelectedFactionName = string.Empty;
        }

        public void SelectFaction(int index)
        {
            if (index >= 0 && index < Factions.Count)
            {
                SelectedItem.IsSelected = false;
                SelectedItem = Factions[index];
                SelectedItem.IsSelected = true;
            }
        }

        public void ExecuteRandomize()
        {
            int index = MBRandom.RandomInt(Factions.Count);
            SelectFaction(index);
        }

        private void OnFactionSelected(FactionItemVM faction)
        {
            SelectedItem = faction;
            _onSelectionChanged(faction.Faction);
            SelectedFactionName = SelectedItem.Faction.Name.ToString();
        }
    }
}
