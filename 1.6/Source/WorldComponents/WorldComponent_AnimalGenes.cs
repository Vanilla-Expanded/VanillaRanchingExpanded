using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace VanillaRanchingExpanded
{
    public class WorldComponent_AnimalGenes : WorldComponent
    {

        public static int maxStabilityPenalty = 5;

        public Dictionary<Pawn, CompAnimalGenes> pawnToCompAnimalGenes = new Dictionary<Pawn, CompAnimalGenes>();
       
        public static WorldComponent_AnimalGenes Instance;


        public WorldComponent_AnimalGenes(World world) : base(world) => Instance = this;

       
        public void AddAnimalComp(Pawn pawn, CompAnimalGenes comp)
        {
            if (!pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                pawnToCompAnimalGenes[pawn] = comp;
            }
        }

        public void RemoveAnimalComp(Pawn pawn)
        {
            if (pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                pawnToCompAnimalGenes.Remove(pawn);
            }
        }


    }
}
