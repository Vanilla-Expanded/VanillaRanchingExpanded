using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{
    public class CompAnimalAssignments : ThingComp
    {
        public HashSet<Pawn> penAssignedAnimals = new HashSet<Pawn>();

        public CompProperties_AnimalAssignments Props => props as CompProperties_AnimalAssignments;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref penAssignedAnimals, "penAssignedAnimals", LookMode.Reference);
        }

        public override void CompTickLong()
        {
            base.CompTickLong();

            List<Pawn> animalsToRemove = new List<Pawn>();
            foreach(Pawn p in penAssignedAnimals)
            {
                if (p.Dead)
                {
                    animalsToRemove.Add(p);
                }
            }   
            foreach(Pawn p in animalsToRemove)
            {
                penAssignedAnimals.Remove(p);
            }
        }
    }
}