using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MimicSlime
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[]
    {
        typeof(PawnGenerationRequest)
    })]
    public static class SlimeHarmonyPatch_GeneratePawn
    {
        public static void Postfix(Pawn __result)
        {
            var manager = MimicSlimeGlobalManager.Instance;

            if (Rand.Chance(MimicSlimeModSettings.ActuallySlimeChance / 100) && __result != null && __result.RaceProps.Humanlike && !MimicSlimeUtility.IsSlime(__result))
            {
                Pawn slime = MimicSlimeUtility.CreateSlimePawn(MimicSlimeDefOf.MimicSlime_Colonist, __result?.Faction);
                manager.RegisterOriginalSlime(slime);
                manager.AddEvoluPoints(slime, UnityEngine.Random.Range(10, 31));
                manager.AddPawnToQueueNoRemove(slime, __result);
                __result.health.AddHediff(MimicSlimeDefOf.ActuallySlime);
            }

            if (__result.def == MimicSlimeDefOf.MimicSlimeRace)
            {
                __result.kindDef.GetModExtension<OnsetSlimePointsModExtension>()?.TryAddPoints(__result);
                __result.story.Adulthood.GetModExtension<OnsetSlimePointsModExtension>()?.TryAddPoints(__result);

                if (manager.GetEvoluPoints(__result) >= 20)
                {
                    Hediff slimeHediff = __result.health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.MimicSlime);
                    if (slimeHediff != null)
                    {
                        __result.health.RemoveHediff(slimeHediff);
                        __result.health.AddHediff(MimicSlimeDefOf.HumanoidMimicSlime);
                        __result.story.bodyType = MimicSlimeDefOf.MimicSlimeBody_HumanoidType;
                        MimicSlimeUtility.ChangeHair(__result, -1);
                        __result.Drawer.renderer.SetAllGraphicsDirty();
                    }

                    Hediff_SlimeLevel levelHediff = (Hediff_SlimeLevel)__result.health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.SlimeLevel);
                    levelHediff.updateNotify = true;
                    MimicSlimeUtility.AddAbility(MimicSlimeDefOf.ParasiteAbility, __result);
                }
            }
        }
    }
}
