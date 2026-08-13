using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaRanchingExpanded;
using Verse;


namespace VanillaRanchingExpanded
{
    public class AnimalGeneInjector : ThingWithComps
    {
        public AnimalGeneDef animalGene;

        public override bool CanStackWith(Thing other)
        {
            return false;
        }

        public override void PostMake()
        {
            base.PostMake();
            animalGene = GenerateRandomGene();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref animalGene, "animalGene");
        }

        public AnimalGeneDef GenerateRandomGene()
        {
            if (animalGene is null)
            {
                List<AnimalGeneDef> allGenes = DefDatabase<AnimalGeneDef>.AllDefsListForReading.Where(x => !x.dontGenerateInGeneTweakTools).ToList();
                return allGenes.RandomElement();
            }
            return animalGene;

        }

        public override string LabelNoCount
        {
            get
            {
                if (animalGene != null)
                {
                    return this.def.label + ": " + animalGene.label;
                }
                return base.LabelNoCount;
            }
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();

            if (animalGene != null)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }

                text += "VRE_ToolGene".Translate(animalGene.LabelCap);
            }
            return text;
        }

        public override string DescriptionFlavor => DescriptionDetailed;

        public override string DescriptionDetailed
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(def.description);
                stringBuilder.AppendLine();

                if (animalGene != null)
                {

                    stringBuilder.AppendLine(animalGene.LabelCap.Colorize(ColorLibrary.Yellow));
                    stringBuilder.AppendLine(animalGene.description);
                    if (!animalGene.statOffsets.NullOrEmpty())
                    {
                        stringBuilder.Append(animalGene.statOffsets.Select((StatModifier x) => $"{x.stat.LabelCap} {x.stat.Worker.ValueToString(x.value, finalized: false, ToStringNumberSense.Offset)}").ToLineList(" - "));
                        stringBuilder.AppendLine();
                    }
                    if (!animalGene.statFactors.NullOrEmpty())
                    {
                        stringBuilder.Append(animalGene.statFactors.Select((StatModifier x) => $"{x.stat.LabelCap} {x.stat.Worker.ValueToString(x.value, finalized: false, ToStringNumberSense.Factor)}").ToLineList(" - "));
                        stringBuilder.AppendLine();
                    }                                   
                }
                return stringBuilder.ToString();
            }
        }      

        public override IEnumerable<DefHyperlink> DescriptionHyperlinks
        {
            get
            {
                if (animalGene != null)
                {
                    yield return new DefHyperlink(animalGene);

                }
            }
        }
    }
}
