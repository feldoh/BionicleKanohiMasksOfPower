using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(PawnCapacityWorker_Manipulation), "CalculateCapacityLevel")]
	public static class PawnCapacityWorker_Manipulation_CalculateCapacityLevel_Patch
	{
		private static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (diffSet.pawn.Wears(BionicleDefOf.BKMOP_Kakama))
            {
				__result *= 2f;
			}
		}
	}

	[HarmonyPatch(typeof(ITab_Pawn_Gear), "ShouldShowInventory")]
	public static class ITab_Pawn_Gear_ShouldShowInventory_Patch
	{
		private static void Postfix(ref bool __result, Pawn p)
		{
			if (p.health?.hediffSet?.HasHediff(BionicleDefOf.BKMOP_PawnDuplicate) ?? false)
            {
				__result = false;
			}
		}
	}
}