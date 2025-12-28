using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class IncidentWorker_SlimeRain : IncidentWorker
    {
        // 事件参数
        public const float BaseWealthThreshold = 5000f;   // 基础财富阈值
        private const int MinDurationTicks = 60000;       // 最小持续时间(1天)
        private const int MaxDurationTicks = 120000;      // 最大持续时间(2天)

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            SendStandardLetter(
                "SlimeRain_LetterLabel".Translate(),
                "SlimeRain_LetterText".Translate(),
                LetterDefOf.ThreatSmall,
                parms,
                new LookTargets(map.Center, map)
            );
            map.weatherManager.TransitionTo(DefDatabase<WeatherDef>.GetNamed("Rain"));

            GameCondition_SlimeRain condition = (GameCondition_SlimeRain)GameConditionMaker.MakeCondition(
                DefDatabase<GameConditionDef>.GetNamed("SlimeRain"),
                Rand.Range(MinDurationTicks, MaxDurationTicks)
            );

            condition.map = map;
            map.gameConditionManager.RegisterCondition(condition);

            return true;
        }
    }

    public class GameCondition_SlimeRain : GameCondition
    {
        public Map map;
        private Faction slimeFaction = FactionUtility.DefaultFactionFrom(MimicSlimeDefOf.HostileWildSlime);
        private const int CheckInterval = 60;
        
        public override void GameConditionTick()
        {
            base.GameConditionTick();
            if (Find.TickManager.TicksGame % CheckInterval != 0)
                return;

            // 计算基于财富的生成概率
            float wealthFactor = map.wealthWatcher.WealthTotal / IncidentWorker_SlimeRain.BaseWealthThreshold;
            float spawnChance = Mathf.Clamp(wealthFactor, 6, 51)/100;

            if (Rand.Chance(spawnChance))
            {
                if (TryFindSpawnPoint(map, out IntVec3 spawnPoint))
                {
                    int value = UnityEngine.Random.Range(1, 201);
                    PawnKindDef slimeKind;

                    if (value == 1 && MimicSlimeModSettings.SpawnSlimeKingInSlimeRain) // 0.5% 
                    {
                        MimicSlimeUtility.SpawnSlimeKing(map, spawnPoint, slimeFaction);
                        return;
                    }
                    else if (value < 12 && MimicSlimeModSettings.SpawnGiantSlimeInSlimeRain) // 5% 
                    {
                        slimeKind = MimicSlimeDefOf.GiantSlime;
                    }
                    else
                    {
                        slimeKind = MimicSlimeDefOf.WildSlime;
                    }

                    // 生成史莱姆
                    SpawnSlime(map, spawnPoint, slimeKind);
                }
            }
        }

        private bool TryFindSpawnPoint(Map map, out IntVec3 result)
        {
            // 尝试寻找合适的生成点
            return CellFinderLoose.TryFindRandomNotEdgeCellWith(10, (IntVec3 c) =>
                IsValidSpawnCell(map, c), map, out result);
        }

        private bool IsValidSpawnCell(Map map, IntVec3 cell)
        {
            // 确保单元格可通行且不在屋顶下
            if (!cell.Walkable(map)) return false;
            if (cell.Roofed(map)) return false;
            return true;
        }

        private void SpawnSlime(Map map, IntVec3 cell, PawnKindDef slimeKind)
        {
            Pawn slime = PawnGenerator.GeneratePawn(slimeKind, slimeFaction);
            IntVec3 dropPos = cell;
            dropPos.z = map.Size.z - 1;
            GenSpawn.Spawn(slime, dropPos, map);
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, slime, cell, null, MimicSlimeDefOf.Slime_Drop);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, cell, map);
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref slimeFaction, "slimeFaction");
        }
    }
}
