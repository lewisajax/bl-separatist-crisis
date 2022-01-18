using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SeparatistCrisis.Patches
{
    [HarmonyPatch]
    public class SetSandboxPlayerStartPos
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SandboxCharacterCreationContent), "OnCharacterCreationFinalized")]
        private static void OnCharacterCreationFinalized(SandboxCharacterCreationContent __instance)
        {
            CultureObject culture = __instance.GetSelectedCulture();
            Vec2 settlementLocation = Settlement.FindFirst(s => s.Culture == culture).GatePosition;

            MobileParty.MainParty.Position2D = settlementLocation;
            ((MapState)GameStateManager.Current.ActiveState).Handler.TeleportCameraToMainParty();
        }
    }
}