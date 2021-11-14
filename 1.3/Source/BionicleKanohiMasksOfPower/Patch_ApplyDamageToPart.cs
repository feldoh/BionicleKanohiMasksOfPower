using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace BionicleKanohiMasksOfPower
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
	public static class Patch_ApplyDamageToPart
	{
		public static void Prefix(ref DamageInfo dinfo, Pawn pawn, DamageResult result)
		{
			if (dinfo.Instigator is Pawn attacker && attacker.Wears(BionicleDefOf.BKMOP_Pakari))
			{
				dinfo.SetAmount(dinfo.Amount * 2f);
			}
		}
	}
    public static class PawnOverlayUtility
    {
        private static Dictionary<Material, Material> materials_DuplicatePawnOverlay = new Dictionary<Material, Material>();
        [TweakValue("00", 0f, 1f)] public static float colorR = 0.25f;
        [TweakValue("00", 0f, 1f)] public static float colorG = 0.20f;
        [TweakValue("00", 0f, 1f)] public static float colorB = 0.21f;
        [TweakValue("00", 0f, 1f)] public static float colorA = 0.67f;
        public static Color DuplicatePawnOverlay => new Color(colorR, colorG, colorB, colorA);
        public static Material GetDuplicateMat(Material baseMat)
        {
            if (!materials_DuplicatePawnOverlay.TryGetValue(baseMat, out Material value) || true)
            {
                value = MaterialAllocator.Create(baseMat);
                value.color = DuplicatePawnOverlay;
                if (materials_DuplicatePawnOverlay.ContainsKey(baseMat))
                {
                    materials_DuplicatePawnOverlay[baseMat] = value;
                }
                else
                {
                    materials_DuplicatePawnOverlay.Add(baseMat, value);
                }
            }
            return value;
        }
    }

    [StaticConstructorOnStartup]
    public static class PawnRenderer_RenderPawnAt_Patches
    {
        private static Pawn curPawn;
        static PawnRenderer_RenderPawnAt_Patches()
        {
            var renderPawnAtMethod = AccessTools.Method(typeof(PawnRenderer), "RenderPawnAt", new Type[]
            {
                typeof(Vector3),
                typeof(Rot4?),
                typeof(bool)
            });
            Core.harmony.Patch(renderPawnAtMethod, new HarmonyMethod(AccessTools.Method(typeof(PawnRenderer_RenderPawnAt_Patches), nameof(RenderPawnAtPrefix))));
            var getDamagedMatMethod = AccessTools.Method(typeof(DamageFlasher), "GetDamagedMat");
            Core.harmony.Patch(getDamagedMatMethod, new HarmonyMethod(AccessTools.Method(typeof(PawnRenderer_RenderPawnAt_Patches), nameof(GetDuplicateMat))));
        }

        [HarmonyPriority(10000)]
        private static void RenderPawnAtPrefix(Pawn ___pawn)
        {
            curPawn = ___pawn;
        }
        private static void GetDuplicateMat(ref Material baseMat)
        {
            if (curPawn != null && curPawn.IsDuplicate())
            {
                baseMat = PawnOverlayUtility.GetDuplicateMat(baseMat);
            }
        }

        public static bool IsDuplicate(this Pawn pawn)
        {
            return pawn.health.hediffSet?.hediffs?.Any(x => x.def == BionicleDefOf.BKMOP_PawnDuplicate) ?? false;
        }
    }
}