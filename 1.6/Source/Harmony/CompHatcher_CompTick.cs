using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;


namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("CompTick")]

    public class VanillaRanchingExpanded_CompHatcher_CompTick_Patch
    {
        [HarmonyPostfix]
        public static void RemoveIfRuined(CompHatcher __instance)
        {
            if (__instance.TemperatureDamaged) {
                WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.Remove(__instance.parent);
            }
            
        }
    }
}