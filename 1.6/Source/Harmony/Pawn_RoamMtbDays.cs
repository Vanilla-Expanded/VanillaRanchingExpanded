using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

   [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("RoamMtbDays", MethodType.Getter)]

    public class VanillaRanchingExpanded_Pawn_RoamMtbDays_Patch
    {
        [HarmonyPostfix]
        public static void ModifyRoamInterval(Pawn __instance, ref float? __result)
        {
            if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(__instance))
            {
                __result *= __instance.GetStatValue(InternalDefOf.VRE_RoamMTBFactor);
            }

        }
    }
}