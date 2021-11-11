using HarmonyLib;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
	public static class Thing_TakeDamage_Patch
	{
		public static bool Prefix(Thing __instance, DamageInfo dinfo)
		{
			if (__instance is Pawn pawn && pawn.Wears(BionicleDefOf.BKMOP_Kaukau) && dinfo.Def?.defName == "VacuumDamage")
            {
				return false;
			}
			return true;
		}
	}
}