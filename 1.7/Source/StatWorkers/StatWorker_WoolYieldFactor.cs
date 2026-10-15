
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_WoolYieldFactor : StatWorker
    {

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            stringBuilder.AppendLine();

            Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
            if (pawn != null)
            {
                CompShearable comp = pawn.TryGetComp<CompShearable>();
                if (comp != null)
                {
                    stringBuilder.AppendLine("VRE_WoolYieldFactorDetails".Translate(comp.Props.woolAmount, comp.Props.woolDef.label, (int)(comp.Props.woolAmount * finalVal)));
                    stringBuilder.AppendLine();
                }

            }

            return stringBuilder.ToString();
        }
    }
}