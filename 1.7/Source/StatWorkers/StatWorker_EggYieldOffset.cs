
using System;
using System.Text;
using RimWorld;
using Verse;
namespace VanillaRanchingExpanded
{
    public class StatWorker_EggYieldOffset : StatWorker
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

                    stringBuilder.AppendLine("VRE_EggYieldOffsetDetails".Translate(comp.Props.eggCountRange.min, comp.Props.eggCountRange.max,Math.Max(comp.Props.eggCountRange.min + finalVal, 1), Math.Max(comp.Props.eggCountRange.max + finalVal, 1)));
                    stringBuilder.AppendLine();
                }

            }

            return stringBuilder.ToString();
        }
    }
}