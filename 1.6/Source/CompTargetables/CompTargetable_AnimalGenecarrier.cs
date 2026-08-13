
using RimWorld;
using System.Collections.Generic;
using Verse;


namespace VanillaRanchingExpanded
{
    public class CompTargetable_AnimalGenecarrier : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false
            };
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing != null && WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(target.Thing))
            {
                return true;
            }
            return false;
        }
    }
}