using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled")]
    public static class ActuallySlimeHarmonyPatch_Royalty_Notify_PawnKilled
    {
        public static Pawn pawnNotDead;

        public static bool Prefix(Pawn_RoyaltyTracker __instance)
        {
            if (ActuallySlimeHarmonyPatch_Royalty_Notify_PawnKilled.pawnNotDead == __instance.pawn)
            {
                ActuallySlimeHarmonyPatch_Royalty_Notify_PawnKilled.pawnNotDead = null;
                return false;
            }
            return true;
        }
    }
}
