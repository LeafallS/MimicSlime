using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;
using static HarmonyLib.Code;

namespace MimicSlime
{
    public class Verb_MakeInvisibility : Verb_CastBase
    {
        protected override bool TryCastShot()
        {
            if (currentTarget.TryGetPawn(out Pawn pawn) && !pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.PhantomMantleInvisibility))
            {
                pawn.health.AddHediff(MimicSlimeDefOf.PhantomMantleInvisibility);

                CompApparelReloadable reloadableCompSource = base.ReloadableCompSource;
                reloadableCompSource?.UsedOnce();
                return true;
            }
            return false;
        }
    }
}
