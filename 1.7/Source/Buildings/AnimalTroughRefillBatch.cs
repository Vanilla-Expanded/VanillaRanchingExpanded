using System;

namespace VanillaRanchingExpanded
{
    // Saved by the building. The budget only decreases, even if animals eat during a delivery.
    internal sealed class AnimalTroughRefillBatch
    {
        internal const int CheckInterval = 2500;
        private const float RoundingTolerance = 0.000001f;
        internal float remainingNutrition;
        internal bool started;
        internal int nextCheckTick = -1;
        internal bool Active => remainingNutrition > RoundingTolerance;

        internal void Approve(float missingNutrition)
        {
            remainingNutrition = Math.Max(0f, missingNutrition);
            started = false;
        }

        internal void Complete(int now)
        {
            remainingNutrition = 0f;
            started = false;
            nextCheckTick = now + CheckInterval;
        }

        internal void ClampToSpace(float space, int now)
        {
            if (!Active) return;
            remainingNutrition = Math.Min(remainingNutrition, Math.Max(0f, space));
            if (!Active) Complete(now);
        }

        internal void Delivered(float nutrition, int now)
        {
            remainingNutrition = Math.Max(0f, remainingNutrition - nutrition);
            if (!Active) Complete(now);
        }

        internal static int CountThatFits(float remaining, float perItem)
        {
            if (remaining <= 0f || perItem <= 0f || float.IsNaN(remaining) || float.IsNaN(perItem)) return 0;
            // Allow only float representation error, never a whole extra item.
            return (int)Math.Min(int.MaxValue, Math.Floor(((double)remaining + RoundingTolerance) / perItem));
        }

        internal static bool IsBetterDelivery(float nutrition, float distance, float bestNutrition, float bestDistance)
        {
            return nutrition > bestNutrition || (nutrition == bestNutrition && distance < bestDistance);
        }
    }
}
