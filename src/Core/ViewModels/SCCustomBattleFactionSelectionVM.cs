using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace SeparatistCrisis.ViewModels
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
                return this._factions;
            }
            set
            {
                if (value != this._factions)
                {
                    this._factions = value;
                    base.OnPropertyChangedWithValue<MBBindingList<FactionItemVM>>(value, "Factions");
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

        public SCCustomBattleFactionSelectionVM(Action<BasicCultureObject> onSelectionChanged)
        {
            this._onSelectionChanged = onSelectionChanged;
            this.Factions = new MBBindingList<FactionItemVM>();
            foreach (BasicCultureObject faction in CustomBattleData.Factions)
            {
                this.Factions.Add(new FactionItemVM(faction, new Action<FactionItemVM>(this.OnFactionSelected)));
            }
            this.SelectedItem = this.Factions[0];
            this.SelectFaction(0);
            this.Factions.ApplyActionOnAllItems(delegate (FactionItemVM x)
            {
                x.RefreshValues();
            });
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FactionItemVM selectedItem = this.SelectedItem;

            if (selectedItem != null)
                this.SelectedFactionName = selectedItem.Faction.Name.ToString();
            else
                this.SelectedFactionName = string.Empty;
        }

        public void SelectFaction(int index)
        {
            if (index >= 0 && index < this.Factions.Count)
            {
                this.SelectedItem.IsSelected = false;
                this.SelectedItem = this.Factions[index];
                this.SelectedItem.IsSelected = true;
            }
        }

        public void ExecuteRandomize()
        {
            int index = MBRandom.RandomInt(this.Factions.Count);
            this.SelectFaction(index);
        }

        private void OnFactionSelected(FactionItemVM faction)
        {
            this.SelectedItem = faction;
            this._onSelectionChanged(faction.Faction);
            this.SelectedFactionName = this.SelectedItem.Faction.Name.ToString();
        }
    }
}
