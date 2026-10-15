using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(CompAnimalPenMarker), nameof(CompAnimalPenMarker.AcceptsToPen))]
    public class CompAnimalPenMarker_AcceptsToPen_Patch
    {
        public static void Postfix(Pawn animal, ref bool __result, CompAnimalPenMarker __instance)
        {
            var comp = __instance.parent.GetComp<CompAnimalAssignments>();
            if (comp == null) return;

            if (comp.forceExcluded.Contains(animal))
            {
                __result = false;
            }
            else if (comp.forceIncluded.Contains(animal) && AnimalPenUtility.GetFixedAnimalFilter().Allows(animal.def))
            {
                __result = true;
            }
        }
    }
}
