using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaRanchingExpanded;
using Verse;
using VEF.AnimalGenes;

namespace VanillaRanchingExpanded
{
    public class AnimalGamete : ThingWithComps
    {
        public Pawn gameteFather;

        public override bool CanStackWith(Thing other)
        {
            return false;
        }   

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref gameteFather, "gameteFather");
        }

      
        public override string LabelNoCount
        {
            get
            {
                if (gameteFather != null)
                {
                    return gameteFather.kindDef.GetLabelGendered(gameteFather.gender) + " "+ this.def.label;
                }
                return base.LabelNoCount;
            }
        }


    }
}
