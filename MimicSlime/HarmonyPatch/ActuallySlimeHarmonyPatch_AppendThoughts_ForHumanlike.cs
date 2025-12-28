using HarmonyLib;
using MimicSlime;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
    public class ActuallySlimeHarmonyPatch_AppendThoughts_ForHumanlike
    {
        public static Pawn pawnNotDead;

        public static bool Prefix(Pawn victim)
        {
            if (ActuallySlimeHarmonyPatch_AppendThoughts_ForHumanlike.pawnNotDead == victim)
            {
                ActuallySlimeHarmonyPatch_AppendThoughts_ForHumanlike.pawnNotDead = null;
                return false;
            }
            return true;
        }
    }
}
