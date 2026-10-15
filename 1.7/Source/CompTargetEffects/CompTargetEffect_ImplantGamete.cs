using RimWorld;
using Verse;
using Verse.AI;
using System;
using System.Linq;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    public class CompTargetEffect_ImplantGamete : CompTargetEffect
    {
        public CompProperties_TargetEffect_ImplantGamete Props
        {
            get
            {
                return (CompProperties_TargetEffect_ImplantGamete)this.props;
            }
        }

        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                AnimalGamete gamete = this.parent as AnimalGamete;
                Pawn pawn = target as Pawn;
                if (gamete != null && pawn !=null)
                {
                    Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, pawn);
                    hediff_Pregnant.SetParents(pawn, gamete.gameteFather,null);
                    pawn.health.AddHediff(hediff_Pregnant);

                    hediff_Pregnant.GetComp<HediffComp_GameteGenes>().genes = gamete.GetComp<CompAnimalGenes>().genes.ToList();
                }

                user.carryTracker.DestroyCarriedThing();
            }
        }
    }
}
