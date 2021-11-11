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
}