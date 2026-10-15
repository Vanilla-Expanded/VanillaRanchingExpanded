using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class JobDriver_HerdAnimal : JobDriver
    {
        private const int BarkDurationTicks = 120;

        private Pawn HerdTarget => (Pawn)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            var penOrdered = false;
            var fleeCell = IntVec3.Invalid;
            var chase = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => HerdTarget.roping.IsRoped);
            chase.AddEndCondition(() =>
            {
                if (penOrdered && AnimalPenUtility.GetCurrentPenOf(HerdTarget, allowUnenclosedPens: false) is not null)
                {
                    return JobCondition.Succeeded;
                }
                return JobCondition.Ongoing;
            });
            yield return chase;
            var bark = ToilMaker.MakeToil();
            bark.initAction = () =>
            {
                var target = HerdTarget;
                LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge s) => s.soundCall, (GeneDef g) => g.soundCall, (MutantDef m) => m.soundCall);
                if (target.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.Roaming)
                {
                    return;
                }
                target.mindState.mentalStateHandler.CurState.RecoverFromState();
                var pen = AnimalPenUtility.ClosestSuitablePen(target, allowUnenclosedPens: false);
                if (pen is null)
                {
                    return;
                }
                var cell = AnimalPenUtility.FindPlaceInPenToStand(pen, target);
                if (cell.IsValid is false)
                {
                    return;
                }
                fleeCell = cell;
                penOrdered = true;
                IssueFlee(target, cell);
            };
            bark.defaultCompleteMode = ToilCompleteMode.Delay;
            bark.defaultDuration = BarkDurationTicks;
            yield return bark;
            var watch = ToilMaker.MakeToil();
            watch.tickAction = () =>
            {
                var target = HerdTarget;
                if (target.roping.IsRoped || target.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Roaming)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else if (pawn.IsHashIntervalTick(30) && (penOrdered is false || AnimalPenUtility.GetCurrentPenOf(target, allowUnenclosedPens: false) is not null))
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else
                {
                    if (pawn.IsHashIntervalTick(60) && (target.CurJob?.def != InternalDefOf.VRE_FleeToPen || target.pather.Moving is false))
                    {
                        IssueFlee(target, fleeCell);
                    }

                    if (!pawn.Position.AdjacentTo8WayOrInside(target.Position))
                    {
                        JumpToToil(chase);
                    }
                }
            };
            watch.defaultCompleteMode = ToilCompleteMode.Never;
            yield return watch;
        }

        private static void IssueFlee(Pawn target, IntVec3 cell)
        {
            target.jobs.StopAll();
            target.pather.StopDead();
            var gotoPen = JobMaker.MakeJob(InternalDefOf.VRE_FleeToPen, cell);
            gotoPen.locomotionUrgency = LocomotionUrgency.Sprint;
            gotoPen.playerForced = true;
            target.jobs.StartJob(gotoPen, JobCondition.InterruptForced);
        }
    }
}
