using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
namespace VanillaRanchingExpanded
{
    [HarmonyPatch(typeof(AnimalPenGUI), nameof(AnimalPenGUI.DoAllowedAreaMessage))]
    public static class VanillaRanchingExpanded_AnimalPenGUI_DoAllowedAreaMessage_Patch
    {
        public static bool Prefix(Rect rect, Pawn pawn)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            CompAnimalPenMarker currentPenOf = AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false);
            TaggedString taggedString;
            TaggedString taggedString2;
            if (currentPenOf != null)
            {

                Widgets.Dropdown(rect, pawn, (Pawn pawn) => null, MenuGenerator, "VRE_ReassignAnimalPen".Translate());
            }
            else
            {
                GUI.color = Color.gray;
                taggedString = "(" + "Unpenned".Translate() + ")";
                taggedString2 = "UnpennedTooltip".Translate();
            }
            Widgets.Label(rect, taggedString);
            TooltipHandler.TipRegion(rect, taggedString2);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            return false;
        }

        public static IEnumerable<Widgets.DropdownMenuElement<Building>> MenuGenerator(Pawn pawn)
        {
            foreach (Building pen in pawn.Map.listerBuildings.allBuildingsAnimalPenMarkers)
            {
                CompAnimalPenMarker comp = pen.TryGetComp<CompAnimalPenMarker>();
                CompAnimalAssignments compAssignments = pen.TryGetComp<CompAnimalAssignments>();

                Widgets.DropdownMenuElement<Building> dropdownMenuElement = default(Widgets.DropdownMenuElement<Building>);
                dropdownMenuElement.option = new FloatMenuOption(comp.label, delegate
                {
                    if (!compAssignments.penAssignedAnimals.Contains(pawn))
                    {
                        compAssignments.penAssignedAnimals.Add(pawn);
                        foreach (Building pen2 in pawn.Map.listerBuildings.allBuildingsAnimalPenMarkers)
                        {
                            if (pen != pen2)
                            {
                                CompAnimalPenMarker comp2 = pen2.TryGetComp<CompAnimalPenMarker>();
                                CompAnimalAssignments compAssignments2 = pen2.TryGetComp<CompAnimalAssignments>();
                                if (compAssignments2.penAssignedAnimals.Contains(pawn)){ 
                                    compAssignments2.penAssignedAnimals.Remove(pawn); 
                                }
                                
                            }
                        }
                        
                    }
                });

                yield return dropdownMenuElement;
            }
        }
    }
}

