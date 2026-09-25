using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;
using System.Net;

namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(CompEggLayer))]
    [HarmonyPatch("CompTick")]
    public static class VanillaRanchingExpanded_CompEggLayer_CompTick_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeEggIntervals(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var field = AccessTools.Field(typeof(CompProperties_EggLayer), "eggLayIntervalDays");
            var intervalMethod = AccessTools.Method(typeof(VanillaRanchingExpanded_CompEggLayer_CompTick_Patch), "ChangeEggInterval");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(field))
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, intervalMethod);
                }


                else yield return codes[i];
            }
        }

        public static float ChangeEggInterval(float baseEggLayIntervalDays, CompEggLayer compEgglayer)
        {
            Pawn pawn = compEgglayer.parent as Pawn;
            if (pawn != null) {
                return baseEggLayIntervalDays * pawn.GetStatValue(InternalDefOf.VRE_EggIntervalFactor,cacheStaleAfterTicks: 60);
            }return baseEggLayIntervalDays;
        }

    }
}