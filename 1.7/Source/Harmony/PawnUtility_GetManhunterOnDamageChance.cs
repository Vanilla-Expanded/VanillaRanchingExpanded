using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;


namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("GetManhunterOnDamageChance")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Thing),typeof(float) })]
    public class VanillaRanchingExpanded_PawnUtility_GetManhunterOnDamageChance_Patch
    {
        [HarmonyPostfix]
        public static void ModifyManhunterChance(Pawn pawn, ref float __result)
        {
            __result *= pawn.GetStatValue(InternalDefOf.VRE_ManhunterOnDamageFactor, cacheStaleAfterTicks: 60);
            __result += pawn.GetStatValue(InternalDefOf.VRE_ManhunterOnDamageOffset, cacheStaleAfterTicks: 60);

        }
    }
}