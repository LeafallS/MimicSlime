using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;

namespace MimicSlime
{
    public class DeathActionWorker_WildSlime : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            Pawn pawn = corpse.InnerPawn;
            IntVec3 position = corpse.PositionHeld;
            Map map = corpse.MapHeld;
            if (pawn == null || map == null)
            {
                return;
            }

            pawn?.Strip();
            corpse?.Destroy();
            PlaceThing(pawn, position , map);
            if (pawn.kindDef == MimicSlimeDefOf.GiantSlime)
            {
                for (int i = 0; i < 3; i++)
                {
                    Pawn child = MimicSlimeUtility.CreateSlimePawn(MimicSlimeDefOf.WildSlime, pawn.Faction);
                    this.SpawnPawn(child, pawn, position, map, prevLord);
                }
            }

            FilthMaker.TryMakeFilth(position, map, MimicSlimeDefOf.MimicSlimeBloodDef, 1, FilthSourceFlags.None, true);
        }

        protected virtual void PlaceThing(Pawn pawn, IntVec3 position, Map map)
        {
            Thing essence = ThingMaker.MakeThing(MimicSlimeDefOf.MimicSlimeEssence, null);
            essence.stackCount = 1;
            GenPlace.TryPlaceThing(essence, position, map, ThingPlaceMode.Near, null, null, null, 1);
        }

        private void SpawnPawn(Pawn child, Pawn parent, IntVec3 position, Map map, Lord lord)
        {
            GenSpawn.Spawn(child, position, map, WipeMode.VanishOrMoveAside);
            if (lord == null)
            {
                lord = LordMaker.MakeNewLord(parent.Faction, new LordJob_AssaultColony(), map, null);
            }
            lord?.AddPawn(child);
            child.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
            CompInspectStringEmergence compInspectStringEmergence = child.TryGetComp<CompInspectStringEmergence>();
            if (compInspectStringEmergence != null)
            {
                compInspectStringEmergence.sourcePawn = parent;
            }
            FleshbeastUtility.SpawnPawnAsFlyer(child, map, position, 5, true);
        }
    }
}
