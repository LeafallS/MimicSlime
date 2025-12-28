using RimWorld;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class Verb_MimicSlimeCastAbilityTouch : Verb_CastAbilityTouch
    {
        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target.IsValid && JumpUtility.ValidJumpTarget(this.CasterPawn, this.caster.Map, target.Cell))
            {
                GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
            }
            GenDraw.DrawRadiusRing(this.caster.Position, this.EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(this.caster.Position, c, this.caster.Map) && JumpUtility.ValidJumpTarget(this.caster, this.caster.Map, c));
        }

        public override void OnGUI(LocalTargetInfo target)
        {
            if (this.CanHitTarget(target) && JumpUtility.ValidJumpTarget(this.CasterPawn, this.caster.Map, target.Cell))
            {
                base.OnGUI(target);
                return;
            }
            GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return this.caster != null && this.CanHitTarget(target) && JumpUtility.ValidJumpTarget(this.CasterPawn, this.caster.Map, target.Cell);
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            bool num = base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);

            if (!CurrentTarget.TryGetPawn(out Pawn targetPawn)) return false;
            if (!targetPawn.health.hediffSet.HasHediff(MimicSlimeDefOf.MimicSlimeAnesthetic))
            {
                targetPawn.health.AddHediff(MimicSlimeDefOf.MimicSlimeAnesthetic);
                TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
            }

            if (this.CasterPawn.Drawer.renderer.CurAnimation != MimicSlimeDefOf.MimicSlimeCastAbilityTouchAnimation)
            {
                this.CasterPawn.Drawer.renderer.SetAnimation(MimicSlimeDefOf.MimicSlimeCastAbilityTouchAnimation);
            }

            if (targetPawn.Drawer.renderer.CurAnimation != MimicSlimeDefOf.MimicSlimeUseAbilityAnimation)
            {
                targetPawn.Drawer.renderer.SetAnimation(MimicSlimeDefOf.MimicSlimeUseAbilityAnimation);
            }
            MoteMaker.MakeStaticMote(targetPawn.Position.ToVector3Shifted(), targetPawn.MapHeld, MimicSlimeDefOf.SlimeUseAbilityMote);

            return num;
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();

            if (this.CasterPawn.Drawer.renderer.CurAnimation == MimicSlimeDefOf.MimicSlimeCastAbilityTouchAnimation)
            {
                this.CasterPawn.Drawer.renderer.SetAnimation(null);
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (this.CasterPawn.Drawer.renderer.CurAnimation == MimicSlimeDefOf.MimicSlimeCastAbilityTouchAnimation)
            {
                this.CasterPawn.Drawer.renderer.SetAnimation(null);
            }
        }
    }
}
