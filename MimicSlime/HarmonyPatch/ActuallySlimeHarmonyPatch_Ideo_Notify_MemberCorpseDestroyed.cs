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
    [HarmonyPatch(typeof(Ideo), "Notify_MemberCorpseDestroyed")]
    public static class ActuallySlimeHarmonyPatch_Ideo_Notify_MemberCorpseDestroyed
    {
        public static Pawn pawnNotDead;

        public static bool Prefix(Pawn member)
        {
            if (ActuallySlimeHarmonyPatch_Ideo_Notify_MemberDied.pawnNotDead == member)
            {
                ActuallySlimeHarmonyPatch_Ideo_Notify_MemberDied.pawnNotDead = null;
                return false;
            }
            return true;
        }
    }
}
