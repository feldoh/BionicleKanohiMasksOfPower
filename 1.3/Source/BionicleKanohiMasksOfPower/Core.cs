using HarmonyLib;
using System.Linq;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [StaticConstructorOnStartup]
	public static class Core
    {
        public static Harmony harmony;
		static Core()
        {
            harmony = new Harmony("Core.Mod");
            harmony.PatchAll();
        }

		public static bool Wears(this Pawn pawn, ThingDef thingDef)
        {
			return pawn.apparel?.WornApparel?.Any(x => x.def == thingDef) ?? false;
        }
    }
}