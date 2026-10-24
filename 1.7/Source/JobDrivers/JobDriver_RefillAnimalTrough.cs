using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class JobDriver_RefillAnimalTrough : JobDriver_HaulToCell
    {
        private Building_AnimalTrough Trough => job.GetTarget(TargetIndex.B).Cell.IsValid
            ? Map.haulDestinationManager.SlotGroupParentAt(job.GetTarget(TargetIndex.B).Cell) as Building_AnimalTrough
            : null;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Building_AnimalTrough trough = Trough;
            return trough != null && base.TryMakePreToilReservations(errorOnFailed) &&
                pawn.Reserve(trough, job, errorOnFailed: errorOnFailed) && trough.TryStartRefill();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AddFinishAction(_ => DropRemainderOutsideTroughs());
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Trough == null || !Trough.CanContinueRefill());
            this.FailOnForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
            yield return PlaceInTrough();
        }

        private Toil PlaceInTrough()
        {
            Toil toil = ToilMaker.MakeToil("PlaceInAnimalTrough");
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                Thing carried = pawn.carryTracker.CarriedThing;
                Building_AnimalTrough trough = Trough;
                if (carried == null || trough == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
                int count = trough.DeliveryCount(carried, cell);
                if (count <= 0)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                float perItem = carried.GetStatValue(StatDefOf.Nutrition);
                // The target/filter/contents can change while walking. Drop only what still fits;
                // the finish action drops any remainder outside troughs.
                if (!pawn.carryTracker.TryDropCarriedThing(cell, count, ThingPlaceMode.Direct, out _,
                    (placed, placedCount) => trough.Notify_Delivered(perItem * placedCount)))
                    EndJobWith(JobCondition.Incompletable);
            };
            return toil;
        }

        private void DropRemainderOutsideTroughs()
        {
            Thing carried = pawn.carryTracker.CarriedThing;
            if (carried == null || !pawn.Spawned) return;
            pawn.carryTracker.innerContainer.TryDrop(carried, pawn.Position, Map, ThingPlaceMode.Near,
                out _, nearPlaceValidator: cell =>
                    !(Map.haulDestinationManager.SlotGroupParentAt(cell) is Building_AnimalTrough));
        }
    }
}
