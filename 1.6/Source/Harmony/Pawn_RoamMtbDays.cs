using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using VEF.Buildings;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("RoamMtbDays", MethodType.Getter)]

    public class VanillaRanchingExpanded_Pawn_RoamMtbDays_Patch
    {
        [HarmonyPostfix]
        public static void ModifyRoamInterval(Pawn __instance, ref float? __result)
        {
            __result *= __instance.GetStatValue(InternalDefOf.VRE_RoamMTBFactor);

        }
    }
}