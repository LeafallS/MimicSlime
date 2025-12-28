using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MimicSlime
{
    public class FissionCompProperties : HediffCompProperties
    {
        public float severityThreshold = 1.0f;
        public FissionCompProperties() => compClass = typeof(FissionHediffComp);
    }

    public class FissionHediffComp : HediffComp
    {
        public FissionCompProperties Props => (FissionCompProperties)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (parent.Severity >= Props.severityThreshold)
                TriggerFission();
        }

        private void TriggerFission()
        {
            Pawn parentPawn = parent.pawn;
            if (parentPawn == null ||
                parentPawn.Dead ||
                parentPawn.Map == null)
                return;

            Pawn newSlime = PawnGenerator.GeneratePawn(
                parentPawn.kindDef,
                parentPawn?.Faction
            );
            newSlime.ageTracker.AgeBiologicalTicks = 0;
            newSlime.ageTracker.AgeChronologicalTicks = 0;
            GenSpawn.Spawn(newSlime, parentPawn.Position, parentPawn.Map);

            parent.Severity = 0.01f;

            Messages.Message(
                "Slime_FissionComplete".Translate(parentPawn.LabelShort),
                newSlime,
                MessageTypeDefOf.SilentInput
            );
        }
    }
}
