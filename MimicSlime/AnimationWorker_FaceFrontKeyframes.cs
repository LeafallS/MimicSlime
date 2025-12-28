using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class AnimationWorker_FaceFrontKeyframes : AnimationWorker_Keyframes
    {
        public override float AngleAtTick(int tick, AnimationDef def, PawnRenderNode node, AnimationPart part, PawnDrawParms parms)
        {
            return base.AngleAtTick(tick, def, node, part, parms) - parms.pawn.Drawer.renderer.wiggler.downedAngle;
        }
    }
}
