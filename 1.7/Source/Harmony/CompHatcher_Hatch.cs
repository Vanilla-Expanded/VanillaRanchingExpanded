using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("Hatch")]
    public static class VanillaRanchingExpanded_CompHatcher_Hatch_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MoveGenesFromEggToPawn(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
        {
            var moveGenesMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_CompHatcher_Hatch_Patch), nameof(MoveNextEggGenes));
            var generatePawn = AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn),new[] { typeof(PawnGenerationRequest) });
           
            var cursor = generator.DeclareLocal(typeof(CompGroupedEggGenes.HatchCursor));
            yield return CodeInstruction.LoadArgument(0);
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(VanillaRanchingExpanded_CompHatcher_Hatch_Patch), nameof(StartHatch)));
            yield return new CodeInstruction(OpCodes.Stloc, cursor);

            foreach (var instruction in codeInstructions)
            {
                yield return instruction;
                if (instruction.Calls(generatePawn))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldloca, cursor);
                    yield return new CodeInstruction(OpCodes.Call, moveGenesMethod);
                }
            }
        }

        private static CompGroupedEggGenes.HatchCursor StartHatch(CompHatcher compHatcher) => CompGroupedEggGenes.StartHatch(compHatcher.parent);

        private static void MoveNextEggGenes(Pawn pawn, ref CompGroupedEggGenes.HatchCursor cursor)
        {
            var genes = cursor.TakeNext();
            if (genes != null && pawn?.AnimalGenes is CompAnimalGenes comp) { comp.SetInheritedGenes(genes); }                
        }

        public static void MoveGenes(Pawn pawn, CompHatcher compHatcher)
        {
            var cursor = StartHatch(compHatcher);
            MoveNextEggGenes(pawn, ref cursor);
        }
    }
}
