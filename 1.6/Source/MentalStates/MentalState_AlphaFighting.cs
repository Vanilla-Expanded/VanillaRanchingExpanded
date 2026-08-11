using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class MentalState_AlphaFighting : MentalState
    {
        private const int AlphaFightDurationTicks = 600;
        private int fightTicks;

        public override void MentalStateTick(int delta)
        {
            base.MentalStateTick(delta);
            if (causedByPawn.Dead || causedByPawn.Downed)
            {
                RecoverFromState();
                return;
            }
            if (!pawn.pather.Moving)
            {
                fightTicks += delta;
            }
            if (fightTicks >= AlphaFightDurationTicks)
            {
                RecoverFromState();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref fightTicks, "fightTicks", 0);
        }

        public override void PostEnd()
        {
            base.PostEnd();
            pawn.jobs.StopAll();
            pawn.mindState.meleeThreat = null;
            if (causedByPawn.MentalState is MentalState_AlphaFighting otherMS && otherMS.causedByPawn == pawn)
            {
                causedByPawn.MentalState.RecoverFromState();
            }
            if (pawn.thingIDNumber < causedByPawn.thingIDNumber)
            {
                ResolveAlphaFight(causedByPawn);
            }
        }

        public void ResolveAlphaFight(Pawn other)
        {
            var comp1 = pawn.GetComp<CompAnimalGenes>();
            var comp2 = other.GetComp<CompAnimalGenes>();

            if ((pawn.Dead || pawn.Downed) && (other.Dead || other.Downed))
            {
                comp1.BecomeAlpha(false);
                comp2.BecomeAlpha(false);
                return;
            }

            Pawn winner;
            if (pawn.Dead || pawn.Downed)
            {
                winner = other;
            }
            else if (other.Dead || other.Downed)
            {
                winner = pawn;
            }
            else
            {
                var hp1 = pawn.health.summaryHealth.SummaryHealthPercent;
                var hp2 = other.health.summaryHealth.SummaryHealthPercent;
                if (hp1 > hp2)
                {
                    winner = pawn;
                }
                else if (hp2 > hp1)
                {
                    winner = other;
                }
                else
                {
                    if (comp1.isAlpha)
                    {
                        winner = pawn;
                    }
                    else if (comp2.isAlpha)
                    {
                        winner = other;
                    }
                    else
                    {
                        winner = Rand.Bool ? pawn : other;
                    }
                }
            }

            comp1.BecomeAlpha(winner == pawn);
            comp2.BecomeAlpha(winner == other);
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }

    public class JobGiver_AlphaFighting : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.MentalState is MentalState_AlphaFighting ms)
            {
                if (!SocialInteractionUtility.TryGetRandomVerbForSocialFight(pawn, out var verb))
                {
                    return null;
                }
                var job = JobMaker.MakeJob(JobDefOf.SocialFight, ms.causedByPawn);
                job.maxNumMeleeAttacks = 1;
                job.verbToUse = verb;
                return job;
            }
            return null;
        }
    }
}
