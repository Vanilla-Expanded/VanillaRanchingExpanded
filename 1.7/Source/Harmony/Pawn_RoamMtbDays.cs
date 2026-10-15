using HarmonyLib;
using RimWorld;
using Verse;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{

   [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("RoamMtbDays", MethodType.Getter)]

    public class VanillaRanchingExpanded_Pawn_RoamMtbDays_Patch
    {
        [HarmonyPostfix]
        public static void ModifyRoamInterval(Pawn __instance, ref float? __result)
        {
            if (__instance.TryGetComp<CompAnimalGenes>()!=null)
            {
                __result *= __instance.GetStatValue(InternalDefOf.VRE_RoamMTBFactor, cacheStaleAfterTicks: 60);
            }

        }
    }
}