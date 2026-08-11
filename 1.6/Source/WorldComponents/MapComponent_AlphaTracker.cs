using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public enum TriggerType
    {
        None,
        PowerVacuum,
        NewChallenger,
        Periodic
    }

    public class PenFamilyState : IExposable
    {
        public ThingWithComps penMarker;
        public string family;
        public int cooldownUntilTick = -1;
        public int fightTargetTick = -1;
        public TriggerType pendingTrigger;
        public List<Pawn> challengers = new List<Pawn>();
        public List<Pawn> knownMales = new List<Pawn>();

        public void ExposeData()
        {
            Scribe_References.Look(ref penMarker, "penMarker");
            Scribe_Values.Look(ref family, "family");
            Scribe_Values.Look(ref cooldownUntilTick, "cooldownUntilTick", -1);
            Scribe_Values.Look(ref fightTargetTick, "fightTargetTick", -1);
            Scribe_Values.Look(ref pendingTrigger, "pendingTrigger", TriggerType.None);
            Scribe_Collections.Look(ref challengers, "challengers", LookMode.Reference);
            Scribe_Collections.Look(ref knownMales, "knownMales", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                challengers ??= new List<Pawn>();
                knownMales ??= new List<Pawn>();
                challengers.RemoveAll(x => x == null || x.Destroyed);
                knownMales.RemoveAll(x => x == null || x.Destroyed);
            }
        }

        public void TryStartCountdown(int delayTicks)
        {
            fightTargetTick = Find.TickManager.TicksGame + delayTicks;
        }

        public void CancelPendingFight()
        {
            pendingTrigger = TriggerType.None;
            fightTargetTick = -1;
        }
    }

    public class MapComponent_AlphaTracker : MapComponent
    {
        public List<PenFamilyState> penStates = new List<PenFamilyState>();
        private const int AlphaUpdateIntervalTicks = 600;
        private static readonly IntRange AlphaFightCountdownTicks = new IntRange(30000, 60000);
        private const float PeriodicChallengeMtbDays = 60f;

        public MapComponent_AlphaTracker(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref penStates, "penStates", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                penStates ??= new List<PenFamilyState>();
                penStates.RemoveAll(x => x.penMarker == null || x.penMarker.Destroyed);
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            var currentTick = Find.TickManager.TicksGame;

            if (currentTick % AlphaUpdateIntervalTicks == 0)
            {
                UpdateAlphas();
            }

            foreach (var state in penStates)
            {
                if (state.fightTargetTick > 0 && currentTick >= state.fightTargetTick)
                {
                    state.fightTargetTick = -1;
                    TriggerFight(state);
                }
                else if (state.pendingTrigger != TriggerType.None && state.fightTargetTick <= 0 && currentTick >= state.cooldownUntilTick)
                {
                    var delay = AlphaFightCountdownTicks.RandomInRange;
                    state.TryStartCountdown(delay);
                }
            }
        }

        private void UpdateAlphas()
        {
            penStates.RemoveAll(x => x.penMarker == null || x.penMarker.Destroyed);
            var compsByPenFamily = new Dictionary<ThingWithComps, Dictionary<string, List<(Pawn pawn, CompAnimalGenes comp)>>>();
            foreach (var pawn in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                var comp = pawn.GetComp<CompAnimalGenes>();
                if (comp == null || comp.feratype == null || !comp.feratype.canBeAlpha)
                {
                    continue;
                }

                var penMarker = AnimalPenUtility.GetCurrentPenOf(pawn, false);
                if (penMarker == null)
                {
                    comp.soloTicks = 0;
                    if (comp.isAlpha)
                    {
                        comp.BecomeAlpha(false);
                    }
                    continue;
                }

                if (!IsEligibleMale(pawn))
                {
                    comp.soloTicks = 0;
                    if (comp.isAlpha)
                    {
                        comp.BecomeAlpha(false);
                    }
                    continue;
                }

                if (!compsByPenFamily.ContainsKey(penMarker.parent))
                {
                    compsByPenFamily[penMarker.parent] = new Dictionary<string, List<(Pawn, CompAnimalGenes)>>();
                }
                if (!compsByPenFamily[penMarker.parent].ContainsKey(comp.feratype.feratypeFamily))
                {
                    compsByPenFamily[penMarker.parent][comp.feratype.feratypeFamily] = new List<(Pawn, CompAnimalGenes)>();
                }

                compsByPenFamily[penMarker.parent][comp.feratype.feratypeFamily].Add((pawn, comp));
            }

            foreach (var penKvp in compsByPenFamily)
            {
                var penBuilding = penKvp.Key;
                foreach (var famKvp in penKvp.Value)
                {
                    ProcessPenFamily(penBuilding, famKvp.Key, famKvp.Value);
                }
            }
        }

        private void ProcessPenFamily(ThingWithComps penBuilding, string family, List<(Pawn pawn, CompAnimalGenes comp)> comps)
        {
            var state = GetOrCreateState(penBuilding, family);
            var currentAlphas = comps.Where(c => c.comp.isAlpha).Select(c => c.pawn).ToList();

            if (comps.Count == 1)
            {
                var soloComp = comps[0].comp;
                if (!soloComp.isAlpha)
                {
                    soloComp.soloTicks += AlphaUpdateIntervalTicks;
                    if (soloComp.soloTicks >= 1200)
                    {
                        soloComp.BecomeAlpha(true);
                    }
                }
                state.CancelPendingFight();
                state.knownMales.Clear();
                state.knownMales.AddRange(comps.Select(c => c.pawn));
                return;
            }

            comps.ForEach(c => c.comp.soloTicks = 0);

            if (currentAlphas.Count == 0 && !comps.Any(c => c.pawn.MentalStateDef == InternalDefOf.VRE_AlphaFighting))
            {
                if (state.pendingTrigger != TriggerType.PowerVacuum)
                {
                    state.pendingTrigger = TriggerType.PowerVacuum;
                    state.challengers = new List<Pawn>();
                    state.TryStartCountdown(AlphaFightCountdownTicks.RandomInRange);
                }
            }
            else if (currentAlphas.Count >= 1)
            {
                ProcessAlphas(state, comps, currentAlphas);
            }

            state.knownMales.Clear();
            state.knownMales.AddRange(comps.Select(c => c.pawn));
        }

        private void ProcessAlphas(PenFamilyState state, List<(Pawn pawn, CompAnimalGenes comp)> comps, List<Pawn> currentAlphas)
        {
            var alpha = currentAlphas[0];
            if (currentAlphas.Count > 1)
            {
                var otherAlphas = currentAlphas.Where(p => p != alpha).ToList();
                if (state.pendingTrigger == TriggerType.None)
                {
                    state.pendingTrigger = TriggerType.NewChallenger;
                    state.challengers = new List<Pawn>(otherAlphas);
                    if (Find.TickManager.TicksGame >= state.cooldownUntilTick)
                    {
                        state.TryStartCountdown(AlphaFightCountdownTicks.RandomInRange);
                    }
                }
                else if (state.pendingTrigger == TriggerType.NewChallenger)
                {
                    state.challengers.AddRange(otherAlphas.Where(p => !state.challengers.Contains(p)));
                }
            }
            var nonAlphas = comps.Where(c => c.pawn != alpha).Select(c => c.pawn).ToList();
            var newMales = nonAlphas.Where(m => !state.knownMales.Contains(m)).ToList();

            if (newMales.Count > 0)
            {
                if (state.pendingTrigger != TriggerType.NewChallenger)
                {
                    state.pendingTrigger = TriggerType.NewChallenger;
                    state.challengers = new List<Pawn>(newMales);
                    if (Find.TickManager.TicksGame >= state.cooldownUntilTick)
                    {
                        state.TryStartCountdown(AlphaFightCountdownTicks.RandomInRange);
                    }
                }
                else
                {
                    state.challengers.AddRange(newMales.Where(p => !state.challengers.Contains(p)));
                }
            }
            else
            {
                if (state.pendingTrigger == TriggerType.None && Find.TickManager.TicksGame >= state.cooldownUntilTick)
                {
                    if (Rand.MTBEventOccurs(PeriodicChallengeMtbDays, GenDate.TicksPerDay, AlphaUpdateIntervalTicks))
                    {
                        state.pendingTrigger = TriggerType.Periodic;
                        state.challengers = new List<Pawn>
                        {
                            nonAlphas.RandomElement()
                        };
                        state.TryStartCountdown(1);
                    }
                }
            }
        }

        private PenFamilyState GetOrCreateState(ThingWithComps penMarker, string family)
        {
            var state = penStates.FirstOrDefault(x => x.penMarker == penMarker && x.family == family);
            if (state == null)
            {
                state = new PenFamilyState
                {
                    penMarker = penMarker,
                    family = family
                };
                penStates.Add(state);
            }
            return state;
        }

        private void TriggerFight(PenFamilyState state)
        {
            var males = GetMales(state);
            if (males.Count < 2)
            {
                state.pendingTrigger = TriggerType.None;
                return;
            }

            Pawn pawn1 = null;
            Pawn pawn2 = null;

            if (state.pendingTrigger == TriggerType.PowerVacuum)
            {
                if (males.Any(c => c.comp.isAlpha))
                {
                    state.CancelPendingFight();
                    return;
                }
                pawn1 = males.RandomElement().pawn;
                pawn2 = males.Where(c => c.pawn != pawn1).RandomElement().pawn;
            }
            else if (state.pendingTrigger == TriggerType.NewChallenger || state.pendingTrigger == TriggerType.Periodic)
            {
                pawn1 = males.FirstOrDefault(c => c.comp.isAlpha).pawn;
                if (pawn1 == null)
                {
                    state.CancelPendingFight();
                    return;
                }
                var malePawns = males.Select(c => c.pawn).ToList();
                pawn2 = state.challengers.Where(p => malePawns.Contains(p)).RandomElementWithFallback();
                if (pawn2 == null)
                {
                    state.CancelPendingFight();
                    return;
                }
            }

            if (pawn1 != null && pawn2 != null && pawn1 != pawn2)
            {
                Messages.Message("VRE_AlphaFighting".Translate(pawn1.LabelShort, pawn2.LabelShort), pawn1, MessageTypeDefOf.NeutralEvent);
                EnterAlphaFight(pawn1, pawn2);
                EnterAlphaFight(pawn2, pawn1);
                state.cooldownUntilTick = Find.TickManager.TicksGame + (30 * GenDate.TicksPerDay);
            }

            state.pendingTrigger = TriggerType.None;
        }

        private void EnterAlphaFight(Pawn fighter, Pawn target)
        {
            fighter.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
            fighter.jobs.ClearQueuedJobs();
            fighter.pather.StopDead();
            fighter.mindState.mentalStateHandler.TryStartMentalState(InternalDefOf.VRE_AlphaFighting, null, forced: true, forceWake: true, otherPawn: target);
        }

        private List<(Pawn pawn, CompAnimalGenes comp)> GetMales(PenFamilyState state)
        {
            var males = new List<(Pawn, CompAnimalGenes)>();
            foreach (var p in map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                var comp = p.GetComp<CompAnimalGenes>();
                if (comp == null || comp.feratype == null || !comp.feratype.canBeAlpha || comp.feratype.feratypeFamily != state.family)
                {
                    continue;
                }
                var currentPen = AnimalPenUtility.GetCurrentPenOf(p, false);
                if (currentPen != null && currentPen.parent == state.penMarker && IsEligibleMale(p))
                {
                    males.Add((p, comp));
                }
            }
            return males;
        }

        private bool IsEligibleMale(Pawn p)
        {
            if (p.gender != Gender.Male)
            {
                return false;
            }
            if (p.Downed)
            {
                return false;
            }
            if (p.health.hediffSet.HasHediff(HediffDefOf.Sterilized))
            {
                return false;
            }
            if (!p.ageTracker.Adult)
            {
                return false;
            }
            return true;
        }
    }
}
