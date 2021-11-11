using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(PawnCapacityWorker_Sight), "CalculateCapacityLevel")]
	public static class PawnCapacityWorker_Sight_CalculateCapacityLevel_Patch
	{
		private static void Postfix(ref float __result, HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			if (diffSet.pawn.Wears(BionicleDefOf.BKMOP_Akaku))
			{
				__result *= 2f;
			}
		}
	}

	[HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
	public static class ThoughtWorker_Dark_CurrentStateInternal_Patch
	{
		private static void Postfix(ref ThoughtState __result, Pawn p)
		{
			if (p.Wears(BionicleDefOf.BKMOP_Ruru))
			{
				__result = false;
			}
		}
	}
}