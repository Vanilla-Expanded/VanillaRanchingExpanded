
using System;
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_EggIntervalFactor : StatWorker
    {

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
            stringBuilder.AppendLine();

            Pawn pawn = req.Pawn ?? (req.Thing as Pawn);
            if (pawn != null)
            {
                CompEggLayer comp = pawn.TryGetComp<CompEggLayer>();
                if (comp != null)
                {
                    stringBuilder.AppendLine("VRE_EggIntervalFactorDetails".Translate(comp.Props.eggLayIntervalDays, comp.Props.eggLayIntervalDays * finalVal));
                    stringBuilder.AppendLine();
                }

            }

            return stringBuilder.ToString();
        }
    }
}