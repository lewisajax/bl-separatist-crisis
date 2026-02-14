using SeparatistCrisis.Patches;
using SeparatistCrisis.Utils;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SeparatistCrisis.PatchTools
{
    /// <summary>
    /// Upon first and only instantiation, collects all of the active unannotated Harmony patches in its assembly and wires them.
    /// </summary>
    /// <remarks>
    /// Rather than discover Harmony patch classes via reflection, they are instantiated directly in the <see cref="PatchManager"/>.
    /// This is in order to establish a clear code reachability path to the patches in the face of an optimizing compiler/runtime.
    /// Subject to change if paranoia levels drop regarding hard-to-detect do-nothing patches thanks to the optimizer.
    /// </remarks>
    public sealed class PatchManager
    {
        public static PatchManager? MainInstance { get; private set; }
        public static PatchManager? CampaignInstance { get; private set; }
        public static PatchManager? CustomInstance { get; private set; } // Not just for CustomGame but 'custom' as in used by anything

        public static IReadOnlyList<PatchClass> MainPatchClasses => _mainPatchClasses;
        public static IReadOnlyList<PatchClass> CampaignPatchClasses => _campaignPatchClasses;

        public IReadOnlyList<PatchClass.Patch> Patches => _patches;

        public Harmony Harmony { get; init; }

        public static void ApplyMainPatches(string harmonyId)
        {
            if (MainInstance is not null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(ApplyMainPatches)} more than once!");

            if (PatchManager.MainPatchClasses == null)
                throw new InvalidOperationException("MainPatchClasses is null");

            MainInstance = new PatchManager(harmonyId, PatchManager.MainPatchClasses);
        }

        public static void ApplyCampaignPatches(string harmonyId)
        {
            CampaignInstance ??= new PatchManager(harmonyId, PatchManager.CampaignPatchClasses);
        }

        public static void ApplyCustomPatches(string harmonyId, IReadOnlyList<PatchClass> patches)
        {
            CustomInstance ??= new PatchManager(harmonyId, patches);
        }

        public static void RemoveCampaignPatches()
        {
            if (CampaignInstance is null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(RemoveCampaignPatches)} before calling {nameof(PatchManager)}.{nameof(ApplyCampaignPatches)}!");

            // Removing unannotated Harmony patches
            foreach (var patch in CampaignInstance.Patches)
            {
                patch.Remove(CampaignInstance.Harmony);
            }

            CampaignInstance = null;
        }

        public static void RemoveCustomPatches()
        {
            if (CustomInstance is null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(RemoveCustomPatches)} before calling {nameof(PatchManager)}.{nameof(ApplyCustomPatches)}!");

            // Removing unannotated Harmony patches
            foreach (var patch in CustomInstance.Patches)
            {
                patch.Remove(CustomInstance.Harmony);
            }

            CampaignInstance = null;
        }

        private PatchManager(string harmonyId, IReadOnlyList<PatchClass> patchClasses)
        {
            this.Harmony = new(harmonyId);
            _patches = patchClasses.SelectMany(pc => pc.Patches).ToArray();

            // Applying unannotated Harmony patches
            foreach (var patch in _patches)
            {
                patch.Apply(Harmony);
            }
        }

        private readonly PatchClass.Patch[] _patches;

        // REGISTER ALL ACTIVE HARMONY PATCH CLASSES TO USE OnSubModuleLoad HERE:
        private static readonly PatchClass[] _mainPatchClasses = new PatchClass[]
        {
            new OnNewGameCreatedPatch(),
            new OnNewGameCreatedPartialFollowUpPatch(),
            new GetGameKeyCategoriesListPatch(),
            new SpawnAgentPatch(),
            // new OpenCustomBattleMissionPatch(),
        };

        // REGISTER ALL ACTIVE HARMONY PATCH CLASSES TO USE OnGameStart HERE:
        private static readonly PatchClass[] _campaignPatchClasses = Array.Empty<PatchClass>();
    }
}