
using System;
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_MilkIntervalOffset : StatWorker
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
                    
                    stringBuilder.AppendLine("VRE_MilkIntervalOffsetDetails".Translate(comp.Props.milkIntervalDays, (int)Math.Max(comp.Props.milkIntervalDays + finalVal,1)));
                    stringBuilder.AppendLine();
                }

            }

            return stringBuilder.ToString();
        }
    }
}