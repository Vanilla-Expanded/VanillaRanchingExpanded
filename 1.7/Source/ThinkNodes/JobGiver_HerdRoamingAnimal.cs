using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class JobGiver_HerdRoamingAnimal : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var target = FindRoamingAnimal(pawn);
            if (target is null)
            {
                return null;
            }
            var job = JobMaker.MakeJob(InternalDefOf.VRE_HerdAnimal, target);
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            job.expiryInterval = 3000;
            job.checkOverrideOnExpire = true;
            return job;
        }

        private static Pawn FindRoamingAnimal(Pawn herder)
        {
            foreach (var p in herder.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                if (p == herder || p.RaceProps.Animal is false || p.roping.IsRoped)
                {
                    continue;
                }
                if (p.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.Roaming)
                {
                    continue;
                }
                if (!herder.CanReserve(p) || !herder.CanReach(p, PathEndMode.Touch, Danger.None))
                {
                    continue;
                }
                return p;
            }
            return null;
        }
    }
}
