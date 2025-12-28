using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace MimicSlime
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]

    public static class ActuallySlimeHarmonyPatch_NotifyPlayerOfKilled
    {
        public static Pawn pawnNotDead;

        // 拦截死亡通知
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff, Caravan caravan)
        {
            if (pawnNotDead == ___pawn)
            {
                try
                {
                    pawnNotDead = null;
                    return false; // 跳过原版死亡通知
                }
                catch (Exception ex)
                {
                    Log.Error($"Error in NotifyPlayerOfKilled prefix: {ex}");
                }
                pawnNotDead = null;
            }
            return true;
        }
    }
}