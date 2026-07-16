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

    [HarmonyPatch(typeof(CompShearable))]
    [HarmonyPatch("GatherResourcesIntervalDays", MethodType.Getter)]

    public class VanillaRanchingExpanded_CompShearable_GatherResourcesIntervalDays_Patch
    {
        [HarmonyPostfix]
        public static void ModifyWoolInterval(CompMilkable __instance, ref int __result)
        {
            __result = (int)Math.Max(__result * __instance.parent.GetStatValue(InternalDefOf.VRE_WoolIntervalFactor),1);

        }
    }
}