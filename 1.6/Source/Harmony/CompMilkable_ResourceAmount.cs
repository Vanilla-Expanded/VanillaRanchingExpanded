using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(CompMilkable))]
    [HarmonyPatch("ResourceAmount", MethodType.Getter)]

    public class VanillaRanchingExpanded_CompMilkable_ResourceAmount_Patch
    {
        [HarmonyPostfix]
        public static void ModifyMilkYield(CompMilkable __instance, ref int __result)
        {
            __result = (int)(__result*__instance.parent.GetStatValue(InternalDefOf.VRE_MilkYieldFactor));

        }
    }
}