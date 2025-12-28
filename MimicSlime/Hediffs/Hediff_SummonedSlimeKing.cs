using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class Hediff_SummonedSlimeKing : HediffWithComps
    {
        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.Destroy(DestroyMode.Vanish);
        }
    }
}
