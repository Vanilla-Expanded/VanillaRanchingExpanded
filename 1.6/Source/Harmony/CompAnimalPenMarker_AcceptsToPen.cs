using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(CompAnimalPenMarker))]
    [HarmonyPatch("AcceptsToPen")]

    public class VanillaRanchingExpanded_CompAnimalPenMarker_AcceptsToPen_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(Pawn animal, ref bool __result, CompAnimalPenMarker __instance)
        {
            if (__result)
            {
                CompAnimalAssignments compAssignments = __instance.parent.TryGetComp<CompAnimalAssignments>();
                if (!compAssignments.penAssignedAnimals.NullOrEmpty())
                {
                    if (!compAssignments.penAssignedAnimals.Contains(animal))
                    {
                        __result = false;
                    }

                }
            }


        }
    }
}