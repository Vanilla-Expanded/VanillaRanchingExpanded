
using RimWorld;
using System.Collections.Generic;
using Verse;
using VEF.AnimalGenes;


namespace VanillaRanchingExpanded
{
    public class CompTargetable_FemaleAnimalGenecarrier : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = false,
                mapObjectTargetsMustBeAutoAttackable = false
            };
        }

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Pawn pawn = target.Thing as Pawn;
            AnimalGamete gamete = parent as AnimalGamete;
            if (pawn is null || gamete is null)
            {
                return false;
            }
            if (pawn.IsColonist)
            {
                if (showMessages)
                {
                    Messages.Message("VRE_YouNeedHelp".Translate(), pawn, MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            if (pawn.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes pawnComp || gamete.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes gameteComp)
            {
                return false;
            }
            if (pawn.gender!=Gender.Female)
            {
                if (showMessages)
                {
                    Messages.Message("VRE_OnlyFemale".Translate(), pawn, MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            if (pawn.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant)==true)
            {
                if (showMessages)
                {
                    Messages.Message("VRE_AlreadyPregnant".Translate(), pawn, MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            if (pawnComp.feratype.feratypeFamily != gameteComp.feratype.feratypeFamily)
            {
                if (showMessages)
                {
                    Messages.Message("VRE_DifferentFeratype".Translate(), pawn, MessageTypeDefOf.RejectInput);
                    return false;
                }
            }

            return true;
        }
    }
}