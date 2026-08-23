using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;
using Verse;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(PawnUtility), "FertileMateTarget")]
    public static class PawnUtility_FertileMateTarget_Patch
    {
        public static void Postfix(Pawn male, ref bool __result)
        {
            if (__result && male.Faction == Faction.OfPlayer)
            {
                var comp = male.GetComp<CompAnimalGenes>();
                if (comp?.feratype != null && comp.feratype.canBeAlpha)
                {
                    if (!comp.isAlpha)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
