using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MimicSlime
{
    public class DeathActionWorker_SlimeKing : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord lord)
        {
            Pawn pawn = corpse.InnerPawn;
            IntVec3 position = corpse.Position;
            Map map = corpse.Map;
            FilthMaker.TryMakeFilth(position, map, MimicSlimeDefOf.MimicSlimeBloodDef, 5, FilthSourceFlags.None, true);

            Thing essence = ThingMaker.MakeThing(MimicSlimeDefOf.MimicSlimeEssence, null);
            essence.stackCount = 10;
            GenPlace.TryPlaceThing(essence, position, map, ThingPlaceMode.Near, null, null, null, 1);

            if (pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.SummonedSlimeKing)) return;
            else
            {
                corpse.Destroy();
                Thing slimegun = ThingMaker.MakeThing(MimicSlimeDefOf.SlimeGun, null);
                slimegun.stackCount = 1;
                GenPlace.TryPlaceThing(slimegun, position, map, ThingPlaceMode.Near, null, null, null, 1);

                if (Rand.Chance(0.01f))
                {
                    SpawnQueen(position, map);
                }
                else
                {
                    Thing crown = ThingMaker.MakeThing(MimicSlimeDefOf.SlimeKingCrown_Submitted, null);
                    crown.stackCount = 1;
                    GenPlace.TryPlaceThing(crown, position, map, ThingPlaceMode.Near, null, null, null, 1);
                }
            }
        }

        private void SpawnQueen(IntVec3 position, Map map)
        {
            Pawn queen = MimicSlimeUtility.CreateSlimePawn(MimicSlimeDefOf.MimicSlime_Queen, Faction.OfPlayer, forceNoGear: false);
            var manager = MimicSlimeGlobalManager.Instance;
            manager.RegisterOriginalSlime(queen);
            GenSpawn.Spawn(queen, position, map);

            Find.LetterStack.ReceiveLetter(
                "SlimeQueenSpawnTitle".Translate(),
                "SlimeQueenSpawnDesc".Translate(
                    queen.NameShortColored
                ),
                LetterDefOf.PositiveEvent,
                new LookTargets(queen)
            );
        }
    }
}
