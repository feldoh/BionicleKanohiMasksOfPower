﻿using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(SkillRecord), "Level", MethodType.Getter)]
	public static class SkillRecord_Level_Patch
	{
		public static void Postfix(SkillRecord __instance, ref int __result)
		{
			if ((__instance.def == SkillDefOf.Melee || __instance.def == SkillDefOf.Shooting) && __instance.Pawn.Wears(BionicleDefOf.BKMOP_Akaku))
            {
				__result = Mathf.Min(20, __result + 5);
			}
			else if ((__instance.def == SkillDefOf.Social || __instance.def == SkillDefOf.Intellectual || __instance.def == SkillDefOf.Crafting) 
				&& __instance.Pawn.Wears(BionicleDefOf.BKMOP_Rau))
            {
				__result = Mathf.Min(20, __result + 5);
			}
		}
	}
}