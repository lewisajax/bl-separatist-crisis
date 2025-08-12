using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.ScreenSystem;

namespace SeparatistCrisis.Patches
{
    /*[HarmonyPatch]
    public class FixCrosshair
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MissionGauntletCrosshair), "GetShouldCrosshairBeVisible")]
        private static void GetShouldCrosshairBeVisible(MissionGauntletCrosshair __instance, ref bool __result)
        {
            if (__result || 
                !BannerlordConfig.DisplayTargetingReticule ||
                __instance.Mission.Mode == MissionMode.Conversation ||
                __instance.Mission.Mode == MissionMode.CutScene ||
                ScreenManager.GetMouseVisibility())
            {
                return;
            }

            if (__instance.Mission.MainAgent == null || __instance.Mission.MainAgent.WieldedWeapon.IsEmpty)
            {
                return;
            }

            MissionWeapon missionWeapon = __instance.Mission.MainAgent.WieldedWeapon;
            if (!missionWeapon.CurrentUsageItem.IsRangedWeapon || __instance.MissionScreen.IsViewingCharacter() || __instance.MissionScreen.CustomCamera != null)
            {
                return;
            }

            if (missionWeapon.CurrentUsageItem.WeaponClass != WeaponClass.Crossbow)
            {
                return;
            }
            
            missionWeapon = missionWeapon.AmmoWeapon;
            if (missionWeapon.Amount <= (short) 0)
            {
                return;
            }

            __result = true;
        }
    }*/
}