using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VEF;
using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class Building_AnimalTrough : Building_Storage
    {
        public float nutritionTarget = 6f;
        private const int RefillCheckInterval = 2500; //1 hour
        [System.NonSerialized]
        private bool refillNeeded;
        [System.NonSerialized]
        public bool draggingNutritionBar;
        private int nextRefillCheckTick;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref nutritionTarget, "nutritionTarget", 6f);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Gizmo_AnimalTroughNutrition(this);
        }

        public override void TickRare()
        {
            base.TickRare();

            if (!Spawned || nutritionTarget <= 0f)
            {
                refillNeeded = false;
                return;
            }

            int currentTick = Find.TickManager.TicksGame;

            if (currentTick < nextRefillCheckTick)
            {
                return;
            }

            nextRefillCheckTick = currentTick + RefillCheckInterval;
            CheckRefill();
        }


        private bool TryFindRefillCell(Pawn pawn, Thing food, out IntVec3 cell)
        {
            cell = IntVec3.Invalid;

            if (slotGroup == null || Map == null)
            {
                return false;
            }

            foreach (IntVec3 candidate in AllSlotCellsList())
            {
                if (Map.haulDestinationManager.SlotGroupParentAt(candidate) != this)
                {
                    continue;
                }

                if (!GetStoreSettings().AllowedToAccept(food))
                {
                    return false;
                }

                if (!pawn.CanReach(candidate, PathEndMode.Touch, Danger.Some))
                {
                    continue;
                }

                if (!pawn.CanReserve(candidate))
                {
                    continue;
                }

                cell = candidate;
                return true;
            }

            return false;
        }




        private void CheckRefill()
        {
            refillNeeded = false;

            CompAnimalPenMarker marker = GetEnclosedPen();
            if (marker == null)
            {
                return;
            }

            float dailyConsumption =
                marker.PenFoodCalculator.SumNutritionConsumptionPerDay;

            if (dailyConsumption <= 0f)
            {
                return;
            }

            float availableNutrition =
                CalculateAvailableNutrition(marker)
                + CalculatePendingNutrition(marker);

            if (availableNutrition >= dailyConsumption)
            {
                return;
            }

            if (GetStoredNutrition() >= nutritionTarget)
            {
                return;
            }

            refillNeeded = true;
        }


       

        internal Job TryMakeRefillJob(Pawn pawn, bool forced)
        {
            if (!refillNeeded)
            {
                return null;
            }
            if (!Spawned || nutritionTarget <= 0f || pawn.Map != Map)
            {
                return null;
            }

            CompAnimalPenMarker marker = GetEnclosedPen();
            if (marker == null)
            {
                return null;
            }

            float dailyConsumption =
                marker.PenFoodCalculator.SumNutritionConsumptionPerDay;

            if (dailyConsumption <= 0f || CalculateAvailableNutrition(marker) + CalculatePendingNutrition(marker) >= dailyConsumption)
            {
                return null;
            }

            float remainingTarget = nutritionTarget - GetStoredNutrition();
            if (remainingTarget <= 0f)
            {
                return null;
            }

            foreach (Pawn otherPawn in Map.mapPawns.AllPawnsSpawned)
            {
                Job currentJob = otherPawn.CurJob;

                if (currentJob == null ||
                    currentJob.def != InternalDefOf.VRE_RefillAnimalTrough)
                {
                    continue;
                }

                if (AllSlotCellsList().Contains(
                    currentJob.GetTarget(TargetIndex.B).Cell))
                {
                    return null;
                }
            }

            Thing bestFood = null;
            int bestCount = 0;
            float bestNutrition = float.MaxValue;

            foreach (Thing food in Map.listerThings.AllThings)
            {
                if (!food.Spawned ||
                    food.def.category != ThingCategory.Item ||
                    !food.IngestibleNow ||
                    food.IsForbidden(pawn) ||
                    !Accepts(food))
                {
                    continue;
                }

            
                if (Map.haulDestinationManager.SlotGroupParentAt(food.Position)
                    is Building_AnimalTrough)
                {
                    continue;
                }

                float nutritionPerItem =
                    food.GetStatValue(StatDefOf.Nutrition);

                if (nutritionPerItem <= 0f ||
                    nutritionPerItem > remainingTarget)
                {
                    continue;
                }

                if (!pawn.RaceProps.CanEverEat(food.def) &&
                    !HaulAIUtility.PawnCanAutomaticallyHaul(pawn, food, forced))
                {
                    continue;
                }

                if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, food, forced))
                {
                    continue;
                }

                int count = Mathf.FloorToInt(
                    remainingTarget / nutritionPerItem + 0.00001f);

                count = Mathf.Min(count, food.stackCount);
                count = Mathf.Min(
                    count,
                    MassUtility.CountToPickUpUntilOverEncumbered(pawn, food));

                if (count <= 0)
                {
                    continue;
                }

                float deliveredNutrition = count * nutritionPerItem;

          
                if (deliveredNutrition < bestNutrition)
                {
                    bestFood = food;
                    bestCount = count;
                    bestNutrition = deliveredNutrition;
                }
            }

            if (bestFood == null)
            {
                return null;
            }


            if (!TryFindRefillCell(pawn, bestFood, out IntVec3 destination))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(
                InternalDefOf.VRE_RefillAnimalTrough,
                bestFood,
                destination);

            job.count = bestCount;
            job.haulMode = HaulMode.ToCellStorage;
            job.haulOpportunisticDuplicates = false;

            return job;

        }

        private float CalculatePendingNutrition(CompAnimalPenMarker marker)
        {
            if (Map == null || marker?.PenState == null)
            {
                return 0f;
            }

            float nutrition = 0f;
            HashSet<Region> penRegions = marker.PenState.ConnectedRegions;

            foreach (Pawn hauler in Map.mapPawns.AllPawnsSpawned)
            {
                Job currentJob = hauler.CurJob;

                if (currentJob == null ||
                    currentJob.def != InternalDefOf.VRE_RefillAnimalTrough)
                {
                    continue;
                }

                LocalTargetInfo destination =
                    currentJob.GetTarget(TargetIndex.B);

                if (!destination.IsValid ||
                    !(Map.haulDestinationManager.SlotGroupParentAt(destination.Cell)
                        is Building_AnimalTrough destinationTrough) ||
                    destinationTrough.GetEnclosedPen() != marker)
                {
                    continue;
                }

                Thing carriedThing = hauler.carryTracker.CarriedThing;

                if (carriedThing != null)
                {
                    nutrition += carriedThing.GetStatValue(StatDefOf.Nutrition)
                        * carriedThing.stackCount;

                    continue;
                }

                Thing source = currentJob.GetTarget(TargetIndex.A).Thing;

                if (source == null || !source.Spawned)
                {
                    continue;
                }

                Region sourceRegion =
                    Map.regionGrid.GetValidRegionAt(source.Position);

                
                if (sourceRegion != null && penRegions.Contains(sourceRegion))
                {
                    continue;
                }

                int count = currentJob.count > 0
                    ? Mathf.Min(currentJob.count, source.stackCount)
                    : source.stackCount;

                nutrition += source.GetStatValue(StatDefOf.Nutrition) * count;
            }

            return nutrition;
        }


        private float CalculateAvailableNutrition(CompAnimalPenMarker marker)
        {
            Map map = Map;
            PenMarkerState state = marker.PenState;

            if (map == null || state == null)
            {
                return 0f;
            }

            HashSet<Region> penRegions = state.ConnectedRegions;
            List<Pawn> penAnimals = new List<Pawn>();

            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (!(thing is Pawn pawn) || !pawn.Spawned || !pawn.IsAnimal)
                {
                    continue;
                }

                Region region = map.regionGrid.GetValidRegionAt(pawn.Position);
                if (region != null && penRegions.Contains(region))
                {
                    penAnimals.Add(pawn);
                }
            }

            if (penAnimals.Count == 0)
            {
                return 0f;
            }

            float nutrition = 0f;

           
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (!thing.Spawned ||thing.def.category != ThingCategory.Item ||!thing.IngestibleNow)
                {
                    continue;
                }

                Region region = map.regionGrid.GetValidRegionAt(thing.Position);
                if (region == null || !penRegions.Contains(region))
                {
                    continue;
                }

                bool accessibleFood = false;

                foreach (Pawn animal in penAnimals)
                {
                    if (!animal.RaceProps.CanEverEat(thing.def))
                    {
                        continue;
                    }

                    if (!map.reachability.CanReach(animal.Position,thing.Position,PathEndMode.Touch,TraverseParms.For(animal)))
                    {
                        continue;
                    }

                    accessibleFood = true;
                    break;
                }

                if (accessibleFood)
                {
                    nutrition += thing.GetStatValue(StatDefOf.Nutrition) * thing.stackCount;
                }
            }

            return nutrition;
        }

        private float GetStoredNutrition()
        {
            float nutrition = 0f;

            if (slotGroup == null)
            {
                return nutrition;
            }

            foreach (Thing thing in slotGroup.HeldThings)
            {
                nutrition += thing.GetStatValue(StatDefOf.Nutrition) * thing.stackCount;
            }

            return nutrition;
        }


        private CompAnimalPenMarker GetEnclosedPen()
        {
            if (Map == null)
            {
                return null;
            }

            Region region = Map.regionGrid.GetValidRegionAt(Position);
            if (region == null)
            {
                return null;
            }

            foreach (Building building in Map.listerBuildings.allBuildingsAnimalPenMarkers)
            {
                CompAnimalPenMarker marker = building.GetComp<CompAnimalPenMarker>();

                if (marker == null || marker.PenState == null)
                {
                    continue;
                }

                PenMarkerState state = marker.PenState;

                if (state.Enclosed && state.ContainsConnectedRegion(region))
                {
                    return marker;
                }
            }

            return null;
        }

    }
}
