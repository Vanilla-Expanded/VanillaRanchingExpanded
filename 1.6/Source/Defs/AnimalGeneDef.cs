using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
    public enum AnimalGeneStability
    {
        Awful,
        Poor,
        Baseline,
        Good,
        Excellent
    }

    public class AnimalGeneDef: Def
    {
        [NoTranslate]
        public string iconPath;

        private Color? iconColor;

        [Unsaved(false)]
        private Texture2D cachedIcon;

        public AnimalGeneStability stability;

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

    }
}
