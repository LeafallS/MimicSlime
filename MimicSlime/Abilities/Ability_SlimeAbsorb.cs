using RimWorld;
using Verse;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse.AI;
using System;
using UnityEngine;
using System.Linq;
using Verse.Noise;

namespace MimicSlime
{
    public class Ability_SlimeAbsorb : Ability
    {
        public Ability_SlimeAbsorb() { }

        public Ability_SlimeAbsorb(Pawn pawn) : base(pawn) { }

        public Ability_SlimeAbsorb(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool CanApplyOn(LocalTargetInfo target)
        {
            if (target.Pawn == null)
            {
                Messages.Message(
                    "Ability_SlimeAbsorb_InvalidTarget".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            Pawn slime = MimicSlimeUtility.GetOriginalSlime(pawn);
            if (slime == null)
            {
                Log.Error("Ability SlimeAbsorb cant find originalSlime");
                return false;
            }

            if (MimicSlimeGlobalManager.Instance.GetOriginalSlimeQueue(slime).Count >= MimicSlimeModSettings.QueueStackLimit)
            {
                Messages.Message(
                    "Ability_SlimeAbsorb_QueueFull".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            if (!target.Cell.InHorDistOf(pawn.Position, def.verbProperties.range) || !JumpUtility.ValidJumpTarget(pawn, target.Pawn.Map, target.Cell))
            {
                Messages.Message(
                    "Ability_SlimeAbsorb_OutOfRange".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            if (MechanitorUtility.IsMechanitor(target.Pawn))
            {
                Messages.Message(
                    "Warning_MimicSlimeMechanitor".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            return base.CanApplyOn(target);
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn targetPawn = target.Pawn;
            if (targetPawn == null)
            { 
                Log.Error("Ability SlimeAbsorb Invalid Target");
                return false;
            }
            Pawn originalSlime = MimicSlimeUtility.GetOriginalSlime(pawn);
            if (originalSlime == null)
            {
                Log.Error("Ability SlimeAbsorb cant find originalSlime");
                return false;
            }

            if (ModsConfig.RoyaltyActive && targetPawn.HasPsylink) AbsorbPsy(targetPawn, pawn);

            var manager = MimicSlimeGlobalManager.Instance;
            manager.AddPawnToQueueAndRemovePawn(originalSlime, targetPawn);
            pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
            MimicSlimeUtility.AddAbsorbedThought(targetPawn);

            return base.Activate(target, dest);
        }

        private void AbsorbPsy(Pawn fromPawn, Pawn toPawn)
        {
            if (!toPawn.HasPsylink)
            {
                SlimeData fromData = new SlimeData();
                fromData.CaptureFromPawn(fromPawn);
                fromData.ApplyToPawn(toPawn, false, false, true, false);
            }
            else AddPsyAbility(fromPawn, toPawn);
        }

        private void AddPsyAbility(Pawn fromPawn, Pawn toPawn)
        {
            if (MaxLevel(fromPawn.GetPsylinkLevel(), toPawn.GetPsylinkLevel(), out int level))
            {
                List<AbilityDef> toResume = toPawn.abilities.abilities.Select(a => a.def).ToList().FindAll(a => a.IsPsycast);
                toPawn.ChangePsylinkLevel(level);
                List<AbilityDef> toRemove = toPawn.abilities.abilities.Select(a => a.def).ToList().FindAll(a => a.IsPsycast && !toResume.Contains(a));
                foreach (AbilityDef a in toRemove)
                {
                    toPawn.abilities.RemoveAbility(a);
                }
            }

            foreach (AbilityDef def in fromPawn.abilities.abilities.Select(a => a.def).ToList())
            {
                if (def.IsPsycast)
                {
                    toPawn.abilities.GainAbility(def);
                }
            }
        }

        private static bool MaxLevel(int val1, int val2, out int level)
        {
            if (val1 < val2)
            {
                level = val2;
                return false;
            }
            level = val1;
            return true;
        }

        public override void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
        {
            if (!this.CanQueueCast || !this.CanApplyOn(target))
            {
                return;
            }
            if (this.verb.verbProps.nonInterruptingSelfCast)
            {
                this.verb.TryStartCastOn(this.verb.Caster, false, true, false, false);
                return;
            }
            this.pawn.jobs.TryTakeOrderedJob(this.GetJob(target, destination), new JobTag?(JobTag.Misc), false);
            if (this.CanApplyOn(target))
            {
                if (target.Pawn.CurJobDef != JobDefOf.Wait) target.Pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait));
                JumpUtility.DoJump(this.pawn, target, null, this.def.verbProperties, this, destination);
            }
        }
    }
}