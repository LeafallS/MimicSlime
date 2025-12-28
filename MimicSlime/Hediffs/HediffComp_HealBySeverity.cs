using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class HediffComp_HealBySeverity : HediffComp
    {
        private int ticksToHeal;
        public HediffCompProperties_HealBySeverity Props => (HediffCompProperties_HealBySeverity)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            // 默认间隔15天
            ticksToHeal = Props?.intervalDays != null
                ? (int)(Props.intervalDays * 60000)
                : 15 * 60000;
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            ticksToHeal -= delta;
            if (ticksToHeal <= 0)
            {
                TryHealWeightedRandomWound(base.Pawn, parent.LabelCap);
                ResetTicksToHeal();
            }
        }

        private void TryHealWeightedRandomWound(Pawn pawn, string cause)
        {
            var validHediffs = pawn.health.hediffSet.hediffs
                .Where(h => h.IsPermanent() || h.def.chronic)
                .ToList();

            if (!validHediffs.Any()) return;

            float totalSeverity = validHediffs.Sum(h => h.Severity);

            if (totalSeverity <= 0)
            {
                TryHealRandom(validHediffs, pawn, cause);
                return;
            }

            float randomPoint = Rand.Value * totalSeverity;
            float currentSum = 0f;

            foreach (var hediff in validHediffs)
            {
                currentSum += hediff.Severity;
                if (currentSum >= randomPoint)
                {
                    CureHediff(hediff, pawn, cause);
                    return;
                }
            }

            // 作为后备：选择严重度最高的
            var mostSevere = validHediffs.OrderByDescending(h => h.Severity).First();
            CureHediff(mostSevere, pawn, cause);
        }

        private void TryHealRandom(List<Hediff> hediffs, Pawn pawn, string cause)
        {
            if (hediffs.TryRandomElement(out Hediff hediff))
            {
                CureHediff(hediff, pawn, cause);
            }
        }

        private void CureHediff(Hediff hediff, Pawn pawn, string cause)
        {
            HealthUtility.Cure(hediff);

            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message(
                    "MessagePermanentWoundHealed".Translate(
                        cause, pawn.LabelShort, hediff.Label),
                    pawn,
                    MessageTypeDefOf.PositiveEvent,
                    true
                );
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
        }

        public override string CompDebugString()
        {
            return $"ticksToHeal: {ticksToHeal}\n" +
                   $"Next heal in: {ticksToHeal / 60000f:F1} days";
        }
    }

    public class HediffCompProperties_HealBySeverity : HediffCompProperties
    {
        public float intervalDays = -1; 

        public HediffCompProperties_HealBySeverity()
        {
            compClass = typeof(HediffComp_HealBySeverity);
        }
    }
}
