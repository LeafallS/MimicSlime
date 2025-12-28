using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MimicSlime
{
    public class Hediff_SlimeCure : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            Pawn_HealthTracker health = pawn.health;
            Hediff slimeParasite = health.GetOrAddHediff(MimicSlimeDefOf.SlimeParasite);
            health.RemoveHediff(slimeParasite);
        }
    }
}