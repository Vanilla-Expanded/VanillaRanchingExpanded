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

    [HarmonyPatch(typeof(StatWorker))]
    [HarmonyPatch("GetValueUnfinalized")]
    public class VanillaRanchingExpanded_StatWorker_GetValueUnfinalized_Patch
    {
        [HarmonyPostfix]
        public static void ApplyStatModifiers(StatDef ___stat, StatRequest req, ref float __result)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
                {
                    CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                    if (comp != null) {
                        List<AnimalGeneDef> genesListForReading = comp.genes;
                        for (int num = 0; num < genesListForReading.Count; num++)
                        {

                            __result += genesListForReading[num].statOffsets.GetStatOffsetFromList(___stat);
                            __result *= genesListForReading[num].statFactors.GetStatFactorFromList(___stat);
                        }
                    }
                    
                }
                
            }

        }
    }
}