using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Corpse), nameof(Corpse.SpawnSetup))]
    public static class VanillaRanchingExpanded_Corpse_SpawnSetup_Patch
    {
        public static void Postfix(Corpse __instance, bool respawningAfterLoad)
        {
            Pawn pawn = __instance.InnerPawn;

            if (pawn == null)
                return;

            CompAnimalGenes comp = pawn.TryGetComp<CompAnimalGenes>();

            if (comp != null)
            {
                WorldComponent_AnimalGenes.Instance.AddAnimalComp(pawn, comp);
            }
        }
    }
}