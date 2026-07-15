
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using VEF.Buildings;
using Verse;
namespace VanillaRanchingExpanded
{
    public class CompAnimalGenes : ThingComp
    {
        public FeratypeDef feratype;
        public List<AnimalGeneDef> genes = new List<AnimalGeneDef>();
        
        public new CompProperties_AnimalGenes Props => (CompProperties_AnimalGenes)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Pawn pawn = parent as Pawn;
                if (pawn != null)
                {
                    WorldComponent_AnimalGenes.Instance.AddAnimalComp(pawn, this);
                }
                ApplyFeratype();
            }
            
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            Pawn pawn = parent as Pawn;
            if (pawn != null)
            {
                WorldComponent_AnimalGenes.Instance.RemoveAnimalComp(pawn);
            }
            base.PostDestroy(mode, previousMap);

        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            Pawn pawn = parent as Pawn;
            if (pawn != null)
            {
                WorldComponent_AnimalGenes.Instance.RemoveAnimalComp(pawn);
            }
            base.PostDeSpawn(map, mode);
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
                        genes.Add(gene);
                       
                        foreach (StatModifier statModifier in gene.statFactors)
                        {
                            statModifier.stat.Worker.ClearCacheForThing(pawn);
                        }
                        foreach (StatModifier statModifier2 in gene.statOffsets)
                        {
                            statModifier2.stat.Worker.ClearCacheForThing(pawn);
                        }
                    }
                    
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref feratype, "feratype");
            Scribe_Collections.Look(ref genes, "genes");

        }
    }
}
