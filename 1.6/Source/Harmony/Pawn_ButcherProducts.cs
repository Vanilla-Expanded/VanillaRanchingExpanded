using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("ButcherProducts")]
    public class VanillaRanchingExpanded_Pawn_ButcherProducts_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Thing> AddAdditionalButcherProducts(IEnumerable<Thing> values, Pawn __instance,  float efficiency)
        {
            foreach (var thing in values) yield return thing;

            if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(__instance))
            {
                CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[__instance];
                if (comp != null)
                {
                    foreach (AnimalGeneDef motherAnimalGene in comp.genes)
                    {
                        if (!motherAnimalGene.extraButcherProducts.NullOrEmpty())
                        {
                            foreach (ThingDefCountClass thingDefCountClass in motherAnimalGene.extraButcherProducts)
                            {

                                float num = motherAnimalGene.scaleButcherProductsByMeatAmount ? GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.MeatAmount) * efficiency) : efficiency;
                                float adjustedNum = num * __instance.GetStatValue(InternalDefOf.VRE_AdditionalButcherProductsFactor);

                                if (adjustedNum > 0)
                                {
                                    Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
                                    thing.stackCount = thingDefCountClass.count;
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