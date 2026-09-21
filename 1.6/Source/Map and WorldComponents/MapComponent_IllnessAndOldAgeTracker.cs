using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEF.AnimalGenes;
using Verse;
using static HarmonyLib.Code;

namespace VanillaRanchingExpanded
{
    public class MapComponent_IllnessAndOldAgeTracker : MapComponent
    {

        public Dictionary<HediffDef, float> diseasesAndWeights = new Dictionary<HediffDef, float>() { { InternalDefOf.Animal_Flu, 0.85f },
        { InternalDefOf.VRE_Parasites, 1f },{ InternalDefOf.VRE_ScariaInfection, 0.06f }};

        public int tickCounter = 0;
        public const int tickInterval = 60000;

        public MapComponent_IllnessAndOldAgeTracker(Map map) : base(map)
        {
        }
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            tickCounter++;
            if (tickCounter > tickInterval)
            {
                Dictionary<Thing, CompAnimalGenes> allGeneticAnimals = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes;

                foreach (KeyValuePair<Thing, CompAnimalGenes> entry in allGeneticAnimals)
                {
                    if (entry.Key is Pawn pawn)
                    {
                        if (pawn.IsPlayerControlled) {

                            AnimalGeneDef filthAnimalGene = entry.Value.genes?.Where(x => x.isIllnessGene).FirstOrDefault();
                            if (filthAnimalGene != null)
                            {

                                bool illnessHappens = Rand.MTBEventOccurs(filthAnimalGene.animalIllnessMTB, 60000, 60000);

                                if (illnessHappens)
                                {
                                    HediffDef chosenIllness = diseasesAndWeights.RandomElementByWeight(x => x.Value).Key;
                                    if (chosenIllness != null)
                                    {
                                        Hediff hediff = HediffMaker.MakeHediff(chosenIllness, pawn);
                                        pawn.health.AddHediff(hediff);
                                        Find.LetterStack.ReceiveLetter("VRE_AnimalDisease".Translate(hediff.def.LabelCap), "VRE_AnimalDiseaseDesc".Translate(pawn.Name.ToString(), hediff.def.LabelCap), LetterDefOf.NegativeEvent, pawn);

                                    }
                                }
                            }
                        }
                        

                        float currentAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                        
                        if (currentAge > entry.Value.LifeSpanFactor * pawn.def.race.lifeExpectancy)
                        {
                            bool hasOldAge = pawn.health.hediffSet.HasHediff(InternalDefOf.VRE_OldAge);
                            if (!hasOldAge)
                            {
                                Hediff hediff = HediffMaker.MakeHediff(InternalDefOf.VRE_OldAge, pawn);
                                pawn.health.AddHediff(hediff);
                                Find.LetterStack.ReceiveLetter("VRE_OldAge".Translate(pawn.Name.ToString()), "VRE_OldAgeDesc".Translate(pawn.Name.ToString()), LetterDefOf.NegativeEvent, pawn);

                            }

                        }
                    }
                    tickCounter = 0;
                }
            }

        }
    }
}
