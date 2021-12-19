using HarmonyLib;
using RimWorld;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
	public static class StatExtension_GetStatValue_Patch
	{
		private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
		{
			if (thing is Pawn pawn)
			{
				if (stat == StatDefOf.MoveSpeed && pawn.Wears(BionicleDefOf.BKMOP_Kakama, out var apparel) && apparel.IsMasterworkOrLegendary())
				{
					__result *= 2f;
				}
				else if (stat == StatDefOf.CarryingCapacity && pawn.Wears(BionicleDefOf.BKMOP_Pakari, out var apparel2) && apparel2.IsMasterworkOrLegendary())
                {
					__result *= 2f;
				}
				else if (stat == StatDefOf.AimingDelayFactor && pawn.Wears(BionicleDefOf.BKMOP_Akaku, out var apparel3) && apparel3.IsMasterworkOrLegendary())
                {
					__result *= 0.5f;
                }
			}
		}
	}
}