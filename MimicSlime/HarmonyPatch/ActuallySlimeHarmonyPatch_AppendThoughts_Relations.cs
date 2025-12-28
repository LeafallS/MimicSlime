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

    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
    public class ActuallySlimeHarmonyPatch_AppendThoughts_Relations
    {
        public static Pawn pawnNotDead;

        public static bool Prefix(Pawn victim)
        {
            if (ActuallySlimeHarmonyPatch_AppendThoughts_Relations.pawnNotDead == victim)
            {
                ActuallySlimeHarmonyPatch_AppendThoughts_Relations.pawnNotDead = null;
                return false;
            }
            return true;
        }
    }
}
