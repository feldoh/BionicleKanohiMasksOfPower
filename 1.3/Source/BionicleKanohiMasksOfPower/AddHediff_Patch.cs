using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
	{
		typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
	})]
	public static class AddHediff_Patch
	{
		private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (___pawn.Wears(BionicleDefOf.BKMOP_Kaukau) && (hediff.def?.defName == "SpaceHypoxia" 
				|| hediff.def == HediffDefOf.ToxicBuildup
				|| hediff.def == HediffDefOf.Heatstroke
				|| hediff.def == HediffDefOf.Hypothermia))
			{
				return false;
			}
			return true;
		}
	}
}