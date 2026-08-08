using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(ThingDefGenerator_Corpses), "GenerateCorpseDef")]
    public static class GenerateCorpseDef_Patch
    {
        public static void Postfix(ThingDef pawnDef, ThingDef __result)
        {
            if (pawnDef.race?.Animal != true)
                return;

            if (!__result.inspectorTabs.Contains(typeof(ITab_AnimalGenes)))
            {
                __result.inspectorTabs.Add(typeof(ITab_AnimalGenes));
            }
        }
    }

    [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.ResolveReferences))]
    public static class ThingDef_ResolveReferences_Patch
    {
        public static void Postfix(ThingDef __instance)
        {
            if (!__instance.defName.StartsWith("Corpse_"))
                return;

            Log.Message(
                $"Resolving {__instance.defName}: " +
                $"tabs={__instance.inspectorTabs?.Count ?? 0}, " +
                $"resolved={__instance.inspectorTabsResolved?.Count ?? 0}, " +
                $"gene tab={__instance.inspectorTabs?.Contains(typeof(ITab_AnimalGenes))}"
            );
        }
    }
}