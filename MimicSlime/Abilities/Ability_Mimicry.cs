using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class Ability_Mimicry : Ability
    {
        public Ability_Mimicry() { }
        public Ability_Mimicry(Pawn pawn) : base(pawn) { }
        public Ability_Mimicry(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool CanApplyOn(LocalTargetInfo target)
        {
            if (pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.health.hediffSet.HasTendableHediff())
            {
                Messages.Message(
                    "Ability_Mimicry_Injured".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            if (MimicSlimeUtility.IsDigesting(pawn) && !MimicSlimeModSettings.IgnoreDigest)
            {
                Messages.Message(
                    "Ability_Mimicry_Digesting".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            return base.CanApplyOn(target);
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            try
            {
                // 获取管理器
                var manager = MimicSlimeGlobalManager.Instance;
                if (manager == null)
                {
                    Log.Error($"Ability_Mimicry : Try to get manager, but failed");
                    return false;
                }

                // 获取史莱姆
                var originalSlime = MimicSlimeUtility.GetOriginalSlime(pawn);
                if (originalSlime == null)
                {
                    Log.Error($"Ability_Mimicry : Try to find orignal slime from {pawn}, but failed");
                    return false;
                }

                // 打开对话框
                Find.WindowStack.Add(new Dialog_Mimicry(pawn, manager));
                return base.Activate(target, dest);
            }
            catch (Exception ex)
            {
                Log.Error($"Ability_Mimicry : Error while opening dialog: {ex}");
                return false;
            }
        }
    }
}