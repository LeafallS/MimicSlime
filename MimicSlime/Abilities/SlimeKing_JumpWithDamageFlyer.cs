using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace MimicSlime
{
    public class SlimeKing_JumpWithDamageFlyer : PawnFlyer
    {
        public float damageRadius = 9.9f;
        public int damageAmount = 8;
        private Map cachedMap;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            cachedMap = map;
            base.SpawnSetup(map, respawningAfterLoad);
        }

        protected override void RespawnPawn()
        {
            Pawn instigator = this.FlyingPawn;

            base.RespawnPawn();

            DoJumpDamage(instigator);
        }

        private void DoJumpDamage(Pawn instigator)
        {
            if (instigator == null || cachedMap == null) return;

            IntVec3 center = base.Position;
            IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(center, damageRadius, true);

            foreach (IntVec3 cell in cells)
            {
                if (!cell.InBounds(cachedMap)) continue;

                foreach (Thing thing in cell.GetThingList(cachedMap).ToList())
                {
                    if (thing is Pawn pawn)
                    {
                        if (pawn == instigator)
                        {
                            pawn.stances.stunner.StunFor(300, pawn);
                        }
                        else
                        {
                            ApplyDamage(pawn, instigator);
                            pawn.stances.stunner.StunFor(120, pawn);
                        }
                    }
                }
            }

            FleckMaker.ThrowDustPuffThick(center.ToVector3Shifted(), cachedMap, 2f,
                new UnityEngine.Color(0.2f, 0.8f, 0.2f, 1f));
        }

        private void ApplyDamage(Thing target, Pawn instigator)
        {
            DamageInfo dinfo = new DamageInfo(
                def: DamageDefOf.Blunt,
                amount: damageAmount,
                instigator: instigator
            );
            target.TakeDamage(dinfo);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref cachedMap, "cachedMap");
        }
    }
}
