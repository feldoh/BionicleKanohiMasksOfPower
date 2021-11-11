using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Down : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Komau apparel;
		public Command_Down(Apparel_Komau apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickDown > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickDown;
				if (cooldownTicksRemaining < Apparel_Komau.CooldownDownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Komau.CooldownDownTicks, 0, cooldownTicksRemaining);
					Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
				}
			}
			if (result.State == GizmoState.Interacted)
			{
				return result;
			}
			return new GizmoResult(result.State);
        }
	}

	[StaticConstructorOnStartup]
	public class Command_MentalState : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Komau apparel;
		public Command_MentalState(Apparel_Komau apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickMentalState > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickMentalState;
				if (cooldownTicksRemaining < Apparel_Komau.CooldownMentalStateTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Komau.CooldownMentalStateTicks, 0, cooldownTicksRemaining);
					Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
				}
			}
			if (result.State == GizmoState.Interacted)
			{
				return result;
			}
			return new GizmoResult(result.State);
		}
	}

	public class Apparel_Komau : Apparel
    {
		public int lastUsedTickDown;
		public int lastUsedTickMentalState;
		public const int CooldownDownTicks = 900;
        public const int CooldownMentalStateTicks = 900;
        public static TargetingParameters TargetingParameters(Pawn pawn)
		{
            return new TargetingParameters
            {
                canTargetPawns = true,
                validator = (TargetInfo x) => x.Thing is Pawn other && other != pawn && !other.Downed
			};
        }
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var g in base.GetWornGizmos())
            {
                yield return g;
            }
            if (Wearer.IsColonistPlayerControlled)
            {
				yield return new Command_Down(this)
				{
					defaultLabel = "Bionicle.DownPawn".Translate(),
					defaultDesc = "Bionicle.DownPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var hediff = HediffMaker.MakeHediff(BionicleDefOf.BKMOP_MakeDown, localTargetInfo.Pawn);
							localTargetInfo.Pawn.health.AddHediff(hediff);
							lastUsedTickDown = Find.TickManager.TicksGame;
						});
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickDown + Apparel_Komau.CooldownDownTicks > Find.TickManager.TicksGame
				};

				yield return new Command_MentalState(this)
				{
					defaultLabel = "Bionicle.ChangeMentalState".Translate(),
					defaultDesc = "Bionicle.ChangeMentalStateDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var pawn = localTargetInfo.Pawn;
							if (pawn.MentalState != null)
                            {
								pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
							}
                            else
                            {
								pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                            }
							lastUsedTickMentalState = Find.TickManager.TicksGame;
						});
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickMentalState + Apparel_Komau.CooldownMentalStateTicks > Find.TickManager.TicksGame
				};
			}
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTickDown, "lastUsedTick");
        }
    }
}