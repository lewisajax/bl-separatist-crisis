using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.Utils
{
    public class ShaderGameManager : CustomGameManager
    {
        public override void OnLoadFinished()
        {
            IsLoaded = true;
            LoadScene();
        }

        private void LoadScene()
        {
            CustomBattleData data = new CustomBattleData();
            data.GameType = CustomBattleGameType.Battle;
            data.SceneId = "battle_terrain_a";
            data.PlayerCharacter = SelectPlayer();
            data.PlayerParty = GetPlayerParty(data.PlayerCharacter);
            data.EnemyParty = GetEnemyParty();
            data.IsPlayerGeneral = true;
            data.PlayerSideGeneralCharacter = null;
            data.SeasonId = "summer";
            data.SceneLevel = "";
            data.TimeOfDay = 12;

            CustomBattleHelper.StartGame(data);
        }

        private CustomBattleCombatant GetEnemyParty()
        {
            BasicCultureObject culture = MBObjectManager.Instance.GetObject<BasicCultureObject>("galactic_republic");
            BasicCharacterObject enemyCharacter =
                MBObjectManager.Instance.GetObject<BasicCharacterObject>("republic_clone_cadet");

            CustomBattleCombatant party = new(new TextObject("Enemy Party"), culture, Banner.CreateRandomBanner());
            party.AddCharacter(enemyCharacter, 1);
            party.Side = BattleSideEnum.Attacker;
            return party;
        }

        private CustomBattleCombatant GetPlayerParty(BasicCharacterObject playerCharacter)
        {
            MBReadOnlyList<BasicCharacterObject> characters =
                MBObjectManager.Instance.GetObjectTypeList<BasicCharacterObject>();

            //BasicCultureObject culture0 = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            BasicCultureObject culture0 = MBObjectManager.Instance.GetObject<BasicCultureObject>("galactic_republic");
            IEnumerable<BasicCharacterObject> charactersList = characters.Where(x =>
                (x.IsSoldier || x.IsHero) && (x.Culture == culture0) && x != playerCharacter);

            CustomBattleCombatant party = new(new TextObject("Player Party"), culture0, Banner.CreateRandomBanner());
            party.AddCharacter(playerCharacter, 1);
            party.SetGeneral(playerCharacter);
            party.Side = BattleSideEnum.Defender;
            foreach (BasicCharacterObject unit in charactersList)
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