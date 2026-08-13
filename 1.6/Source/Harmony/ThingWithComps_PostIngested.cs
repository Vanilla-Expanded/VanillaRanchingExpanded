using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(ThingWithComps), "PostIngested")]
    public static class VanillaRanchingExpanded_ThingWithComps_PostIngested_Patch
    {
        public static void Postfix(ThingWithComps __instance)
        {
            WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.Remove(__instance);
        }
    }


}