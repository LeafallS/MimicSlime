using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MimicSlime
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class SlimeHarmonyPatch_Kill
    {
        private static bool Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (MimicSlimeUtility.IsOriginalSlime(__instance))
            {
                MimicSlimeUtility.NotDeath(__instance, false);

                TaggedString label = "SlimeDeath".Translate(__instance.LabelShortCap);
                TaggedString text = "SlimeDeathText".Translate(__instance.LabelShortCap);
                Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Death, __instance);
            }

            else if (__instance.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime))
            {
                Pawn slime = MimicSlimeUtility.GetOriginalSlime(__instance);
                if (slime == null)
                {
                    Log.Error("SlimeHarmonyPatch_Kill: Fail to find original slime");
                    return true;
                }

                if (slime.Spawned) slime.DeSpawn();
                GenSpawn.Spawn(slime, __instance.Position, __instance.MapHeld);

                if (slime.Faction != __instance.Faction)
                {
                    slime.SetFaction(__instance.Faction);
                }

                if (slime.Dead)
                {
                    ResurrectionUtility.TryResurrect(slime);
                }
                slime.Kill(dinfo, exactCulprit);

                MimicSlimeUtility.NotDeath(__instance);
            }
            return true; 
        }

        private static void Postfix(Pawn __instance)
        {
            if (MimicSlimeUtility.IsOriginalSlime(__instance))
            {
                RemoveApparel(__instance);
            }
            else if (__instance.Corpse != null && __instance.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime))
            {
                RemoveApparel(__instance);
                ActuallySlimeHarmonyPatch_NotifyPlayerOfKilled.pawnNotDead = __instance;
                ActuallySlimeHarmonyPatch_Ideo_Notify_MemberDied.pawnNotDead = __instance;
                MimicSlimeUtility.StripPawn(__instance);
                __instance.Corpse.Destroy();
                ActuallySlimeHarmonyPatch_NotifyPlayerOfKilled.pawnNotDead = null;
                ActuallySlimeHarmonyPatch_Ideo_Notify_MemberDied.pawnNotDead = null;
            }
        }

        private static void RemoveApparel(Pawn pawn)
        {
            foreach (var a in pawn.apparel.WornApparel)
            {
                a.WornByCorpse = false;
            }
        }
    }
}