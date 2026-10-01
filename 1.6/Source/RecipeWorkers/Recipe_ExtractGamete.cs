
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VEF.AnimalGenes;
using Verse;
namespace VanillaRanchingExpanded
{
    public class Recipe_ExtractGamete : Recipe_AddHediff
    {
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
           
            Pawn pawn = thing as Pawn;
            if (pawn == null)
            {
                return false;
            }
            if ((recipe.genderPrerequisite ?? pawn.gender) != pawn.gender)
            {
                return false;
            }

            if (pawn.TryGetComp<CompAnimalGenes>() is null)
            {
                return "VRE_NotACompatibleAnimal".Translate();
            }

            if (pawn.TryGetComp<CompEggLayer>() is not null)
            {
                return "VRE_NotEggLayers".Translate();
            }

            if (pawn.ageTracker.AgeBiologicalYears < recipe.minAllowedAge)
            {
                return "CannotMustBeAge".Translate(recipe.minAllowedAge);
            }
            if (pawn.Sterile())
            {
                return "CannotSterile".Translate();
            }
            if (pawn.health.hediffSet.HasHediff(InternalDefOf.VRE_GameteExtracted))
            {
                return "VRE_SurgeryDisableReasonGameteExtracted".Translate();
            }
            return base.AvailableReport(thing, part);
        }

        public override bool CompletableEver(Pawn surgeryTarget)
        {
            return IsValidNow(surgeryTarget, null, ignoreBills: true);
        }

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            AnimalGamete thing = ThingMaker.MakeThing(InternalDefOf.VRE_AnimalGamete) as AnimalGamete;
            thing.gameteFather = pawn;
            CompAnimalGenes pawnComp = pawn.TryGetComp<CompAnimalGenes>();
            CompAnimalGenes gameteComp = thing.TryGetComp<CompAnimalGenes>();
            if (pawnComp != null && gameteComp != null) { 
                gameteComp.genes = pawnComp.genes.ToList();
                gameteComp.feratype = pawnComp.feratype;
            }
            if (!GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, null, (IntVec3 x) => x.InBounds(pawn.Map) && x.Standable(pawn.Map) && !x.Fogged(pawn.Map)))
            {
                Log.Error("Could not drop gamete near " + pawn.Position.ToString());
            }
        }
    }
}