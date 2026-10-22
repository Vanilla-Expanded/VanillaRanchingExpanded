using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
    public class Gizmo_AnimalTroughNutrition : Gizmo_Slider
    {
        private readonly Building_AnimalTrough trough;


        public Gizmo_AnimalTroughNutrition(Building_AnimalTrough trough)
        {
            this.trough = trough;
        }

        protected override float Target
        {
            get => Mathf.Clamp01(trough.nutritionTarget / 100f);
            set => trough.nutritionTarget = Mathf.Round(value * 100f);
        }

        protected override float ValuePercent =>
            Mathf.Clamp01(trough.nutritionTarget / 100f);

        protected override FloatRange DragRange => FloatRange.ZeroToOne;

        protected override bool IsDraggable => true;

        protected override string Title => "VRE_NutritionToStore".Translate();

        protected override string BarLabel =>
            trough.nutritionTarget.ToString("0");

        protected override bool DraggingBar
        {
            get => trough.draggingNutritionBar;
            set => trough.draggingNutritionBar = value;
        }

        protected override string GetTooltip() =>
            "VRE_TargetNutrition".Translate();
    }
}
