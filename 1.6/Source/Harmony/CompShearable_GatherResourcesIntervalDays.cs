using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;


namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(CompShearable))]
    [HarmonyPatch("GatherResourcesIntervalDays", MethodType.Getter)]

    public class VanillaRanchingExpanded_CompShearable_GatherResourcesIntervalDays_Patch
    {
        [HarmonyPostfix]
        public static void ModifyWoolInterval(CompMilkable __instance, ref int __result)
        {
            __result = Math.Max((int)(__result * __instance.parent.GetStatValue(InternalDefOf.VRE_WoolIntervalFactor, cacheStaleAfterTicks: 60)),1);

        }
    }
}