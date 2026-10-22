
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class WorkGiver_RefillAnimalTrough : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Job JobOnThing(Pawn pawn,Thing thing,bool forced = false)
        {

           
            if (thing is not Building_AnimalTrough trough)
            {
                return null;
            }

            return trough.TryMakeRefillJob(pawn, forced);
        }
    }
}
