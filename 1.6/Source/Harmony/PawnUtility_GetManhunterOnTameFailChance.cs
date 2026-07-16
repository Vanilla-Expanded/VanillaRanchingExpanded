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