using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("HealthScale", MethodType.Getter)]
    public class VanillaRanchingExpanded_Pawn_HealthScale_Patch
    {
        [HarmonyPostfix]
        public static void ApplyHealthScaleStat(Pawn __instance, ref float __result)
        {
            __result *= __instance.GetStatValue(InternalDefOf.VRE_HealthFromGenesScale);

        }
    }
}