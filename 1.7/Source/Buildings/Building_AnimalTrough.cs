using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class Building_AnimalTrough : Building_Storage
    {
        public float nutritionTarget = 6f;
        [NonSerialized] public bool draggingNutritionBar;
        private readonly AnimalTroughRefillBatch batch = new AnimalTroughRefillBatch();

        public override void ExposeData()
        {
            base.ExposeData();
            ExposeRefillData();
        }

        private void ExposeRefillData()
        {
            Scribe_Values.Look(ref nutritionTarget, "nutritionTarget", 6f);
            Scribe_Values.Look(ref batch.remainingNutrition, "refillNutritionRemaining", 0f);
            Scribe_Values.Look(ref batch.started, "refillBatchStarted", false);
            Scribe_Values.Look(ref batch.nextCheckTick, "nextRefillCheckTick", -1);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (batch.nextCheckTick < 0)
                batch.nextCheckTick = Find.TickManager.TicksGame +
                    (int)((uint)thingIDNumber * 391u % AnimalTroughRefillBatch.CheckInterval);
        }

        public void SetNutritionTarget(float value)
        {
            value = Math.Clamp(value, 0f, 100f);
            if (storageGroup == null)
                ApplyNutritionTarget(value);
            else
                foreach (IStorageGroupMember member in storageGroup.members)
                    if (member is Building_AnimalTrough trough)
                        trough.ApplyNutritionTarget(value);
        }

        private void ApplyNutritionTarget(float value)
        {
            nutritionTarget = value;
            if (Spawned)
                batch.ClampToSpace(value - GetStoredNutrition(), Find.TickManager.TicksGame);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;
            yield return new Gizmo_AnimalTroughNutrition(this);
        }

        public override string GetInspectString()
        {
            var text = new StringBuilder(base.GetInspectString());
            if (Spawned)
            {
                if (text.Length > 0) text.AppendLine();
                text.Append("VRE_TroughNutrition".Translate(GetStoredNutrition().ToString("0.##"),
                    nutritionTarget.ToString("0.##")));
                if (nutritionTarget <= 0f)
                    text.AppendLine().Append("VRE_TroughDisabled".Translate());
                else if (batch.Active)
                    text.AppendLine().Append("VRE_TroughRefilling".Translate(batch.remainingNutrition.ToString("0.##")));
            }
            return text.ToString();
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!Spawned) return;
            int now = Find.TickManager.TicksGame;
            batch.ClampToSpace(nutritionTarget - GetStoredNutrition(), now);
            if (batch.Active)
            {
                if (!HasAnimalsToFeed(GetEnclosedPen())) batch.Complete(now);
                return;
            }
            if (now < batch.nextCheckTick) return;
            batch.nextCheckTick = now + AnimalTroughRefillBatch.CheckInterval;
            if (nutritionTarget <= 0f) return;

            CompAnimalPenMarker marker = GetEnclosedPen();
            if (HasAnimalsToFeed(marker) && PenNeedsFood(marker) &&
                HasRefillSource(nutritionTarget - GetStoredNutrition()))
                batch.Approve(nutritionTarget - GetStoredNutrition());
        }

        // A batch is committed only after the first hauler has reserved its source and destination.
        // JobOnThing is also called while scanning, so it must not commit a batch itself.
        internal bool TryStartRefill()
        {
            if (!CanContinueRefill()) return false;
            if (!batch.started)
            {
                if (!PenNeedsFood(GetEnclosedPen()))
                {
                    batch.Complete(Find.TickManager.TicksGame);
                    return false;
                }
                batch.started = true;
            }
            return true;
        }

        internal bool CanContinueRefill()
        {
            if (!Spawned || nutritionTarget <= 0f || !batch.Active) return false;
            return HasAnimalsToFeed(GetEnclosedPen());
        }

        private bool PenNeedsFood(CompAnimalPenMarker marker)
        {
            return marker != null && CalculateAvailableNutrition(marker) + CalculatePendingNutrition(marker)
                < marker.PenFoodCalculator.SumNutritionConsumptionPerDay;
        }

        private static bool HasAnimalsToFeed(CompAnimalPenMarker marker)
        {
            return marker != null && marker.PenFoodCalculator.SumNutritionConsumptionPerDay > 0f;
        }

        private bool HasRefillJob()
        {
            foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
                if (pawn.CurJob?.def == InternalDefOf.VRE_RefillAnimalTrough &&
                    AllSlotCellsList().Contains(pawn.CurJob.GetTarget(TargetIndex.B).Cell))
                    return true;
            return false;
        }

        internal Job TryMakeRefillJob(Pawn pawn, bool forced)
        {
            if (!batch.Active || !Spawned || pawn.Map != Map || nutritionTarget <= 0f ||
                Faction != pawn.Faction || this.IsForbidden(pawn) || !pawn.CanReserve(this) || HasRefillJob()) return null;
            if (!HasAnimalsToFeed(GetEnclosedPen())) return null;

            float remaining = Math.Min(batch.remainingNutrition, nutritionTarget - GetStoredNutrition());
            Thing bestFood = null;
            IntVec3 bestCell = IntVec3.Invalid;
            int bestCount = 0;
            float bestNutrition = 0f;
            float bestDistance = float.MaxValue;
            bool anyUsableSource = false;

            foreach (Thing food in Map.listerThings.AllThings)
            {
                if (!IsRefillFood(food)) continue;
                float perItem = food.GetStatValue(StatDefOf.Nutrition);
                int fittingCount = AnimalTroughRefillBatch.CountThatFits(remaining, perItem);
                if (fittingCount <= 0 || !HasPhysicalSpace(food)) continue;
                anyUsableSource = true;
                if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, food, forced)) continue;
                if (pawn.carryTracker.CarriedThing != null &&
                    !pawn.carryTracker.CarriedThing.CanStackWith(food)) continue;
                int count = Math.Min(fittingCount, Math.Min(food.stackCount,
                    pawn.carryTracker.AvailableStackSpace(food.def)));
                if (count <= 0 || !TryFindRefillCell(pawn, food, out IntVec3 cell, out int space)) continue;
                count = Math.Min(count, space);
                float deliveredNutrition = count * perItem;
                float distance = pawn.Position.DistanceToSquared(food.Position) + food.Position.DistanceToSquared(cell);
                if (AnimalTroughRefillBatch.IsBetterDelivery(deliveredNutrition, distance, bestNutrition, bestDistance))
                {
                    bestFood = food;
                    bestCell = cell;
                    bestCount = count;
                    bestNutrition = deliveredNutrition;
                    bestDistance = distance;
                }
            }

            if (bestFood == null)
            {
                // An inaccessible source may still be usable by another colonist. Only abandon
                // the batch if there is no permitted, fitting source with physical storage space.
                if (!anyUsableSource) batch.Complete(Find.TickManager.TicksGame);
                return null;
            }
            Job job = JobMaker.MakeJob(InternalDefOf.VRE_RefillAnimalTrough, bestFood, bestCell);
            job.count = bestCount;
            job.haulMode = HaulMode.ToCellStorage;
            job.haulOpportunisticDuplicates = false;
            return job;
        }

        private bool IsRefillFood(Thing food)
        {
            return food.Spawned && food.def.category == ThingCategory.Item && food.IngestibleNow &&
                GetParentStoreSettings().AllowedToAccept(food) && Accepts(food) &&
                !(Map.haulDestinationManager.SlotGroupParentAt(food.Position) is Building_AnimalTrough);
        }

        private bool HasRefillSource(float remaining)
        {
            // Do not promise food to the pen if this trough's filter has no usable source.
            foreach (Thing food in Map.listerThings.AllThings)
                if (IsRefillFood(food) && HasPhysicalSpace(food) &&
                    AnimalTroughRefillBatch.CountThatFits(remaining, food.GetStatValue(StatDefOf.Nutrition)) > 0)
                    return true;
            return false;
        }

        private bool HasPhysicalSpace(Thing food)
        {
            foreach (IntVec3 cell in AllSlotCellsList())
                if (CellSpaceFor(cell, food) > 0) return true;
            return false;
        }

        internal bool TryFindRefillCell(Pawn pawn, Thing food, out IntVec3 cell, out int space)
        {
            cell = IntVec3.Invalid;
            space = 0;
            foreach (IntVec3 candidate in AllSlotCellsList())
            {
                int candidateSpace = CellSpaceFor(candidate, food);
                if (candidateSpace <= space || candidate.IsForbidden(pawn) ||
                    !pawn.CanReach(candidate, PathEndMode.Touch, pawn.NormalMaxDanger()) ||
                    !pawn.CanReserve(candidate)) continue;
                cell = candidate;
                space = candidateSpace;
            }
            return cell.IsValid;
        }

        internal int CellSpaceFor(IntVec3 cell, Thing food)
        {
            if (!Spawned || Map.haulDestinationManager.SlotGroupParentAt(cell) != this ||
                !cell.IsValidStorageFor(Map, food) || cell.ContainsStaticFire(Map)) return 0;
            int stacks = 0;
            int space = 0;
            foreach (Thing item in cell.GetThingList(Map))
            {
                if (item.def.category != ThingCategory.Item) continue;
                stacks++;
                if (item.CanStackWith(food)) space += Math.Max(0, item.def.stackLimit - item.stackCount);
            }
            // A direct drop can create one new stack, in addition to filling compatible stacks.
            if (stacks < def.building.maxItemsInCell) space += food.def.stackLimit;
            return space;
        }

        internal int DeliveryCount(Thing food, IntVec3 cell)
        {
            if (!CanContinueRefill() || !Accepts(food) || !GetParentStoreSettings().AllowedToAccept(food)) return 0;
            return Math.Min(food.stackCount, Math.Min(CellSpaceFor(cell, food),
                AnimalTroughRefillBatch.CountThatFits(Math.Min(batch.remainingNutrition,
                    nutritionTarget - GetStoredNutrition()), food.GetStatValue(StatDefOf.Nutrition))));
        }

        internal void Notify_Delivered(float nutrition)
        {
            batch.Delivered(nutrition, Find.TickManager.TicksGame);
        }

        private float CalculatePendingNutrition(CompAnimalPenMarker marker)
        {
            float nutrition = 0f;
            foreach (Thing thing in Map.listerThings.ThingsOfDef(def))
            {
                if (!(thing is Building_AnimalTrough other) || other == this || !other.batch.Active ||
                    other.GetEnclosedPen()?.PenState != marker.PenState) continue;
                float pending = Math.Min(other.batch.remainingNutrition,
                    Math.Max(0f, other.nutritionTarget - other.GetStoredNutrition()));
                // A reserved source still on the ground inside the pen was already counted.
                foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
                {
                    Job job = pawn.CurJob;
                    if (job?.def != InternalDefOf.VRE_RefillAnimalTrough ||
                        !other.AllSlotCellsList().Contains(job.GetTarget(TargetIndex.B).Cell)) continue;
                    Thing source = job.GetTarget(TargetIndex.A).Thing;
                    if (source?.Spawned == true && IsAvailablePenFood(source, marker, GetPenAnimals(marker)))
                        pending -= Math.Min(job.count, source.stackCount) * source.GetStatValue(StatDefOf.Nutrition);
                }
                nutrition += Math.Max(0f, pending);
            }
            return nutrition;
        }

        private List<Pawn> GetPenAnimals(CompAnimalPenMarker marker)
        {
            var animals = new List<Pawn>();
            foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
            {
                Region region = Map.regionGrid.GetValidRegionAt(pawn.Position);
                if (pawn.IsAnimal && region != null && marker.PenState.ContainsConnectedRegion(region))
                    animals.Add(pawn);
            }
            return animals;
        }

        private bool IsAvailablePenFood(Thing food, CompAnimalPenMarker marker, List<Pawn> animals)
        {
            if (!food.Spawned || (food.def.category != ThingCategory.Item && !(food is Plant)) ||
                !food.IngestibleNow) return false;
            Region region = Map.regionGrid.GetValidRegionAt(food.Position);
            if (region == null || !marker.PenState.ContainsConnectedRegion(region)) return false;
            foreach (Pawn animal in animals)
                if (animal.RaceProps.CanEverEat(food.def) && !food.IsForbidden(animal) &&
                    animal.CanReach(food, PathEndMode.Touch, animal.NormalMaxDanger())) return true;
            return false;
        }

        private float CalculateAvailableNutrition(CompAnimalPenMarker marker)
        {
            List<Pawn> animals = GetPenAnimals(marker);
            float nutrition = 0f;
            foreach (Thing food in Map.listerThings.AllThings)
            {
                if (!IsAvailablePenFood(food, marker, animals)) continue;
                float perItem = food.GetStatValue(StatDefOf.Nutrition);
                nutrition += food is Plant plant ? perItem * plant.Growth : perItem * food.stackCount;
            }
            return nutrition;
        }

        public float GetStoredNutrition()
        {
            float nutrition = 0f;
            foreach (Thing food in slotGroup.HeldThings)
                nutrition += food.GetStatValue(StatDefOf.Nutrition) * food.stackCount;
            return nutrition;
        }

        private CompAnimalPenMarker GetEnclosedPen()
        {
            if (!Spawned) return null;
            Region region = Map.regionGrid.GetValidRegionAt(Position);
            if (region == null) return null;
            foreach (Building building in Map.listerBuildings.allBuildingsAnimalPenMarkers)
            {
                CompAnimalPenMarker marker = building.GetComp<CompAnimalPenMarker>();
                if (marker?.PenState != null && marker.PenState.Enclosed &&
                    marker.PenState.ContainsConnectedRegion(region)) return marker;
            }
            return null;
        }
    }
}
