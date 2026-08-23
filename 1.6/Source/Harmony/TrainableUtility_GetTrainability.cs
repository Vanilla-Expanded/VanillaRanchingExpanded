using HarmonyLib;
using RimWorld;
using Verse;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(TrainableUtility))]
    [HarmonyPatch("GetTrainability")]

    public class VanillaRanchingExpanded_TrainableUtility_GetTrainability_Patch
    {
        [HarmonyPostfix]
        public static void ModifyTrainability(Pawn pawn, ref TrainabilityDef __result)
        {
            if (pawn!=null&&WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                if (comp != null)
                { 
                    foreach(AnimalGeneDef gene in comp.genes)
                    {
                        if (gene.trainabilityDef != null)
                        {
                            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.SentienceCatalyst))
                            {
                                __result = gene.trainabilityDef;
                            }
                        }
                    }
                }              
            }
        }
    }
}