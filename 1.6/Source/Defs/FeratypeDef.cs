using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
    public class FeratypeDef: Def
    {

        [NoTranslate]
        public string iconPath;
        [Unsaved(false)]
        private Texture2D cachedIcon;

        public PawnKindDef race;

        public static readonly Color IconColor = new Color(0.75f, 0.75f, 0.75f);

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
                        cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
                    }
                }
                return cachedIcon;
            }
        }
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }
            if (iconPath.NullOrEmpty())
            {
                yield return "iconPath is empty.";
            }
           
        }
    }
}
