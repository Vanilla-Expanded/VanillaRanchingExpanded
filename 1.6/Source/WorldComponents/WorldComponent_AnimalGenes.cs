using RimWorld.Planet;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VanillaRanchingExpanded;

namespace VEF.Buildings
{
    public class WorldComponent_AnimalGenes : WorldComponent
    {

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
