using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using System.Reflection;
namespace VanillaRanchingExpanded
{
    
    public class VanillaRanchingExpanded_Mod : Mod
    {
        public VanillaRanchingExpanded_Mod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.VanillaRanchingExpanded");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
