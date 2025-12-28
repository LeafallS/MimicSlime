using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class Hediff_MimicSlime : HediffWithComps
    {
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.TryGetComp<CompTraitsOnSpawn>(out CompTraitsOnSpawn comp))
                {
                    comp.Notify_KilledPawn(pawn);
                }
            }
        }
    }
}
