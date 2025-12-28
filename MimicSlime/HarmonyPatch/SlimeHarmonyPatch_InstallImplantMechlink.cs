using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    [HarmonyPatch(typeof(CompUseEffect_InstallImplantMechlink), nameof(CompUseEffect_InstallImplantMechlink.CanBeUsedBy))]
    public static class SlimeHarmonyPatch_InstallImplantMechlink
    {
        static void Postfix(Pawn p, ref AcceptanceReport __result)
        {
            if (p.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime) || MimicSlimeUtility.IsOriginalSlime(p))
            {
                Messages.Message(
                    "Warning_MimicSlimeMechanitor".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                __result = false;
            }
        }
    }
}