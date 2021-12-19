using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    public class Recipe_TransferQuality : RecipeWorker
    {

    }
    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]
    public static class Patch_MakeRecipeProducts
    {
        public static void Postfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (recipeDef.workerClass == typeof(Recipe_TransferQuality))
            {
                foreach (var i in ingredients)
                {
                    if (i.TryGetQuality(out QualityCategory qc))
                    {
                        foreach (var t in __result)
                        {
                            var comp = t.TryGetComp<CompQuality>();
                            if (comp != null)
                            {
                                comp.SetQuality(qc, ArtGenerationContext.Colony);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static bool IsMasterworkOrLegendary(this Thing thing)
        {
            return true;
            if (thing.TryGetQuality(out var qc))
            {
                return qc == QualityCategory.Masterwork || qc == QualityCategory.Legendary;
            }
            return false;
        }
    }
}