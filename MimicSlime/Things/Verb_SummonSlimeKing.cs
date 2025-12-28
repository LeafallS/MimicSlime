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
    public class Verb_SummonSlimeKing : Verb_CastBase
    {
        protected override bool TryCastShot()
        {
            Map map = caster.Map;
            if (map == null)
            {
                Log.Error("Tried to spawn slime king on null map");
                return false;
            }

            // 寻找生成位置
            if (!CellFinder.TryFindRandomCellNear(currentTarget.Cell, map, 5,
                c => c.Walkable(map) && c.Standable(map), out IntVec3 spawnPos))
            {
                spawnPos = CellFinder.RandomClosewalkCellNear(currentTarget.Cell, map, 1);
            }

            // 生成史莱姆王
            Pawn slimeKing = MimicSlimeUtility.SpawnSlimeKing(map, spawnPos, Faction.OfPlayer);

            // 添加召唤状态Hediff
            Hediff summonedHediff = HediffMaker.MakeHediff(HediffDef.Named("SummonedSlimeKing"), slimeKing);
            slimeKing.health.AddHediff(summonedHediff);
            slimeKing.training.Train(TrainableDefOf.Obedience, null);
            slimeKing.training.Train(TrainableDefOf.Obedience, null);
            slimeKing.training.Train(TrainableDefOf.Obedience, null);
            slimeKing.training.Train(TrainableDefOf.Release, null);
            slimeKing.training.Train(TrainableDefOf.Release, null);


            CompApparelReloadable reloadableCompSource = base.ReloadableCompSource;
            reloadableCompSource?.UsedOnce();
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return 4.9f;
        }
    }
}
