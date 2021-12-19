using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Disarm : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Matatu apparel;
		public Command_Disarm(Apparel_Matatu apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickDisarm > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickDisarm;
				if (cooldownTicksRemaining < Apparel_Matatu.CooldownDisarmTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Matatu.CooldownDisarmTicks, 0, cooldownTicksRemaining);
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
	public class Command_WallRaise : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Matatu apparel;
		public Command_WallRaise(Apparel_Matatu apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTickWallRaise > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTickWallRaise;
				if (cooldownTicksRemaining < Apparel_Matatu.CooldownWallRaise)
				{
					float num = Mathf.InverseLerp(Apparel_Matatu.CooldownWallRaise, 0, cooldownTicksRemaining);
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

	public class Apparel_Matatu : Apparel
	{
		public int lastUsedTickDisarm;
		public int lastUsedTickWallRaise;

		public const int CooldownDisarmTicks = 900;
		public const int CooldownWallRaise = 900;
		public TargetingParameters TargetingParameters(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => x.Thing is Pawn other && other != pawn && !other.Downed
			};
		}
		public TargetingParameters TargetingParametersPawn(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => x.Thing is Pawn other && other != pawn && pawn.Position.DistanceTo(other.Position) <= 27f
			};
		}

		public TargetingParameters TargetingParametersTeleport(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				validator = (TargetInfo x) => x.Cell.Walkable(pawn.Map) && x.Cell.DistanceTo(pawn.Position) <= 27f
			};
		}
		public TargetingParameters TargetingParametersCell(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				validator = (TargetInfo x) => CanApplyOn(new LocalTargetInfo(x.Cell))
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
				yield return new Command_Disarm(this)
				{
					defaultLabel = "Bionicle.DisarmPawn".Translate(),
					defaultDesc = "Bionicle.DisarmPawnDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							var equipment = localTargetInfo.Pawn.equipment?.Primary;
							if (equipment != null)
							{
								localTargetInfo.Pawn.equipment.TryDropEquipment(equipment, out _, localTargetInfo.Pawn.Position);
							}
						});
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickDisarm + Apparel_Matatu.CooldownDisarmTicks > Find.TickManager.TicksGame
				};

				yield return new Command_WallRaise(this)
				{
					defaultLabel = "Bionicle.WallRaise".Translate(),
					defaultDesc = "Bionicle.WallRaiseDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParametersCell(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
							Apply(localTargetInfo);
							lastUsedTickWallRaise = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) => DrawEffectPreview(x), null, Wearer);
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTickWallRaise + Apparel_Matatu.CooldownWallRaise > Find.TickManager.TicksGame
				};

				yield return new Command_Action
				{
					defaultLabel = "Bionicle.Pull".Translate(),
					defaultDesc = "Bionicle.PullDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParametersPawn(Wearer), delegate (LocalTargetInfo target)
						{
							Find.Targeter.BeginTargeting(TargetingParametersTeleport(target.Pawn), delegate (LocalTargetInfo dest)
							{
								target.Thing.Position = dest.Cell;
								Pawn pawn2 = target.Thing as Pawn;
								if (pawn2 != null)
								{
									pawn2.stances.stunner.StunFor(60, Wearer, addBattleLog: false, showMote: false);
								}
							}, highlightAction: (LocalTargetInfo targetInfo) =>
							{
								var fields = GenRadial.RadialCellsAround(target.Pawn.Position, 27f, true).Where(x => x.Walkable(Wearer.Map));
								GenDraw.DrawFieldEdges(fields.ToList());
								GenDraw.DrawTargetHighlightWithLayer(targetInfo.Cell, AltitudeLayer.MetaOverlays);
							}, null, Wearer);
						}, highlightAction: (LocalTargetInfo targetInfo) =>
						{
							var fields = GenRadial.RadialCellsAround(Wearer.Position, 27f, true);
							GenDraw.DrawFieldEdges(fields.ToList());
							GenDraw.DrawTargetHighlightWithLayer(targetInfo.Cell, AltitudeLayer.MetaOverlays);
						}, null, Wearer);
					},
					icon = this.def.uiIcon,
				};
			}
		}
		public void Apply(LocalTargetInfo target)
		{
			Map map = Wearer.Map;
			List<Thing> list = new List<Thing>();
			list.AddRange(AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
																			   where t.def.category == ThingCategory.Item
																			   select t));
			foreach (Thing item in list)
			{
				item.DeSpawn();
			}
			foreach (IntVec3 item2 in AffectedCells(target, map))
			{
				GenSpawn.Spawn(ThingDefOf.RaisedRocks, item2, map);
				FleckMaker.ThrowDustPuffThick(item2.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), CompAbilityEffect_Wallraise.DustColor);
			}
			foreach (Thing item3 in list)
			{
				IntVec3 intVec = IntVec3.Invalid;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 intVec2 = item3.Position + GenRadial.RadialPattern[i];
					if (intVec2.InBounds(map) && intVec2.Walkable(map) && map.thingGrid.ThingsListAtFast(intVec2).Count <= 0)
					{
						intVec = intVec2;
						break;
					}
				}
				if (intVec != IntVec3.Invalid)
				{
					GenSpawn.Spawn(item3, intVec, map);
				}
				else
				{
					GenPlace.TryPlaceThing(item3, item3.Position, map, ThingPlaceMode.Near);
				}
			}
		}

		public bool CanApplyOn(LocalTargetInfo target)
		{
			return Valid(target, throwMessages: false);
		}

		public void DrawEffectPreview(LocalTargetInfo target)
		{
			GenDraw.DrawFieldEdges(AffectedCells(target, Wearer.Map).ToList(), Valid(target) ? Color.white : Color.red);
		}

		private static List<IntVec2> pattern = new List<IntVec2>
		{
			new IntVec2(0, 0), new IntVec2(1, 0), new IntVec2(-1, 0), new IntVec2(0, 1), new IntVec2(0, -1)
		};
		private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
		{
			foreach (IntVec2 item in pattern)
			{
				IntVec3 intVec = target.Cell + new IntVec3(item.x, 0, item.z);
				if (intVec.InBounds(map))
				{
					yield return intVec;
				}
			}
		}

		public bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			if (AffectedCells(target, Wearer.Map).Any((IntVec3 c) => c.Filled(Wearer.Map)))
			{
				return false;
			}
			if (AffectedCells(target, Wearer.Map).Any((IntVec3 c) => !c.Standable(Wearer.Map)))
			{
				if (throwMessages)
				{
					Messages.Message("AbilityUnwalkable".Translate(Wearer.def.LabelCap), target.ToTargetInfo(Wearer.Map), MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}
	
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastUsedTickDisarm, "lastUsedTickDisarm");
			Scribe_Values.Look(ref lastUsedTickWallRaise, "lastUsedTickWallRaise");
		}
	}
}