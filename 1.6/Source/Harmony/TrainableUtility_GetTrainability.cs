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

    [HarmonyPatch(typeof(TrainableUtility))]
    [HarmonyPatch("GetTrainability")]

    public class VanillaRanchingExpanded_TrainableUtility_GetTrainability_Patch
    {
        [HarmonyPostfix]
        public static void ModifyTrainability(Pawn pawn, ref TrainabilityDef __result)
        {
            if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
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