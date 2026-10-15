using Verse;
using Verse.AI;

namespace VanillaRanchingExpanded
{
    public class ThinkNode_ConditionalCanHerd : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn.def != InternalDefOf.VRE_Collie)
            {
                return false;
            }
            if (ModsConfig.OdysseyActive)
            {
                return pawn.training.HasLearned(InternalDefOf.VRE_Herd);
            }
            return true;
        }
    }
}
