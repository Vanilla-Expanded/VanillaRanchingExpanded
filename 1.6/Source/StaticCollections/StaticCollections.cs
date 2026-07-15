using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaRanchingExpanded
{
    [StaticConstructorOnStartup]
    public static class StaticCollections
    {
        public static List<PawnKindDef> ranchingAnimals = new List<PawnKindDef>();

        static StaticCollections(){
        
            foreach(FeratypeDef feratype in DefDatabase<FeratypeDef>.AllDefsListForReading)
            {
                ranchingAnimals.Add(feratype.race);

            }
        }

    }
}
