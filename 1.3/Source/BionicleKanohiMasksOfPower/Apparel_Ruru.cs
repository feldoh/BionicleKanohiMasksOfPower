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
	public class Command_Ruru : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Ruru apparel;
		public Command_Ruru(Apparel_Ruru apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTick > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTick;
				if (cooldownTicksRemaining < Apparel_Ruru.CooldownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Ruru.CooldownTicks, 0, cooldownTicksRemaining);
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
	public class Apparel_Ruru : Apparel
    {
		public int lastUsedTick;
		public const int CooldownTicks = 600;
        public static TargetingParameters TargetingParameters(Pawn pawn)
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                validator = (TargetInfo x) => x.Thing is Pawn other && other != pawn && GenSight.LineOfSight(pawn.Position, other.Position, pawn.Map)
            };
        }


        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var g in base.GetWornGizmos())
            {
                yield return g;
            }
            if (Wearer.IsColonistPlayerControlled && this.IsMasterworkOrLegendary())
			{
				yield return new Command_Ruru(this)
				{
					defaultLabel = "Bionicle.StunPawn".Translate(),
					defaultDesc = "Bionicle.StunPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							localTargetInfo.Pawn.stances.stunner.StunFor(300, Wearer);
							lastUsedTick = Find.TickManager.TicksGame;
						});
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTick + Apparel_Ruru.CooldownTicks > Find.TickManager.TicksGame
				};
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }
}