using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
   

    public class AnimalGeneDef: Def
    {
        [NoTranslate]
        public string iconPath;

        private Color? iconColor;

        [Unsaved(false)]
        private Texture2D cachedIcon;

        [Unsaved(false)]
        private string cachedDescription;

        public int stability;

        public List<StatModifier> statOffsets = new List<StatModifier>();

        public List<StatModifier> statFactors = new List<StatModifier>();

        public float marketValueFactor = 1f;

        public string familyTag;

        public string DescriptionFull => cachedDescription ?? (cachedDescription = GetDescriptionFull());

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }
            if (stability<-2 || stability>2)
            {
                yield return "stability of an AnimalGeneDef needs to be between -2 and 2.";
            }

        }

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    if (iconPath.NullOrEmpty())
                    {
                        cachedIcon = BaseContent.BadTex;
                    }
                    else
                    {
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath) ?? BaseContent.BadTex;
                    }
                }
                return cachedIcon;
            }
        }

        public Color IconColor
        {
            get
            {
                if (iconColor.HasValue)
                {
                    return iconColor.Value;
                }
                
                return Color.white;
            }
        }

        public string GetDescriptionFull()
        {
            StringBuilder sb = new StringBuilder();
            if (!description.NullOrEmpty())
            {
                sb.Append(description).AppendLine().AppendLine();
            }

            /*bool flag2 = false;
            if (biostatCpx != 0)
            {
                sb.AppendLineTagged("Complexity".Translate().Colorize(GeneUtility.GCXColor) + ": " + biostatCpx.ToStringWithSign());
                flag2 = true;
            }
            
            if (flag2)
            {
                sb.AppendLine();
            }*/
            bool effectsTitleWritten = false;
            if (!statFactors.NullOrEmpty())
            {
                for (int i = 0; i < statFactors.Count; i++)
                {
                    StatModifier statModifier = statFactors[i];
                    if (statModifier.stat.CanShowWithLoadedMods())
                    {
                        AppendEffectLine(statModifier.stat.LabelCap + " " + statModifier.ToStringAsFactor);
                    }
                }
            }
            
            if (!statOffsets.NullOrEmpty())
            {
                for (int l = 0; l < statOffsets.Count; l++)
                {
                    StatModifier statModifier3 = statOffsets[l];
                    if (statModifier3.stat.CanShowWithLoadedMods())
                    {
                        AppendEffectLine(statModifier3.stat.LabelCap + " " + statModifier3.ValueToStringAsOffset);
                    }
                }
            }
           
            return sb.ToString().TrimEndNewlines();
            void AppendEffectLine(string text)
            {
                if (!effectsTitleWritten)
                {
                    sb.AppendLineTagged(("Effects".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    effectsTitleWritten = true;
                }
                sb.AppendLine("  - " + text);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
           
            if (statOffsets != null)
            {
                for (int k = 0; k < statOffsets.Count; k++)
                {
                    StatModifier statModifier = statOffsets[k];
                    if (statModifier.stat.CanShowWithLoadedMods())
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier.stat.LabelCap, statModifier.ValueToStringAsOffset, statModifier.stat.description, 4070);
                    }
                }
            }
            if (statFactors != null)
            {
                for (int k = 0; k < statFactors.Count; k++)
                {
                    StatModifier statModifier2 = statFactors[k];
                    if (statModifier2.stat.CanShowWithLoadedMods())
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, statModifier2.stat.LabelCap, statModifier2.ToStringAsFactor, statModifier2.stat.description, 4070);
                    }
                }
            }
            
        }

    }
}
