using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

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

      

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksGame % 2000 != 0)
                return;

            List<Pawn> toRemove = null;

            foreach (Pawn pawn in pawnToCompAnimalGenes.Keys)
            {
                if (pawn.Dead && pawn.Corpse == null)
                {
                    toRemove ??= new List<Pawn>();
                    toRemove.Add(pawn);
                }
            }

            if (toRemove != null)
            {
                foreach (Pawn pawn in toRemove)
                {
                    pawnToCompAnimalGenes.Remove(pawn);
                }
            }
        }


    }
}
