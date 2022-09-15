using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.Util
{
    public class ShaderGameManager: CustomGameManager
    {
        public override void OnLoadFinished()
        {
            IsLoaded = true;
            LoadScene();
        }

        private void LoadScene()
        {
            CustomBattleData data = CustomBattleHelper.PrepareBattleData(SelectPlayer(), null, 
                GetPlayerParty(SelectPlayer()), GetEnemyParty(), CustomBattlePlayerSide.Defender, 
                CustomBattlePlayerType.Commander, CustomBattleGameType.Battle, "battle_terrain_a", "summer", 12, 
                null, null, null, 0, false);
            
            CustomBattleHelper.StartGame(data);
        }

        private CustomBattleCombatant GetEnemyParty()
        {
            BasicCultureObject culture = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            BasicCharacterObject enemyCharacter = MBObjectManager.Instance.GetObject<BasicCharacterObject>("republic_clone_cadet");

            CustomBattleCombatant party = new (new TextObject("Enemy Party"), culture, Banner.CreateRandomBanner());
            party.AddCharacter(enemyCharacter, 1);
            party.Side = BattleSideEnum.Attacker;
            return party;
        }

        private CustomBattleCombatant GetPlayerParty(BasicCharacterObject playerCharacter)
        {
            MBReadOnlyList<BasicCharacterObject> characters = MBObjectManager.Instance.GetObjectTypeList<BasicCharacterObject>();
            
            BasicCultureObject culture0 = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            BasicCultureObject culture1 = MBObjectManager.Instance.GetObject<BasicCultureObject>("separatistalliance");
            BasicCultureObject culture2 = MBObjectManager.Instance.GetObject<BasicCultureObject>("mandalorians");
            IEnumerable<BasicCharacterObject> charactersList = characters.Where(x => x.IsSoldier && (x.Culture == culture0 || x.Culture == culture1 || x.Culture == culture2));
            
            CustomBattleCombatant party = new (new TextObject("Player Party"), culture0, Banner.CreateRandomBanner());
            party.AddCharacter(playerCharacter, 1);
            party.SetGeneral(playerCharacter);
            party.Side = BattleSideEnum.Defender;
            foreach(BasicCharacterObject unit in charactersList)
            {
                int num = 1;

                party.AddCharacter(unit, num);
            }
            
            return party;
        }

        private BasicCharacterObject SelectPlayer()
        {
            return MBObjectManager.Instance.GetObject<BasicCharacterObject>("republic_clone_cadet");
        }
    }
}