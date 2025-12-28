using RimWorld;
using System;
using System.Security.Policy;
using Verse;

namespace MimicSlime
{
    public class OnsetSlimePointsModExtension : DefModExtension
    {
        public IntRange OnsetPoints = new IntRange(0, 0);

        public void TryAddPoints(Pawn pawn)
        {
            if (pawn == null || pawn.def != MimicSlimeDefOf.MimicSlimeRace) return;

            if (pawn?.health?.hediffSet == null) return;

            Hediff_SlimeLevel levelHediff = (Hediff_SlimeLevel)pawn.health.GetOrAddHediff(MimicSlimeDefOf.SlimeLevel);
            MimicSlimeGlobalManager.Instance?.AddEvoluPoints(pawn, OnsetPoints.RandomInRange);
        }
    }
}
