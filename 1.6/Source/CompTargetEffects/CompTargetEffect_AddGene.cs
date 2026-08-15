using RimWorld;
using Verse;
using Verse.AI;
using System;
using System.Linq;

namespace VanillaRanchingExpanded
{
    public class CompTargetEffect_AddGene : CompTargetEffect
    {
        public CompProperties_TargetEffect_AddGene Props
        {
            get
            {
                return (CompProperties_TargetEffect_AddGene)this.props;
            }
        }

        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                AnimalGeneInjector kit = this.parent as AnimalGeneInjector;
                if (kit != null)
                {
                    AnimalGeneDef gene = kit.animalGene;
                   
                    CompAnimalGenes comp = target.TryGetComp<CompAnimalGenes>();
                    if (comp != null) {
                        AnimalGeneUtility.AddGeneRespectingFamily(comp, gene);
                    }
                    else
                    {
                        Messages.Message("VRE_CantApply".Translate(gene.LabelCap, target.LabelCap), target,
                            MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                }
               
                user.carryTracker.DestroyCarriedThing();
            }
        }
    }
}
