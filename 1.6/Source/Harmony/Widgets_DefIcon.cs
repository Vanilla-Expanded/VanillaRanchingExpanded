
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace VanillaRanchingExpanded
{

    [HarmonyPatch(typeof(Widgets))]
    [HarmonyPatch("DefIcon")]

    public class VanillaRanchingExpanded_Widgets_DefIcon_Patch
    {
        [HarmonyPostfix]
        public static void DoGeneDefIcon(Rect rect, Def def, Color? color, float scale, Material material, float alpha)
        {
            AnimalGeneDef geneDef = def as AnimalGeneDef;
            if (geneDef != null)
            {
                GUI.color = color ?? geneDef.IconColor;
                CachedTexture cachedTexture = ITab_AnimalGenes.GetBackGround(geneDef.stability);
                GUI.DrawTexture(rect, cachedTexture.Texture);
                Widgets.DrawTextureFitted(rect, geneDef.Icon, scale, material, alpha);
                GUI.color = Color.white;
                return;
            }
        }
    }
}