using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("GetManhunterOnTameFailChance")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public class VanillaRanchingExpanded_PawnUtility_GetManhunterOnTameFailChance_Patch
    {
        [HarmonyPostfix]
        public static void ModifyManhunterChance(Pawn pawn, ref float __result)
        {
            __result *= pawn.GetStatValue(InternalDefOf.VRE_ManhunterOnTameFailFactor);
            __result += pawn.GetStatValue(InternalDefOf.VRE_ManhunterOnTameFailOffset);
        }
    }
}