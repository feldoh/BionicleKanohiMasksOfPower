﻿using HarmonyLib;
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
	public class Command_Mahiki : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Mahiki apparel;
		public Command_Mahiki(Apparel_Mahiki apparel)
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
				if (cooldownTicksRemaining < Apparel_Mahiki.CooldownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Mahiki.CooldownTicks, 0, cooldownTicksRemaining);
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

    public class Hediff_Duplicate : HediffWithComps
    {
        public int initTick;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            initTick = Find.TickManager.TicksGame;
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame > initTick + 600)
            {
                this.pawn.Destroy();
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref initTick, "initTick");
        }
    }
	public class Apparel_Mahiki : Apparel
    {
		public int lastUsedTick;
		public const float EffectiveRange = 15f;
		public const int CooldownTicks = 900;
		public static bool CanHitTargetFrom(Pawn caster, IntVec3 root, LocalTargetInfo targ)
		{
			float num = EffectiveRange * EffectiveRange;
			IntVec3 cell = targ.Cell;
			if ((float)caster.Position.DistanceToSquared(cell) <= num)
			{
				return GenSight.LineOfSight(root, cell, caster.Map);
			}
			return false;
		}
		public static void DrawHighlight(Pawn caster, LocalTargetInfo target)
		{
			if (target.IsValid && ValidJumpTarget(caster.Map, target.Cell))
			{
				GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
			}
			GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && ValidJumpTarget(caster.Map, c));
		}

		public static bool ValidJumpTarget(Map map, IntVec3 cell)
		{
			if (!cell.IsValid || !cell.InBounds(map))
			{
				return false;
			}
			if (cell.Impassable(map) || !cell.Walkable(map) || cell.Fogged(map))
			{
				return false;
			}
			Building edifice = cell.GetEdifice(map);
			Building_Door building_Door;
			if (edifice != null && (building_Door = edifice as Building_Door) != null && !building_Door.Open)
			{
				return false;
			}
			return true;
		}

		public static TargetingParameters TargetingParameters(Pawn pawn)
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				canTargetPawns = false,
				canTargetBuildings = false,
				validator = (TargetInfo x) => CanHitTargetFrom(pawn, pawn.Position, x.Cell) && ValidJumpTarget(pawn.Map, x.Cell)
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
				yield return new Command_Mahiki(this)
				{
					defaultLabel = "Bionicle.CreateClone".Translate(),
					defaultDesc = "Bionicle.CreateCloneDesc".Translate(),
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters(Wearer), delegate (LocalTargetInfo localTargetInfo)
						{
                            var pawn = PawnUtility.GetPawnDuplicate(Wearer, Wearer.kindDef);
                            var hediff = HediffMaker.MakeHediff(BionicleDefOf.BKMOP_PawnDuplicate, pawn);
                            pawn.health.AddHediff(hediff);
                            GenSpawn.Spawn(pawn, localTargetInfo.Cell, Wearer.Map);
                            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_DefendPoint(localTargetInfo.Cell, addFleeToil: false), Wearer.Map, Gen.YieldSingle(pawn));
							lastUsedTick = Find.TickManager.TicksGame;
						}, highlightAction: (LocalTargetInfo x) =>
						{
							GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
							DrawHighlight(Wearer, x);
						}, null, Wearer);
					},
					onHover = delegate
					{
						GenDraw.DrawRadiusRing(Wearer.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(Wearer.Position, c, Wearer.Map) && ValidJumpTarget(Wearer.Map, c));
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTick + Apparel_Mahiki.CooldownTicks > Find.TickManager.TicksGame
				};
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }

	public static class PawnUtility
    {
        public static Pawn GetPawnDuplicate(Pawn origin, PawnKindDef newPawnKindDef)
        {
            NameTriple nameTriple = origin.Name as NameTriple;
            Pawn newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(newPawnKindDef, origin.Faction, PawnGenerationContext.NonPlayer,
                fixedGender: origin.gender, fixedBirthName: nameTriple.First, fixedLastName: nameTriple.Last));

            newPawn.Name = new NameTriple(nameTriple.First, nameTriple.Nick, nameTriple.Last);
            newPawn.story.childhood = origin.story.childhood;
            newPawn.story.adulthood = origin.story.adulthood;

            newPawn.playerSettings = new Pawn_PlayerSettings(newPawn);
            newPawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
            newPawn.playerSettings.AreaRestriction = origin.playerSettings.AreaRestriction;
            newPawn.playerSettings.medCare = origin.playerSettings.medCare;
            newPawn.playerSettings.selfTend = origin.playerSettings.selfTend;

            if (newPawn.foodRestriction == null) newPawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            if (origin.foodRestriction?.CurrentFoodRestriction != null) newPawn.foodRestriction.CurrentFoodRestriction = origin.foodRestriction?.CurrentFoodRestriction;
            if (newPawn.outfits == null) newPawn.outfits = new Pawn_OutfitTracker();
            if (origin.outfits?.CurrentOutfit != null) newPawn.outfits.CurrentOutfit = origin.outfits?.CurrentOutfit;
            if (newPawn.drugs == null) newPawn.drugs = new Pawn_DrugPolicyTracker();
            if (origin.drugs?.CurrentPolicy != null) newPawn.drugs.CurrentPolicy = origin.drugs?.CurrentPolicy;
            if (newPawn.timetable == null) newPawn.timetable = new Pawn_TimetableTracker(newPawn);
            if (origin.timetable?.times != null) newPawn.timetable.times = origin.timetable?.times;


            if (newPawn.Faction != origin.Faction)
            {
                newPawn.SetFaction(origin.Faction);
            }

            if (newPawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = newPawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    newPawn.needs.mood.thoughts.memories.RemoveMemory(newPawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            newPawn.story.traits.allTraits.Clear();
            var traits = origin.story?.traits?.allTraits;
            if (traits != null)
            {
                foreach (var trait in traits)
                {
                    newPawn.story.traits.GainTrait(trait);
                }
            }
            newPawn.relations.ClearAllRelations();
            var skills = origin.skills.skills;
            newPawn.skills.skills.Clear();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var newSkill = new SkillRecord(newPawn, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    newPawn.skills.skills.Add(newSkill);
                }
            }

            var apparels = origin.apparel?.WornApparel ?? new List<Apparel>();
            newPawn.apparel.DestroyAll();
            for (int num = apparels.Count - 1; num >= 0; num--)
            {
                var apparel = ThingMaker.MakeThing(apparels[num].def, apparels[num].Stuff) as Apparel;
                newPawn.apparel.Wear(apparel);
            }

            var equipments = origin.equipment?.AllEquipmentListForReading ?? new List<ThingWithComps>();
            newPawn.equipment.DestroyAllEquipment();
            for (int num = equipments.Count - 1; num >= 0; num--)
            {
                var equipment = ThingMaker.MakeThing(equipments[num].def, equipments[num].Stuff) as ThingWithComps;
                newPawn.equipment.AddEquipment(equipment);
            }

            var inventoryThings = origin.inventory?.innerContainer?.ToList() ?? new List<Thing>();
            newPawn.inventory.DestroyAll();
            for (int num = inventoryThings.Count - 1; num >= 0; num--)
            {
                var thing = ThingMaker.MakeThing(inventoryThings[num].def, inventoryThings[num].Stuff);
                thing.stackCount = inventoryThings[num].stackCount;
                newPawn.inventory.TryAddItemNotForSale(thing);
            }

            newPawn.apparel.LockAll();
            return newPawn;
        }
    }
}