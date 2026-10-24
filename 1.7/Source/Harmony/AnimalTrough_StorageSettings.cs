using HarmonyLib;
using RimWorld;

namespace VanillaRanchingExpanded
{
    // Extend the standard clipboard, so copying from ordinary storage clears a stale trough target.
    [HarmonyPatch(typeof(StorageSettingsClipboard))]
    public static class AnimalTrough_StorageClipboard
    {
        private static float? copiedNutritionTarget;

        internal static Building_AnimalTrough FindTrough(StorageSettings settings)
        {
            if (settings?.owner is Building_AnimalTrough trough) return trough;
            if (settings?.owner is StorageGroup group)
                foreach (IStorageGroupMember member in group.members)
                    if (member is Building_AnimalTrough linkedTrough) return linkedTrough;
            return null;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(StorageSettingsClipboard.Copy))]
        public static void Copy(StorageSettings s)
        {
            copiedNutritionTarget = FindTrough(s)?.nutritionTarget;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(StorageSettingsClipboard.PasteInto))]
        public static void Paste(StorageSettings s)
        {
            if (copiedNutritionTarget.HasValue)
                FindTrough(s)?.SetNutritionTarget(copiedNutritionTarget.Value);
        }
    }

    [HarmonyPatch(typeof(StorageGroupUtility), nameof(StorageGroupUtility.SetStorageGroup))]
    public static class AnimalTrough_LinkStorage
    {
        public static void Prefix(IStorageGroupMember member, StorageGroup newGroup, out float? __state)
        {
            __state = member is Building_AnimalTrough trough ? trough.nutritionTarget : null;
            if (__state.HasValue && newGroup != null)
                foreach (IStorageGroupMember existing in newGroup.members)
                    if (existing is Building_AnimalTrough linkedTrough)
                    {
                        __state = linkedTrough.nutritionTarget;
                        break;
                    }
        }

        public static void Postfix(IStorageGroupMember member, float? __state)
        {
            if (__state.HasValue && member is Building_AnimalTrough trough)
                trough.SetNutritionTarget(__state.Value);
        }
    }
}
