using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(ThingDefGenerator_Corpses), "GenerateCorpseDef")]
    public static class VanillaRanchingExpanded_ThingDefGenerator_Corpses_GenerateCorpseDef_Patch
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

   
}