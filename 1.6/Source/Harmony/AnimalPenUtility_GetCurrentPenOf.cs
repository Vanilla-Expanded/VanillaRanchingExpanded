using HarmonyLib;
using RimWorld;
using VanillaRanchingExpanded;
using Verse;
namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(AnimalPenUtility), nameof(AnimalPenUtility.GetCurrentPenOf))]
    public static class VanillaRanchingExpanded_AnimalPenUtility_GetCurrentPenOf_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn animal,ref CompAnimalPenMarker __result)
        {
            if (__result == null)
                return;

            CompAnimalAssignments compAssignments =
                __result.parent.TryGetComp<CompAnimalAssignments>();

            if (compAssignments != null &&
                !compAssignments.penAssignedAnimals.NullOrEmpty() &&
                !compAssignments.penAssignedAnimals.Contains(animal))
            {
                __result = null;
            }
        }
    }
}