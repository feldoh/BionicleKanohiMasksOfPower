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

    public class CompProperties_Corrupt : CompProperties//adds comp for items
    {
        public string swapGroupKey;
        public CompProperties_Corrupt()
        {
            this.compClass = typeof(CompCorrupt);
        }
    }

    public class CompCorrupt : ThingComp
    {
        public CompProperties_Corrupt Props => base.props as CompProperties_Corrupt;
        public Apparel Apparel => this.parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;
        public override void Notify_Equipped(Pawn Wearer)
        {
            base.Notify_Equipped(Wearer);
            if (Apparel.WornByCorpse)
            {
                HealthUtility.AdjustSeverity(Wearer, HediffDefOf.Scaria, 1f);
                Wearer.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);//casues manhunter behavior to start
            }
        }

        public override void Notify_Unequipped(Pawn Wearer)
        {
            base.Notify_Unequipped(Wearer);
            var hediff = Wearer.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Scaria);
            if (hediff != null)
            {
                Wearer.health.RemoveHediff(hediff);
                Wearer.MentalState.RecoverFromState();//should remove manhunter behavior
            }
        }
    }
}