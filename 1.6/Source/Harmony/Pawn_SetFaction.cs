using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("SetFaction")]

    public class VanillaRanchingExpanded_Pawn_SetFaction_Patch
    {
        [HarmonyPostfix]
        public static void AddTrainables(Pawn __instance, Faction newFaction)
        {
            if (WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(__instance))
            {
                if (__instance.IsAnimal && newFaction == Faction.OfPlayer)
                {
                    CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[__instance];
                    foreach(AnimalGeneDef gene in comp.genes)
                    {
                        if (gene.trainableDef != null)
                        {

                        }
                    }
                }

            }
            


        }
    }
}