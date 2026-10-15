using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName))]
    public static class PawnBioAndNameGenerator_GeneratePawnName_Patch
    {
        public static void Prefix(NameStyle style)
        {
            if (style != NameStyle.Numeric)
            {
                return;
            }
            var suffix = " (" + "VRE_Alpha".Translate() + ")";
            var usedNamesTmp = AccessTools.Field(typeof(PawnBioAndNameGenerator), "usedNamesTmp").GetValue(null) as HashSet<string>;
            foreach (var item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (item.Name is NameSingle nameSingle && nameSingle.Name.EndsWith(suffix))
                {
                    usedNamesTmp.Add(nameSingle.Name.Replace(suffix, ""));
                }
            }
        }
    }
}
