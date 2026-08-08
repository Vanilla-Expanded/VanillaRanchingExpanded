
using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{
    public class CompAnimalGenes : ThingComp
    {
        public FeratypeDef feratype;
        public List<AnimalGeneDef> genes = new List<AnimalGeneDef>();
        public float cachedLifespanFactor = -1;
        private bool feratypeApplied;

        public new CompProperties_AnimalGenes Props => (CompProperties_AnimalGenes)props;

        public float LifeSpanFactor
        {
            get
            {
                if (cachedLifespanFactor == -1)
                {
                    int totalStability = 0;
                    foreach (AnimalGeneDef gene in genes)
                    {
                        totalStability += gene.stability;
                    }
                    
                    cachedLifespanFactor = (float)(1 - (0.5/Math.Log(21)*Math.Log(Math.Abs(totalStability)+1)));
                }
                return cachedLifespanFactor;
            }       
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
           
            Pawn pawn = parent as Pawn;
            if (pawn != null)
            {
                WorldComponent_AnimalGenes.Instance.AddAnimalComp(pawn, this);
            }
            if (!respawningAfterLoad && !feratypeApplied)
            {
                ApplyFeratype();
                feratypeApplied = true;
            }

        }

        public void ApplyFeratype()
        {
            Pawn pawn = parent as Pawn;
            foreach (FeratypeDef feratypeIterator in DefDatabase<FeratypeDef>.AllDefsListForReading)
            {
                if(feratypeIterator.race == pawn.kindDef)
                {
                    feratype = feratypeIterator;
                    foreach(AnimalGeneDef gene in feratype.animalGenes)
                    {
                        AnimalGeneUtility.AddGene(this,gene, pawn);
                    }
                    AnimalGeneUtility.HandleMutations(this,pawn);


                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref feratype, "feratype");
            Scribe_Values.Look(ref feratypeApplied, "feratypeApplied");
            Scribe_Collections.Look(ref genes, "genes", LookMode.Def);

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            if (DebugSettings.ShowDevGizmos)
            {

                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Do birth";
                command_Action.action = delegate
                {
                    Pawn pawn = parent as Pawn;
                    Hediff pregnantHediff = pawn?.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Pregnant);
                    if (pregnantHediff != null) {
                        pregnantHediff.Severity = 1;
                    }else Messages.Message("VRE_NotPregnant".Translate(), pawn, MessageTypeDefOf.RejectInput);

                };
                yield return command_Action;


            }
        }

        
    }
}
