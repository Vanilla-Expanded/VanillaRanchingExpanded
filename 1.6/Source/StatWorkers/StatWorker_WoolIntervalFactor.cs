
using System;
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_WoolIntervalFactor : StatWorker
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
                    stringBuilder.AppendLine("VRE_WoolIntervalFactorDetails".Translate(comp.Props.shearIntervalDays,(int)Math.Max(comp.Props.shearIntervalDays * finalVal, 1)));
                    stringBuilder.AppendLine();
                }

            }

            return stringBuilder.ToString();
        }
    }
}