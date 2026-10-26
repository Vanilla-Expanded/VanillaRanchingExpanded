using RimWorld;
using VEF.AnimalGenes;
using Verse;

namespace VanillaRanchingExpanded
{
    public class MapComponent_OldAgeTracker : MapComponent
    {     
        public int tickCounter = 0;
        public const int tickInterval = 60000;

        public MapComponent_OldAgeTracker(Map map) : base(map)
        {
        }
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            tickCounter++;
            if (tickCounter > tickInterval)
            {
                foreach (var item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
                {
                    if (item?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes comp) continue;
                    if (item is Pawn pawn)
                    {
                        float currentAge = pawn.ageTracker.AgeBiologicalYearsFloat;

                        if (currentAge > comp.LifeSpanFactor * pawn.def.race.lifeExpectancy)
                        {
                            bool hasOldAge = pawn.health.hediffSet.HasHediff(InternalDefOf.VRE_OldAge);
                            if (!hasOldAge)
                            {
                                Hediff hediff = HediffMaker.MakeHediff(InternalDefOf.VRE_OldAge, pawn);
                                pawn.health.AddHediff(hediff);
                                if (pawn.IsColonyAnimal)
                                {
                                    Find.LetterStack.ReceiveLetter("VRE_OldAge".Translate(pawn.Name.ToString()), "VRE_OldAgeDesc".Translate(pawn.Name.ToString()), LetterDefOf.NegativeEvent, pawn);
                                }
                            }

                        }
                    }

                }
               
                 tickCounter = 0;
                
            }

        }
    }
}
