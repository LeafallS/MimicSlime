using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class DeathActionWorker_MimicSlime : DeathActionWorker_WildSlime
    {
        protected override void PlaceThing(Pawn pawn, IntVec3 position, Map map)
        {
            Thing essence = ThingMaker.MakeThing(MimicSlimeDefOf.MimicSlimeEssence);
            essence.stackCount = 3;

            Thing core = ThingMaker.MakeThing(MimicSlimeDefOf.MimicSlimeCoreItem);
            CompSlimeCore comp = core.TryGetComp<CompSlimeCore>();
            comp.ImprintPawn(pawn);

            GenPlace.TryPlaceThing(essence, position, map, ThingPlaceMode.Near);
            GenPlace.TryPlaceThing(core, position, map, ThingPlaceMode.Near);
        }
    }
}
