using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MimicSlime
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "DropBloodFilth")]

    public static class ActuallySlimeHarmonyPatch_DropBloodFilth
    {
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if ((___pawn.Spawned || ___pawn.ParentHolder is Pawn_CarryTracker) && ___pawn.SpawnedOrAnyParentSpawned && ___pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime))
            {
                FilthMaker.TryMakeFilth(___pawn.PositionHeld, ___pawn.MapHeld, MimicSlimeDefOf.MimicSlimeBloodDef, ___pawn.LabelIndefinite());
                return false;
            }
            return true;
        }
    }
}