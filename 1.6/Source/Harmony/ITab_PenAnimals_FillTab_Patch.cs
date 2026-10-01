using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HarmonyPatch(typeof(ITab_PenAnimals), "FillTab")]
    public static class ITab_PenAnimals_FillTab_Patch
    {
        public static CompAnimalPenMarker currentPenMarker;
        public static Dictionary<ThingDef, List<Pawn>> cachedPenAnimals = new();

        public static void Prefix(ITab_PenAnimals __instance)
        {
            currentPenMarker = __instance.SelectedCompAnimalPenMarker;
            cachedPenAnimals.Clear();

            if (currentPenMarker != null && currentPenMarker.parent.Map != null)
            {
                var pawns = currentPenMarker.parent.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
                foreach (var p in pawns)
                {
                    if (AnimalPenUtility.NeedsToBeManagedByRope(p))
                    {
                        if (cachedPenAnimals.TryGetValue(p.def, out var list) is false)
                        {
                            list = new List<Pawn>();
                            cachedPenAnimals[p.def] = list;
                        }
                        list.Add(p);
                    }
                }
            }
        }

        public static Exception Finalizer(Exception __exception)
        {
            currentPenMarker = null;
            cachedPenAnimals.Clear();
            return __exception;
        }
    }
}
