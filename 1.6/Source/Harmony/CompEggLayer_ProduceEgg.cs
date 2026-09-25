using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;


namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(CompEggLayer))]
    [HarmonyPatch("ProduceEgg")]

    public class VanillaRanchingExpanded_CompEggLayer_ProduceEgg_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeEggYields(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
          
            var yieldMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_CompEggLayer_ProduceEgg_Patch), "ChangeEggYield");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, yieldMethod);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }


                else yield return codes[i];
            }
        }

        public static int ChangeEggYield(int baseYield, CompEggLayer compEgglayer)
        {
            Pawn pawn = compEgglayer.parent as Pawn;
            if (pawn != null)
            {
                return (int)Math.Max(baseYield + pawn.GetStatValue(InternalDefOf.VRE_EggYieldOffset, cacheStaleAfterTicks: 60),1);
            }
            return baseYield;
        }


        
    }
}