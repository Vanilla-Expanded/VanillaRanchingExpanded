using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;
using System.Reflection;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch]
    public static class VanillaRanchingExpanded_RaceProperties_SpecialDisplayStats_Patch
    {
        public static MethodInfo moveNext;

        public static MethodBase TargetMethod()
        {
            moveNext = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(RaceProperties), "SpecialDisplayStats"));
            return moveNext;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ModifyLifespan(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var lifeExpectancyField = AccessTools.Field(typeof(RaceProperties), "lifeExpectancy");
            var reqField = AccessTools.Field(moveNext.DeclaringType, "req");
 
            var adjustLifeExpectancyMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_RaceProperties_SpecialDisplayStats_Patch), "AdjustLifeExpectancy");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(lifeExpectancyField))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, reqField);
                    yield return new CodeInstruction(OpCodes.Call, adjustLifeExpectancyMethod);
                }
               
                else yield return codes[i];
            }
        }

        public static float AdjustLifeExpectancy(float expectancy, StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn?.TryGetComp<CompAnimalGenes>() is  CompAnimalGenes comp) {
               
                return expectancy * comp.LifeSpanFactor;
                
            }

            return expectancy;
        }

      

    }
}