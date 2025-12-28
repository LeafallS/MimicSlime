using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MimicSlime
{
    public class CompProperties_Usable_CallSlimeKing : CompProperties_UseEffect
    {
        public CompProperties_Usable_CallSlimeKing()
        {
            compClass = typeof(CompUseEffect_CallSlimeKing);
        }

        public string factionDefName = "HostileWildSlime";
        public PawnKindDef bossKind;
        public int delayTicks = 300;
    }

    public class CompUseEffect_CallSlimeKing : CompUseEffect
    {
        public new CompProperties_Usable_CallSlimeKing Props =>
            (CompProperties_Usable_CallSlimeKing)props;

        private Faction SlimeFaction =>
            Find.FactionManager.FirstFactionOfDef(FactionDef.Named(Props.factionDefName));

        private int summonStartTick = -1;
        private bool isSummoning;
        private Effecter prepareEffecter;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            // 清理准备特效
            prepareEffecter?.Cleanup();
            prepareEffecter = null;

            // 开始召唤
            isSummoning = true;
            summonStartTick = Find.TickManager.TicksGame;

            // 显示开始召唤消息
            Messages.Message("SlimeKingSummonStarted".Translate(), parent, MessageTypeDefOf.NeutralEvent);
        }

        // 每帧检查召唤状态
        public override void CompTick()
        {
            base.CompTick();

            // 处理召唤倒计时
            if (isSummoning && Find.TickManager.TicksGame >= summonStartTick + Props.delayTicks)
            {
                isSummoning = false;
                SpawnBoss();
            }

            // 更新准备特效
            PrepareTick();
        }

        private void SpawnBoss()
        {
            Map map = parent.Map;
            if (map == null)
            {
                Log.Error("Tried to spawn slime boss on null map");
                return;
            }

            // 寻找生成位置
            if (!CellFinder.TryFindRandomCellNear(parent.Position, map, 5,
                c => c.Walkable(map) && c.Standable(map), out IntVec3 spawnPos))
            {
                spawnPos = CellFinder.RandomClosewalkCellNear(parent.Position, map, 1);
            }

            // 生成史莱姆王
            Pawn boss = MimicSlimeUtility.SpawnSlimeKing(map, spawnPos, SlimeFaction);

            string letterLabel = "SlimeKingSpawn_Lable".Translate();
            string letterText = "SlimeKingSpawn_Text".Translate();
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.ThreatBig, boss);
        }

        // 使用条件检查
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            // 阵营检查
            if (SlimeFaction == null || SlimeFaction.defeated)
                return "SlimeFactionDisabled".Translate(Props.factionDefName);

            // 正在召唤中
            if (isSummoning)
                return "SlimeBossgroupInProgress".Translate();

            return true;
        }

        // 保存/加载状态
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref summonStartTick, "summonStartTick", -1);
            Scribe_Values.Look(ref isSummoning, "isSummoning", false);
        }

        // 生成后发送预告信件
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                SendPreviewLetter();
            }
        }

        private void SendPreviewLetter()
        {
            if (Props.bossKind == null) return;

            string rewardsList = Props.bossKind.race.killedLeavingsPlayerHostile
                .Select(r => r.Label + " x" + r.count.ToString())
                .ToLineList("- ", false);

            Find.LetterStack.ReceiveLetter(
                "SlimeKingSpawnPreview_Lable".Translate(Props.bossKind.label),
                "SlimeKingSpawnPreview_Text".Translate(
                    parent.def.label,
                    Props.bossKind.label,
                    rewardsList
                ),
                LetterDefOf.NeutralEvent,
                null, null, null,
                new List<ThingDef> { parent.def },
                null, 0, true
            );
        }
    }
}