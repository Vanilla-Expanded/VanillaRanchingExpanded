using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaRanchingExpanded
{
    [HotSwappable]
    [HarmonyPatch(typeof(Listing_TreeThingFilter), "DoThingDef")]
    public static class Listing_TreeThingFilter_DoThingDef_Patch
    {
        private static HashSet<ThingDef> expandedDefs = new();

        public static void Prefix(ThingDef tDef, ref int nestLevel, out int __state)
        {
            __state = nestLevel;
            if (ITab_PenAnimals_FillTab_Patch.currentPenMarker != null &&
                ITab_PenAnimals_FillTab_Patch.cachedPenAnimals.TryGetValue(tDef, out var pawns) &&
                pawns.Count > 0)
            {
                nestLevel += 1;
            }
        }

        public static void Postfix(Listing_TreeThingFilter __instance, ThingDef tDef, int __state)
        {
            if (ITab_PenAnimals_FillTab_Patch.currentPenMarker == null || ITab_PenAnimals_FillTab_Patch.cachedPenAnimals.TryGetValue(tDef, out var pawns) is false || pawns.Count == 0) return;

            var lineHeight = __instance.lineHeight;
            var rowY = __instance.CurHeight - lineHeight - __instance.verticalSpacing;

            var arrowRect = new Rect(__state * __instance.nestIndentWidth, rowY + lineHeight / 2f - 9f, 18f, 18f);
            var isExpanded = expandedDefs.Contains(tDef);
            var wasExpanded = isExpanded;
            if (Widgets.ButtonImage(arrowRect, isExpanded ? TexButton.Collapse : TexButton.Reveal))
            {
                isExpanded = isExpanded is false;
                (isExpanded ? SoundDefOf.TabOpen : SoundDefOf.TabClose).PlayOneShotOnCamera();
            }
            if (isExpanded != wasExpanded)
            {
                if (isExpanded) expandedDefs.Add(tDef);
                else expandedDefs.Remove(tDef);
            }

            if (isExpanded is false) return;

            var marker = ITab_PenAnimals_FillTab_Patch.currentPenMarker;
            var comp = marker.parent.GetComp<CompAnimalAssignments>();
            if (comp == null) return;

            var rowHeight = lineHeight + 5f;
            var pawnX = (__state + 3) * __instance.nestIndentWidth - 16f;
            var infoCardX = __instance.ColumnWidth - 52f;
            foreach (var p in pawns)
            {
                var rowRect = __instance.GetRect(rowHeight);

                var iconRect = new Rect(pawnX, rowRect.y + (rowHeight - 18f) / 2f, 18f, 18f);
                Widgets.ThingIcon(iconRect, p);

                Text.Anchor = TextAnchor.MiddleLeft;
                var labelRect = new Rect(iconRect.xMax + 4f, rowRect.y, infoCardX - iconRect.xMax - 8f, rowHeight);
                Widgets.Label(labelRect, p.Name?.ToStringShort ?? p.LabelShortCap);
                Text.Anchor = TextAnchor.UpperLeft;

                Widgets.InfoCardButton(infoCardX, rowRect.y + (rowHeight - 24f) / 2f, p);

                var checkRect = new Rect(__instance.ColumnWidth - 26f, rowRect.y + (rowHeight - lineHeight) / 2f, lineHeight, lineHeight);

                var isAllowed = marker.AcceptsToPen(p);
                var wasAllowed = isAllowed;

                Widgets.Checkbox(checkRect.x, checkRect.y, ref isAllowed, lineHeight, disabled: false, paintable: true);

                if (isAllowed != wasAllowed)
                {
                    if (isAllowed)
                    {
                        comp.forceExcluded.Remove(p);
                        if (marker.AnimalFilter.Allows(p.def) is false) comp.forceIncluded.Add(p);
                    }
                    else
                    {
                        comp.forceIncluded.Remove(p);
                        if (marker.AnimalFilter.Allows(p.def)) comp.forceExcluded.Add(p);
                    }
                }
            }
        }
    }
}
