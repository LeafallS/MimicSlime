using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MimicSlime
{
    [HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.GetPartsToApplyOn))]
    public static class SlimeHarmonyPatch_RemoveBodyPartRecipe
    {
        static void Postfix(Pawn pawn, ref IEnumerable<BodyPartRecord> __result)
        {
            if (pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime))
            {
                __result = Enumerable.Empty<BodyPartRecord>();
            }
        }
    }
}