
using RimWorld;
using UnityEngine;
using VEF.Buildings;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatPart_AnimalGenes : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
                {
                    CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                    if (comp != null)
                    {
                        val *= GenesFactorsDefined(comp);
                    }
                }
            }
        }
        private float GenesFactorsDefined(CompAnimalGenes comp)
        {
            float num = 1f;
            for (int i = 0; i < comp.genes.Count; i++)
            {
                num *= comp.genes[i].marketValueFactor;
            }
            return num;
        }

        public override string ExplanationPart(StatRequest req)
        {
            string text = null;

            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(pawn))
                {
                    CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[pawn];
                    if (comp != null)
                    {
                        if (GenesFactorsDefined(comp) != 1f)
                        {
                            if (!text.NullOrEmpty())
                            {
                                text += "\n";
                            }
                            text += "GenePriceFactors".Translate() + ":";
                            for (int i = 0; i < comp.genes.Count; i++)
                            {
                                AnimalGeneDef geneDef = comp.genes[i];
                                if (geneDef.marketValueFactor != 1f)
                                {
                                    text += $"\n  - {geneDef.LabelCap} x{geneDef.marketValueFactor.ToStringPercent()}";
                                }
                            }
                        }
                        
                    }
                    return text?.TrimEndNewlines();
                }
            }
            return text;
        }
    }
}