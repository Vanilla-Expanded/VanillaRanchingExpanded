using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("ButcherProducts")]
    public class VanillaRanchingExpanded_Pawn_ButcherProducts_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Thing> AddAdditionalButcherProducts(IEnumerable<Thing> values, Pawn __instance, float efficiency)
        {
            foreach (var thing in values) yield return thing;

            if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(__instance))
            {
               
                CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[__instance];
                if (comp != null)
                {
                    foreach (AnimalGeneDef gene in comp.genes)
                    {
                        if (!gene.extraButcherProducts.NullOrEmpty())
                        {
                            foreach (ThingDefCountClass thingDefCountClass in gene.extraButcherProducts)
                            {
                               
                                float num = gene.scaleButcherProductsByMeatAmount ? __instance.GetStatValue(StatDefOf.MeatAmount)/140 * efficiency : efficiency;
                               
                                float adjustedNum = num * __instance.GetStatValue(InternalDefOf.VRE_AdditionalButcherProductsFactor);
                               
                                if (adjustedNum > 0)
                                {
                                    Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
                                    thing.stackCount = (int)(thingDefCountClass.count * adjustedNum);
                                    yield return thing;
                                }
                            }
                        }
                    }

                }
            }

        }
    }
}