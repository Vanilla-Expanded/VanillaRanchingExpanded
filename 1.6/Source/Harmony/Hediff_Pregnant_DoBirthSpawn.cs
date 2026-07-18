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
            var targetMethod = AccessTools.Method(typeof(TaleRecorder), "RecordTale");

            var method = AccessTools.Method(typeof(VanillaRanchingExpanded_Hediff_Pregnant_DoBirthSpawn_Patch), "AdjustGenes");

            for (var i = 0; i < codes.Count; i++)
            {

                if (i > 0 && codes[i - 1].opcode == OpCodes.Call && codes[i-1].OperandIs(targetMethod) )
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, method);
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

                    comp.genes.Add(DefDatabase<AnimalGeneDef>.AllDefsListForReading.Where(x => x.familyTag == motherAnimalGene.familyTag && x.GeneLevel == finalScore).FirstOrDefault());
                }
            }
            //Random mutations handling
            HandleMutations(comp);
        }

        public static void HandleMutations(CompAnimalGenes comp)
        {
            int amountOfMutations = 0;
            float roll = Rand.Value;

            if (roll < 0.0005f)
                amountOfMutations = 5;
            else if (roll < 0.0021f) // 0.0005 + 0.0016
                amountOfMutations = 4;
            else if (roll < 0.0066f) // + 0.0045
                amountOfMutations = 3;
            else if (roll < 0.0196f) // + 0.013
                amountOfMutations = 2;
            else if (roll < 0.0616f) // + 0.042 
                amountOfMutations = 1;
            else
                amountOfMutations = 0;

            if (amountOfMutations > 0)
            {
                List<AnimalGeneDef> mutatedGenes = comp.genes.TakeRandom(amountOfMutations).ToList();
                foreach (AnimalGeneDef mutatedGene in mutatedGenes)
                {
                    bool goingUpOrDown = Rand.Chance(0.5f);
                    int geneLevel = mutatedGene.GeneLevel;
                    AnimalGeneFamilyTagDef family = mutatedGene.familyTag;
                    int newGeneLevel = goingUpOrDown ? Math.Min(mutatedGene.GeneLevel + 1, 5) : Math.Max(mutatedGene.GeneLevel - 1, 1);
                    comp.genes.RemoveWhere(x => x.familyTag == family && x.GeneLevel == geneLevel);
                    AnimalGeneDef newGene = DefDatabase<AnimalGeneDef>.AllDefsListForReading.Where(x => x.familyTag == family && x.GeneLevel == newGeneLevel).FirstOrDefault();
                    comp.genes.Add(newGene);
                }
            }

        }

    }
}