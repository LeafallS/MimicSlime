using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;


namespace MimicSlime
{
    public class Ability_Parasite : Ability
    {
        public Ability_Parasite() { }

        public Ability_Parasite(Pawn pawn) : base(pawn) { }

        public Ability_Parasite(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool CanApplyOn(LocalTargetInfo target)
        {
            Pawn targetPawn = target.Pawn;
            if (targetPawn == null || !MimicSlimeUtility.IsValidTarget(targetPawn))
            {
                Messages.Message(
                    "Ability_GiveSlimeParasite_InvalidTarget".Translate(),
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            {
                Messages.Message(
                    MessageTypeDefOf.RejectInput
                );
                return false;
            }

            // 检查目标是否已有寄生虫
            {
                Messages.Message(
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

            Hediff_SlimeParasite parasite = (Hediff_SlimeParasite)HediffMaker.MakeHediff(MimicSlimeDefOf.SlimeParasite, targetPawn);
            parasite.Faction = pawn.Faction;
            targetPawn.health.AddHediff(parasite);
            parasite.Severity = 0.01f;

            Messages.Message(
                "Ability_GiveSlimeParasite_Success".Translate(this.pawn.NameShortColored, targetPawn.NameShortColored),
                new LookTargets(this.pawn, targetPawn),
                MessageTypeDefOf.SilentInput
            );

            return base.Activate(targetPawn, dest);
        }
    }
}
