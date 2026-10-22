
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class JobDriver_RefillAnimalTrough : JobDriver_HaulToCell
    {

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);

            this.FailOn(() =>
            {
                IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;

                return !cell.IsValid ||
                    !(Map.haulDestinationManager
                        .SlotGroupParentAt(cell) is Building_AnimalTrough);
            });

            yield return Toils_Goto.GotoThing(
                TargetIndex.A,
                PathEndMode.ClosestTouch);

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            yield return Toils_Goto.GotoCell(
                TargetIndex.B,
                PathEndMode.Touch);

            yield return PlaceInTrough();
        }
       
        private Toil PlaceInTrough()
        {
            Toil toil = ToilMaker.MakeToil("PlaceInAnimalTrough");
            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
                Thing carriedThing = actor.carryTracker.CarriedThing;

                if (carriedThing == null)
                {
                    Log.Error(
                        actor + " tried to refill an animal trough without carrying anything.");

                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (!(actor.Map.haulDestinationManager
                    .SlotGroupParentAt(cell) is Building_AnimalTrough trough)
                    || !trough.AllSlotCellsList().Contains(cell)
                    || !trough.GetStoreSettings().AllowedToAccept(carriedThing))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (!actor.carryTracker.TryDropCarriedThing(
                    cell,
                    ThingPlaceMode.Direct,
                    out Thing droppedThing))
                {
                    Log.Warning(
                        "Failed to place " + carriedThing +
                        " into animal trough at " + cell + ".");

                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
            };

            return toil;
        }
    }
}
