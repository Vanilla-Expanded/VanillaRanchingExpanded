using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using VEF.AnimalGenes;
using Verse;

namespace VanillaRanchingExpanded
{
    public class WorldComponent_AnimalIllnessTracker : WorldComponent
    {
        private const int CheckInterval = GenDate.TicksPerDay;
        private const int DiseaseCooldown = 10 * GenDate.TicksPerDay;
        private int nextIllnessCheckTick = -1;

        private readonly Dictionary<HediffDef, float> diseasesAndWeights = new Dictionary<HediffDef, float>
        {
            { InternalDefOf.Animal_Flu, 0.85f },
            { InternalDefOf.VRE_Parasites, 1f },
            { InternalDefOf.VRE_ScariaInfection, 0.06f }
        };

        public WorldComponent_AnimalIllnessTracker(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nextIllnessCheckTick, "nextIllnessCheckTick", -1);
        }

        public override void WorldComponentTick()
        {
            int now = Find.TickManager.TicksGame;
            if (nextIllnessCheckTick < 0)
            {
                // New games and saves predating this component start with a full day's delay.
                nextIllnessCheckTick = now + CheckInterval;
                return;
            }
            if (now < nextIllnessCheckTick) return;

            // Advance before processing. Loading a save resumes this schedule, and an overdue
            // check runs only once rather than replaying missed days as a burst of illnesses.
            nextIllnessCheckTick = now + CheckInterval;
            CheckIllnesses();
        }

        private void CheckIllnesses()
        {
            var cases = new Dictionary<HediffDef, List<Pawn>>();
            var affectedAnimals = new List<Pawn>();
            var checkedAnimals = new HashSet<Pawn>();
            // Take a snapshot: adding a hediff can cause other mods to query PawnsFinder's
            // shared lists. Include caravans and other world pawns as the old system did.
            List<Pawn> animals = PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList();
            foreach (Pawn pawn in animals)
            {
                if (pawn == null || pawn.Dead || pawn.Destroyed || !pawn.IsColonyAnimal ||
                    !checkedAnimals.Add(pawn)) continue;
                CompAnimalGenes comp = pawn.TryGetComp<CompAnimalGenes>();
                AnimalGeneDef illnessGene = comp?.genes?.FirstOrDefault(gene => gene.isIllnessGene);
                if (illnessGene == null || comp.ticksSinceLastDisease < DiseaseCooldown) continue;
                if (!Rand.MTBEventOccurs(illnessGene.animalIllnessMTB, GenDate.TicksPerDay, CheckInterval)) continue;

                HediffDef illness = diseasesAndWeights.RandomElementByWeight(entry => entry.Value).Key;
                pawn.health.AddHediff(HediffMaker.MakeHediff(illness, pawn));
                comp.ticksSinceLastDisease = 0;
                if (!cases.TryGetValue(illness, out List<Pawn> group))
                {
                    group = new List<Pawn>();
                    cases.Add(illness, group);
                }
                group.Add(pawn);
                affectedAnimals.Add(pawn);
            }

            if (affectedAnimals.Count == 0) return;
            var text = new StringBuilder("VRE_AnimalDiseasesDesc".Translate(affectedAnimals.Count));
            foreach (KeyValuePair<HediffDef, List<Pawn>> group in cases)
            {
                text.AppendLine().AppendLine();
                text.Append(group.Key.LabelCap).Append(": ");
                text.Append(string.Join(", ", group.Value.Select(pawn => pawn.LabelShortCap.Resolve())));
            }
            Find.LetterStack.ReceiveLetter("VRE_AnimalDiseases".Translate(), text.ToString(),
                LetterDefOf.NegativeEvent, new LookTargets(affectedAnimals));
        }
    }
}
