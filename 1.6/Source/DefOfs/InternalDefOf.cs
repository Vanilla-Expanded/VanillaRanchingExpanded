using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace VanillaRanchingExpanded
{
	[DefOf]
	public static class InternalDefOf
	{
		static InternalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
		}

		public static StatDef VRE_HealthFromGenesScale;
        public static StatDef VRE_ManhunterOnDamageFactor;
        public static StatDef VRE_ManhunterOnTameFailFactor;
		public static StatDef VRE_ManhunterOnDamageOffset;
		public static StatDef VRE_ManhunterOnTameFailOffset;
        public static StatDef VRE_ManhunterOnRopingChance;


    }
}
