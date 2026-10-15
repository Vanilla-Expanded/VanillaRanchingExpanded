using System.Collections.Generic;
using Verse;

namespace VanillaRanchingExpanded
{
    public class CompAnimalAssignments : ThingComp
    {
        public HashSet<Pawn> forceIncluded = new();
        public HashSet<Pawn> forceExcluded = new();

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref forceIncluded, "forceIncluded", LookMode.Reference);
            Scribe_Collections.Look(ref forceExcluded, "forceExcluded", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                forceIncluded ??= new();
                forceExcluded ??= new();
                forceIncluded.RemoveWhere(x => x == null);
                forceExcluded.RemoveWhere(x => x == null);
            }
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            forceIncluded.RemoveWhere(p => p.Dead || p.Destroyed);
            forceExcluded.RemoveWhere(p => p.Dead || p.Destroyed);
        }
    }
}
