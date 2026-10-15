using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class MentalState_AnimalTantrumFences : MentalState_TantrumRandom
    {
        protected override void GetPotentialTargets(List<Thing> outThings)
        {
            TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, outThings, GetCustomValidator());
        }

        protected override Predicate<Thing> GetCustomValidator()
        {
            return (Thing t) => t.def.IsFence;
        }
    }

    public class MentalStateWorker_AnimalTantrumFences : MentalStateWorker
    {
        private static List<Thing> tmpThings = new List<Thing>();

        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn)) return false;
            tmpThings.Clear();
            TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, tmpThings, (Thing t) => t.def.IsFence);
            var result = tmpThings.Count > 0;
            tmpThings.Clear();
            return result;
        }
    }
}
