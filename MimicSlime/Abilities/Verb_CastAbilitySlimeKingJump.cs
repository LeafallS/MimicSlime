using RimWorld;
using Verse;
using Verse.AI;

namespace MimicSlime
{
    public class Verb_CastAbilitySlimeKingJump : Verb_Jump
    {
        protected override bool TryCastShot()
        {
            Map map = CasterPawn.Map;

            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(
                MimicSlimeDefOf.SlimeKing_JumpWithDamageFlyer,
                CasterPawn,
                currentTarget.Cell,
                verbProps.flightEffecterDef,
                verbProps.soundLanding,
                verbProps.flyWithCarriedThing
            );

            if (pawnFlyer != null && map != null)
            {
                FleckMaker.ThrowDustPuff(
                    CasterPawn.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f),
                    map, 2f);

                GenSpawn.Spawn(pawnFlyer, currentTarget.Cell, map, WipeMode.Vanish);
                return true;
            }
            return false;
        }
    }
}