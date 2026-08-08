using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(Hediff_Pregnant))]
    [HarmonyPatch("DoBirthSpawn")]
    public static class VanillaRanchingExpanded_Hediff_Pregnant_DoBirthSpawn_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddCrossbreedGenes(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var adjustGenesTargetMethod = AccessTools.Method(typeof(TaleRecorder), "RecordTale");
            var adjustLitterSizeTargetMethod = AccessTools.Method(typeof(Rand), "ByCurve");

            var adjustGenesMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_Hediff_Pregnant_DoBirthSpawn_Patch), "AdjustGenes");
            var adjustLitterSizeMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_Hediff_Pregnant_DoBirthSpawn_Patch), "AdjustLitterSize");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i - 1].opcode == OpCodes.Call && codes[i-1].OperandIs(adjustGenesTargetMethod) )
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, adjustGenesMethod);
                }else
                if (codes[i].opcode == OpCodes.Call && codes[i].OperandIs(adjustLitterSizeTargetMethod))
                {


                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, adjustLitterSizeMethod);
                    yield return codes[i];
                }
                else yield return codes[i];
            }
        }

        public static void AdjustGenes(Pawn pawn, Pawn mother, Pawn father)
        {
           
            if (!WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn)){return;}
            CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
            if (comp is null){ return; }
            if (mother is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(mother)){return;}
            CompAnimalGenes compMother = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[mother];
            if (compMother is null) { return; }
            if (father is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(father))
            {               
                comp.genes = compMother.genes;
                return;
            }
            CompAnimalGenes compFather = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[father];
            if (compFather is null) { return; }

            comp.genes.Clear();
            List<AnimalGeneDef> motherGenes = compMother.genes;
            List<AnimalGeneDef> fatherGenes = compFather.genes;

            int totalMotherStability = 0;
            foreach (AnimalGeneDef gene in compMother.genes)
            {
                totalMotherStability += gene.stability;
            }
            int totalFatherStability = 0;
            foreach (AnimalGeneDef gene in compFather.genes)
            {
                totalFatherStability += gene.stability;
            }

            float avgStability = (float)(totalMotherStability + totalFatherStability) / 2;
            float pullFactor = 0;
            if (avgStability < 0)
            {
                pullFactor = Math.Min(Math.Abs(avgStability) / WorldComponent_AnimalGenes.maxStabilityPenalty,1);
            }

            foreach (AnimalGeneDef motherAnimalGene in motherGenes)
            {
                if(!motherAnimalGene.isSpecialized || fatherGenes.Where(x=> x.familyTag == motherAnimalGene.familyTag).Any())
                {
                    AnimalGeneDef fatherAnimalGene = fatherGenes.Where(x => x.familyTag == motherAnimalGene.familyTag).FirstOrDefault();
                    float rawScore = (float)(motherAnimalGene.GeneLevel + fatherAnimalGene.GeneLevel) / 2;          

                    int finalScore = (int)Math.Round(rawScore + (3 - rawScore) * pullFactor, MidpointRounding.AwayFromZero);

                    AnimalGeneUtility.AddGene(comp, DefDatabase<AnimalGeneDef>.AllDefsListForReading.Where(x => x.familyTag == motherAnimalGene.familyTag && x.GeneLevel == finalScore).FirstOrDefault(), pawn);
                   
                }

                if (!Find.Storyteller.difficulty.babiesAreHealthy && motherAnimalGene.stillbirthChance>0)
                {
                    if (Rand.Chance(motherAnimalGene.stillbirthChance))
                    {
                        Hediff culpritHediff = pawn.health.AddHediff(InternalDefOf.VRE_Stillborn);                      
                        Find.BattleLog.Add(new BattleLogEntry_StateTransition(pawn, pawn.RaceProps.DeathActionWorker.DeathRules, null, culpritHediff, null));
                    }

                }
            }
            //Random mutations handling
            if (!pawn.Dead)
            {
                AnimalGeneUtility.HandleMutations(comp, pawn);
            }
            
        }

        public static SimpleCurve AdjustLitterSize(SimpleCurve existingCurve,Pawn mother)
        {

            if (mother is null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(mother)) { return existingCurve; }
            CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[mother];
            if (comp != null) {
                foreach (AnimalGeneDef motherAnimalGene in comp.genes)
                {
                    if (motherAnimalGene.litterSizeCurveOverride!=null)
                    {
                        return motherAnimalGene.litterSizeCurveOverride;
                    }
                }
            }
            return existingCurve;
        }

    }
}