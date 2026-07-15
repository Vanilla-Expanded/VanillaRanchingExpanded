
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
    public class ITab_AnimalGenes : ITab
    {
        protected Vector2 scrollPosition;

        protected const float TopPadding = 20f;

        public const float GeneSize = 90f;

        public const float GeneGap = 6f;

        public const int MaxGenesHorizontal = 7;

        public const float InitialWidth = 736f;

        protected const float InitialHeight = 550f;

       
        public override bool IsVisible => CanShowGenesTab();

       
        protected Pawn SelPawnForGenes => PawnForGenes(SelThing);

        public ITab_AnimalGenes()
        {
            size = new Vector2(Mathf.Min(736f, UI.screenWidth), 550f);
            labelKey = "VRE_TabAnimalGenes";
        }

      

        protected override void FillTab()
        {
            GeneUIUtility.DrawGenesInfo(new Rect(0f, 20f, size.x, size.y - 20f), Find.Selector.SingleSelectedThing, 550f, ref size, ref scrollPosition);
        }

       

        private static Pawn PawnForGenes(Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                return pawn;
            }
            Corpse corpse = thing as Corpse;
            if (corpse != null)
            {
                return corpse.InnerPawn;
            }
            return null;
        }

        public static bool CanShowGenesTab()
        {
           
            Pawn pawn = PawnForGenes(Find.Selector.SingleSelectedThing);
          
            if (pawn != null && StaticCollections.ranchingAnimals.Contains(pawn.kindDef))
            {
                return true;
            }
           
            return false;
        }
    }
}