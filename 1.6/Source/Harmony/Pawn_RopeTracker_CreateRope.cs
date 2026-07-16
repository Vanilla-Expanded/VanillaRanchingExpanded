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

    [HarmonyPatch(typeof(Pawn_RopeTracker))]
    [HarmonyPatch("CreateRope")]
  
    public class VanillaRanchingExpanded_Pawn_RopeTracker_CreateRope_Patch
    {
        [HarmonyPostfix]
        public static void ModifyManhunterChance(Pawn ropee)
        {
            if (Rand.Chance(ropee.GetStatValue(InternalDefOf.VRE_ManhunterOnRopingChance)))
            {
                ropee.mindState.mentalStateHandler.TryStartMentalState(PawnUtility.ManhunterStateFor(ropee));
            }
           

        }
    }
}