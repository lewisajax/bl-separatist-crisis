using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SeparatistCrisis.Patches
{
    //[HarmonyPatch]
    public class DeactivateVanillaCultureSelection
    {
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(CharacterCreationContentBase), "GetCultures")]
        private static bool GetCultures(ref IEnumerable<CultureObject> __result)
        {
            __result = new List<CultureObject>();
            List<CultureObject> list = new List<CultureObject>();
            foreach (CultureObject objectType in MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
            {
                if (!objectType.IsMainCulture) continue;
                if (objectType.GetCultureCode() == CultureCode.Aserai || objectType.GetCultureCode() == CultureCode.Battania || 
                    objectType.GetCultureCode() == CultureCode.Empire || objectType.GetCultureCode() == CultureCode.Khuzait ||
                    objectType.GetCultureCode() == CultureCode.Sturgia || objectType.GetCultureCode() == CultureCode.Vlandia)
                {
                    continue;
                }
                    
                list.Add(objectType);
            }

            __result = __result.Concat(list);
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterCreationCultureStageVM), "SortCultureList")]
        private static bool SortCultureList(MBBindingList<CharacterCreationCultureVM> listToWorkOn)
        {
            return listToWorkOn.Count > 3;
        }
    }
}