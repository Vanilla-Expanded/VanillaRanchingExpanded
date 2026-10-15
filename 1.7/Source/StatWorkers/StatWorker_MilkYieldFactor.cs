
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_MilkYieldFactor : StatWorker
    {
      
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            stringBuilder.AppendLine();
           
            Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
            if (pawn != null)
            {
                CompMilkable comp = pawn.TryGetComp<CompMilkable>();
                if (comp != null)
                {
                    stringBuilder.AppendLine("VRE_MilkYieldFactorDetails".Translate(comp.Props.milkAmount, (int)(comp.Props.milkAmount*finalVal)));
                    stringBuilder.AppendLine();
                }
                
            }
           
            return stringBuilder.ToString();
        }
    }
}