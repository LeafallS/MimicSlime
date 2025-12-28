using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class PawnRenderNode_MimicSlimeHair : PawnRenderNode_Hair
    {
        public PawnRenderNode_MimicSlimeHair(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            Graphic graphic = base.GraphicFor(pawn);

            if (pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.MimicSlime) || pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.TerrorMimicSlime))
            {
                return null;
            }
            return graphic;
        }
    }
}
