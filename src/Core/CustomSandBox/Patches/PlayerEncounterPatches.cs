using HarmonyLib;
using Helpers;
using SandBox;
using SandBox.GauntletUI.Map;
using SeparatistCrisis.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SeparatistCrisis.CustomSandBox
{
    public class PlayerEncountersPatches : PatchClass<PlayerEncountersPatches>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(Init), new Utils.Reflect.Method(typeof(PlayerEncounter), "Init", new Type[] { typeof(PartyBase), typeof(PartyBase), typeof(Settlement) })),
            new Prefix(nameof(Initialize), new Utils.Reflect.Method(typeof(GauntletMapEventVisual), "Initialize", new Type[] { typeof(CampaignVec2), typeof(int), typeof(bool) })),
        };

        // Really should use a transpiler for this. 
        // All of this to change the flag variable to true.
        private static bool Init(PlayerEncounter __instance, PartyBase attackerParty, PartyBase defenderParty, Settlement settlement = null)
        {
            MethodInfo encSettlementAux = AccessTools.PropertySetter(typeof(PlayerEncounter), "EncounterSettlementAux");
            encSettlementAux.Invoke(__instance, new object[] { ((settlement != null) ? settlement : (defenderParty.IsSettlement ? defenderParty.Settlement : attackerParty.Settlement)) });

            PlayerEncounter.EnemySurrender = false;
            __instance.PlayerPartyInitialStrength = MobileParty.MainParty.Party.CalculateCurrentStrength();
            __instance.SetupFields(attackerParty, defenderParty);
            if (defenderParty.MapEvent != null && attackerParty != MobileParty.MainParty.Party && defenderParty != MobileParty.MainParty.Party)
            {
                FieldInfo mapEvent = AccessTools.Field(typeof(PlayerEncounter), "_mapEvent");

                mapEvent.SetValue(__instance, defenderParty.MapEvent);
                MapEvent mapEventVal = (MapEvent)mapEvent.GetValue(__instance);
                if (mapEventVal.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Defender))
                {
                    MobileParty.MainParty.Party.MapEventSide = mapEventVal.DefenderSide;
                }
                else if (mapEventVal.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Attacker))
                {
                    MobileParty.MainParty.Party.MapEventSide = mapEventVal.AttackerSide;
                }
            }
            bool flag = false; // joinBattle.
            bool flag2 = false; // startBattle. This gets assigned false in GetEncounterMenu so we assign it later
            // All the settlement types open up a menu on the encounter, it doesn't automatically start the battle. We're skipping the menu.
            string encounterMenu = Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(attackerParty, defenderParty, out flag2, out flag);
            if (!string.IsNullOrEmpty(encounterMenu))
            {
                flag2 = true;
                if (flag2)
                {
                    PlayerEncounter.StartBattle();
                    if (MobileParty.MainParty.MapEvent == null)
                    {
                        encounterMenu = Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(attackerParty, defenderParty, out flag2, out flag);
                    }
                }
                if (flag)
                {
                    if (MobileParty.MainParty.MapEvent == null)
                    {
                        if (defenderParty.MapEvent != null)
                        {
                            if (defenderParty.MapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Attacker))
                            {
                                PlayerEncounter.JoinBattle(BattleSideEnum.Attacker);
                            }
                            else if (defenderParty.MapEvent.CanPartyJoinBattle(PartyBase.MainParty, BattleSideEnum.Defender))
                            {
                                PlayerEncounter.JoinBattle(BattleSideEnum.Defender);
                            }
                            else
                            {
                                Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "Init", 508);
                            }
                        }
                        else
                        {
                            Debug.FailedAssert("If there is no map event we should create one in order to join battle", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encounters\\PlayerEncounter.cs", "Init", 513);
                        }
                    }

                    MethodInfo checkParties = AccessTools.Method(typeof(PlayerEncounter), "CheckNearbyPartiesToJoinPlayerMapEvent");
                    checkParties.Invoke(__instance, null);
                }
                if (attackerParty == PartyBase.MainParty && defenderParty.IsSettlement && !defenderParty.Settlement.IsUnderRaid && !defenderParty.Settlement.IsUnderSiege)
                {
                    PlayerEncounter.EnterSettlement();
                }
                GameMenu.ActivateGameMenu(encounterMenu);
            }
            else if (attackerParty == PartyBase.MainParty && defenderParty.IsSettlement && !defenderParty.Settlement.IsUnderRaid && !defenderParty.Settlement.IsUnderSiege)
            {
                PlayerEncounter.EnterSettlement();
            }
            __instance.ForceSallyOut = false;
            __instance.ForceBlockadeSallyOutAttack = false;
            __instance.ForceRaid = false;
            __instance.ForceSupplies = false;
            __instance.ForceVolunteers = false;

            FieldInfo isSally = AccessTools.Field(typeof(PlayerEncounter), "_isSallyOutAmbush");
            isSally.SetValue(__instance, false);

            return false;
        }

        // We want to remove the sound event creation but we don't want to patch the actual sound.create method since it'll be used by other things not related to the map.
        // Everything is in 1 block so we should really use a transpiler
        public static bool Initialize(GauntletMapEventVisual __instance, CampaignVec2 position, int battleSizeValue, bool isVisible)
        {
            PropertyInfo worldPos = AccessTools.Property(typeof(GauntletMapEventVisual), "WorldPosition");
            PropertyInfo isVis = AccessTools.Property(typeof(GauntletMapEventVisual), "IsVisible");
            worldPos.SetValue(__instance, position.ToVec2());
            isVis.SetValue(__instance, isVisible);

            FieldInfo onInit = AccessTools.Field(typeof(GauntletMapEventVisual), "_onInitialized");
            Action<GauntletMapEventVisual> onInitialized = (Action<GauntletMapEventVisual>)onInit.GetValue(__instance);
            if (onInitialized != null)
            {
                onInitialized(__instance);
            }

            // All the sound stuff was here

            return false;
        }
    }
}
