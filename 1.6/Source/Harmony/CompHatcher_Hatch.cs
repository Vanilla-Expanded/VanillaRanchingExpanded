using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;
using System.Net;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("Hatch")]
    public static class VanillaRanchingExpanded_CompHatcher_Hatch_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MoveGenesFromEggToPawn(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var moveGenesMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_CompHatcher_Hatch_Patch), "MoveGenes");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder lb && lb.LocalIndex == 8)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, moveGenesMethod);
                }


                else yield return codes[i];
            }
        }

        public static void MoveGenes(Pawn pawn, CompHatcher compHatcher)
        {
            if (pawn?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes comp) { return; }
           
            Thing egg = compHatcher.parent;
            if (egg?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes compEgg) { return; }
           
            comp.genes.Clear();

            foreach (AnimalGeneDef geneDef in compEgg.genes)
            {
                AnimalGeneUtility.AddGene(comp, geneDef);
            }
           
        }

    }
}