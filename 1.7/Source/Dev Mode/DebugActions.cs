using System.Collections.Generic;
using RimWorld;
using Verse;
using LudeonTK;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    public static class DebugActions
    {
        private static Map Map
        {
            get
            {
                return Find.CurrentMap;
            }
        }

        [DebugAction("VE Ranching", "Add gene to animal", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static DebugActionNode AddGeneToAnimal()
        {
      
            DebugActionNode debugActionNode = new DebugActionNode();
            List<AnimalGeneDef> allGenes = DefDatabase<AnimalGeneDef>.AllDefsListForReading;

            for (int i = 0; i < allGenes.Count; i++)
            {
                AnimalGeneDef gene = allGenes[i];
                
                    debugActionNode.AddChild(new DebugActionNode(gene.LabelCap.ToString(), DebugActionType.ToolMap, delegate
                    {
                        foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap))
                        {
                            CompAnimalGenes comp = thing.TryGetComp<CompAnimalGenes>();
                            if (comp != null)
                            {
                                AnimalGeneUtility.AddGeneRespectingFamily(comp, gene);
                                DebugActionsUtility.DustPuffFrom(thing);
                            }
                            else
                            {
                                Messages.Message("VRE_CantApply".Translate(gene.LabelCap, thing.LabelCap), thing,
                                    MessageTypeDefOf.RejectInput, false);
                                return;
                            }

                        }
                    }));
                
            }
            return debugActionNode;


        }

        [DebugAction("VE Ranching", "Remove gene from animal", false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveGeneFromAnimal()
        {
            Thing item= Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).Where(x => x.TryGetComp<CompAnimalGenes>()!=null)?.First();
            if (item != null)
            {
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_RemoveGene(item)));
            }           
        }

        public static List<DebugMenuOption> Options_RemoveGene(Thing thing)
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            CompAnimalGenes comp = thing.TryGetComp<CompAnimalGenes>();
            if (comp != null)
            {
                foreach (AnimalGeneDef item in comp.genes)
                {
                    AnimalGeneDef gene = item;
                    list.Add(new DebugMenuOption(gene.LabelCap, DebugMenuOptionMode.Action, delegate
                    {
                        AnimalGeneUtility.RemoveGene(comp, gene);
                        DebugActionsUtility.DustPuffFrom(thing);
                    }));
                }
                return list;
            }
           

            return list;
        }



    }
}

