using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.ObjectTypes
{
    /*
        OnSubModuleLoad
        OnApplicationTick
        OnBeforeInitialModuleScreenSetAsRoot

        [StartupVideo Begins]
        OnInitialState

        [MainMenu]

        [Press CustomBattle]
        OnBeforeGameStart
        RegisterSubModuleTypes
        InitializeGameStarter
        OnGameStart
        BeginGameStart
        InitializeSubModuleGameObjects

        [We Reach the CustomGame's Types]
        LoadCustomGameXmls

        OnCampaignStart
        OnGameInitializationFinished

        [We Reach CustomBattle Screen]

        [On Pressing Play]
        OnBeforeMissionBehaviorInitialize
        OnMissionBehaviorInitialize
        AfterAsyncTickTick [On Different Thread]

        [We're In-Game Now]

        [On Exiting]
        OnGameEnd [We're Back On Original Thread]

        [We Reach Main Menu]

        [On Exit Game]
        OnSubModuleUnloaded
    */
    public class AbilityHero: MBObjectBase
    {
        public BasicCharacterObject? Character { get; private set; }

        public MBReadOnlyList<Ability> Abilities { get; private set; } = null!;

        public override void Deserialize(MBObjectManager objectManager, XmlNode node)
        {
            bool isInitialized = base.IsInitialized;
            base.Deserialize(objectManager, node);

            if (node == null || objectManager == null)
                return;

            string? charId = node.Attributes?["character_id"]?.Value;

            if (charId != null)
                this.Character = objectManager.GetObject<BasicCharacterObject>(charId);

            this.Abilities = new MBReadOnlyList<Ability>();

            foreach (object obj in node.ChildNodes)
            {
                XmlNode abilityNode = (XmlNode)obj;
                if (abilityNode.Name == "AbilitySlot")
                {
                    Ability abi = objectManager.ReadObjectReferenceFromXml<Ability>("action", abilityNode);
                    if (abi != null)
                        this.Abilities.Add(abi);
                }
            }
        }

        public static MBReadOnlyList<AbilityHero> All
        {
            get
            {
                return MBObjectManager.Instance.GetObjectTypeList<AbilityHero>();
            }
        }

        public static AbilityHero Find(string idString)
        {
            return MBObjectManager.Instance.GetObject<AbilityHero>(idString);
        }

        public static AbilityHero FindFirst(Func<AbilityHero, bool> predicate)
        {
            return AbilityHero.All.FirstOrDefault(predicate);
        }

        public static IEnumerable<AbilityHero> FindAll(Func<AbilityHero, bool> predicate)
        {
            return AbilityHero.All.Where(predicate);
        }
    }
}
