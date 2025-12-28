using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class CompProperties_Fission : CompProperties
    {
        public CompProperties_Fission() => compClass = typeof(CompFission);
    }

    public class CompFission : ThingComp
    {
        public CompProperties_Fission Props => (CompProperties_Fission)props;
        private Pawn Pawn => (Pawn)parent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Pawn.health.GetOrAddHediff(MimicSlimeDefOf.FissionHediff);
        }
    }
}
