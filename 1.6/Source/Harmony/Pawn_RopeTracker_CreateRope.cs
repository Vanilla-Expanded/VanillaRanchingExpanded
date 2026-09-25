using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn_RopeTracker))]
    [HarmonyPatch("CreateRope")]
  
    public class VanillaRanchingExpanded_Pawn_RopeTracker_CreateRope_Patch
    {
        [HarmonyPostfix]
        public static void ModifyManhunterChance(Pawn ropee)
        {
            if (Rand.Chance(ropee.GetStatValue(InternalDefOf.VRE_ManhunterOnRopingChance, cacheStaleAfterTicks: 60)))
            {
                ropee.mindState.mentalStateHandler.TryStartMentalState(PawnUtility.ManhunterStateFor(ropee));
            }
           

        }
    }
}