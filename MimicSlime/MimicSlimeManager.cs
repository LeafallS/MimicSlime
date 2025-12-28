using RimWorld;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class MimicSlimeGlobalManager : WorldComponent
    {
        private Dictionary<Pawn, Queue<Pawn>> originalSlimeQueues = new Dictionary<Pawn, Queue<Pawn>>();
        private Dictionary<Pawn, int> SlimesPoints = new Dictionary<Pawn, int>();
        private Dictionary<Pawn, int> SlimesHair = new Dictionary<Pawn, int>();

        private static MimicSlimeGlobalManager _instance;
        public static MimicSlimeGlobalManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Find.World.GetComponent<MimicSlimeGlobalManager>();
                return _instance;
            }
        }

        public MimicSlimeGlobalManager(World world) : base(world)
        {
            _instance = this;
        }

        // 注册原始史莱姆
        public void RegisterOriginalSlime(Pawn slime)
        {
            if (slime == null)
            {
                Log.Error($"{slime} not exist, can't register original slime");
                return;
            }

            if (!originalSlimeQueues.ContainsKey(slime))
            {
                originalSlimeQueues[slime] = new Queue<Pawn>();
                SlimesPoints[slime] = 0;
                SlimesHair[slime] = 0;
            }

            if (Find.AnyPlayerHomeMap != null)
            {
                MimicSlimeUtility.AddPawnToWorld(slime);
            }
        }

        // 获取原始史莱姆的队列
        public Queue<Pawn> GetOriginalSlimeQueue(Pawn slime)
        {
            if (slime == null) return new Queue<Pawn>();

            if (!originalSlimeQueues.ContainsKey(slime))
            {
                RegisterOriginalSlime(slime);
            }

            return originalSlimeQueues[slime];
        }

        public Pawn GetOriginalSlimeFromPawn(Pawn pawn)
        {
            if (MimicSlimeUtility.IsOriginalSlime(pawn) && !originalSlimeQueues.ContainsKey(pawn))
            {
                RegisterOriginalSlime(pawn);
                return pawn;
            }

            if (MimicSlimeUtility.IsOriginalSlime(pawn))
            {
                return pawn;
            }

            foreach (KeyValuePair<Pawn, Queue<Pawn>> kvp in originalSlimeQueues)
            {
                if (kvp.Value.Contains(pawn))
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        // 添加Pawn到原始史莱姆队列
        private void AddToQueue(Pawn slime, Pawn pawn)
        {
            var queue = GetOriginalSlimeQueue(slime);

            if (queue.Count >= MimicSlimeModSettings.QueueStackLimit)
            {
                RemoveOldestPawn(slime);
            }

            queue.Enqueue(pawn);
        }

        public void AddPawnToQueueNoRemove(Pawn slime, Pawn pawn)
        {
            if (MimicSlimeUtility.IsOriginalSlime(pawn)) return;
            if (!GetOriginalSlimeQueue(slime).Contains(pawn))
            {
                AddToQueue(slime, pawn);
            }
            pawn.health.AddHediff(MimicSlimeDefOf.ActuallySlime);
            MimicSlimeUtility.AddToFaction(slime, pawn);
        }

        public void AddPawnToQueueAndRemovePawn(Pawn slime, Pawn pawn)
        {
            MimicSlimeUtility.RemovePawn(pawn);
            AddPawnToQueueNoRemove(slime, pawn);
            MimicSlimeUtility.AddToFaction(slime, pawn);
        }

        // 从队列中移除Pawn
        public void RemoveFromQueue(Pawn slime, Pawn pawn)
        {
            var queue = GetOriginalSlimeQueue(slime);

            var newQueue = new Queue<Pawn>();
            bool removed = false;

            foreach (var p in queue)
            {
                if (p != null && pawn != null && p == pawn)
                {
                    removed = true;
                }
                else if (p != null)
                {
                    newQueue.Enqueue(p);
                }
            }

            originalSlimeQueues[slime] = newQueue;
        }

        // 移除最旧的Pawn
        private void RemoveOldestPawn(Pawn slime)
        {
            var queue = GetOriginalSlimeQueue(slime);

            if (queue.Count == 0) return;

            Pawn oldestPawn = queue.Dequeue();
            if (oldestPawn != null && !oldestPawn.Destroyed)
            {
                oldestPawn.Destroy();
            }
        }

        public int GetEvoluPoints(Pawn pawn)
        {
            Pawn slime = GetOriginalSlimeFromPawn(pawn);
            if (slime == null)
            {
                Log.Warning($"GetEvoluPoints: 无法从 {pawn} 获取原始史莱姆。");
                return 0;
            }

            if (!SlimesPoints.TryGetValue(slime, out int points))
            {
                RegisterOriginalSlime(slime);   // 兜底注册
                return 0;
            }

            return points;
        }

        public void AddEvoluPoints(Pawn pawn, int points)
        {
            Pawn slime = GetOriginalSlimeFromPawn(pawn);
            if (slime == null)
            {
                Log.Error($"AddEvoluPoints: 无法从 {pawn} 获取原始史莱姆。");
                return;
            }

            if (!SlimesPoints.ContainsKey(slime))
                RegisterOriginalSlime(slime);

            if (SlimesPoints[slime] < 0)
            {
                SlimesPoints[slime] = 0;
            }

            SlimesPoints[slime] += points;

            UpdateSlimeLevelHediff(slime, SlimesPoints[slime]);
        }

        public int GetHair(Pawn pawn)
        {
            Pawn slime = GetOriginalSlimeFromPawn(pawn);
            if (slime == null)
            {
                Log.Warning($"GetHair: 无法从 {pawn} 获取原始史莱姆。");
                return 0;
            }

            if (!SlimesHair.TryGetValue(slime, out int hair))
            {
                RegisterOriginalSlime(slime);
                return 0;
            }

            return hair;
        }

        public int ChangeHair(Pawn pawn, int hair)
        {
            Pawn slime = GetOriginalSlimeFromPawn(pawn);
            if (slime == null)
            {
                Log.Error($"ChangeHair: 无法从 {pawn} 获取原始史莱姆。");
                return 0;
            }

            if (!SlimesHair.ContainsKey(slime))
                RegisterOriginalSlime(slime);

            if (SlimesHair[slime] < 0)
            {
                SlimesHair[slime] = 0;
            }

            int last = SlimesHair[slime];
            SlimesHair[slime] = hair;
            return last;
        }

        private void UpdateSlimeLevelHediff(Pawn slime, int totalPoints)
        {
            if (slime?.health?.hediffSet == null) return;

            Hediff_SlimeLevel levelHediff = (Hediff_SlimeLevel)slime.health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.SlimeLevel);
            if (levelHediff == null) return;

            levelHediff.UpdateLevel(totalPoints);
        }

        // 获取所有Pawn
        public IEnumerable<Pawn> GetAllPawnsForSlime(Pawn slime)
        {
            return GetOriginalSlimeQueue(slime).AsEnumerable();
        }

        /* ==========  存档相关字段  ========== */
        private List<Pawn> originalSlimeQueues_keys;
        private List<List<Pawn>> originalSlimeQueues_values;
        private List<Pawn> slimesPoints_keys;
        private List<int> slimesPoints_values;
        private List<Pawn> slimesHair_keys;
        private List<int> slimesHair_values;

        public override void ExposeData()
        {
            base.ExposeData();

            /* ---------- originalSlimeQueues  <Pawn, Queue<Pawn>>  ---------- */
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                originalSlimeQueues_keys = originalSlimeQueues.Keys.ToList();
                originalSlimeQueues_values = originalSlimeQueues.Values.Select(q => q.ToList()).ToList();
            }

            Scribe_Collections.Look(ref originalSlimeQueues_keys, "originalSlimeQueues_keys", LookMode.Reference);
            Scribe_Collections.Look(ref originalSlimeQueues_values, "originalSlimeQueues_values", LookMode.Reference);

            /* ---------- SlimesPoints  <Pawn, int>  ---------- */
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                slimesPoints_keys = SlimesPoints.Keys.ToList();
                slimesPoints_values = SlimesPoints.Values.ToList();
            }

            Scribe_Collections.Look(ref slimesPoints_keys, "slimesPoints_keys", LookMode.Reference);
            Scribe_Collections.Look(ref slimesPoints_values, "slimesPoints_values", LookMode.Value);

            /* ---------- SlimesHair  <Pawn, int>  ---------- */
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                slimesHair_keys = SlimesPoints.Keys.ToList();
                slimesHair_values = SlimesPoints.Values.ToList();
            }

            Scribe_Collections.Look(ref slimesHair_keys, "slimesHair_keys", LookMode.Reference);
            Scribe_Collections.Look(ref slimesHair_values, "slimesHair_values", LookMode.Value);

            // 重建字典
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // originalSlimeQueues
                originalSlimeQueues = new Dictionary<Pawn, Queue<Pawn>>();
                if (originalSlimeQueues_keys != null && originalSlimeQueues_values != null &&
                    originalSlimeQueues_keys.Count == originalSlimeQueues_values.Count)
                {
                    for (int i = 0; i < originalSlimeQueues_keys.Count; i++)
                    {
                        var key = originalSlimeQueues_keys[i];
                        var valueList = originalSlimeQueues_values[i];

                        if (key != null && !key.Destroyed)
                        {
                            originalSlimeQueues[key] = new Queue<Pawn>(valueList ?? new List<Pawn>());
                        }
                    }
                }

                // SlimesPoints
                SlimesPoints = new Dictionary<Pawn, int>();
                if (slimesPoints_keys != null && slimesPoints_values != null &&
                    slimesPoints_keys.Count == slimesPoints_values.Count)
                {
                    for (int i = 0; i < slimesPoints_keys.Count; i++)
                    {
                        var key = slimesPoints_keys[i];
                        var value = slimesPoints_values[i];

                        if (key != null && !key.Destroyed)
                        {
                            SlimesPoints[key] = value;
                        }
                    }
                }

                // SlimesHair
                SlimesHair = new Dictionary<Pawn, int>();
                if (slimesHair_keys != null && slimesHair_values != null &&
                    slimesHair_keys.Count == slimesHair_values.Count)
                {
                    for (int i = 0; i < slimesHair_keys.Count; i++)
                    {
                        var key = slimesHair_keys[i];
                        var value = slimesHair_values[i];

                        if (key != null && !key.Destroyed)
                        {
                            SlimesHair[key] = value;
                        }
                    }
                }

                // 清理无效引用
                CleanInvalidReferences();
            }
        }

        // 清理无效引用
        private void CleanInvalidReferences()
        {
            // 清理原始史莱姆
            var invalidSlimes = new List<Pawn>();
            foreach (var slime in originalSlimeQueues.Keys)
            {
                if (slime == null || slime.Destroyed)
                {
                    invalidSlimes.Add(slime);
                }
            }
            foreach (var slime in invalidSlimes)
            {
                originalSlimeQueues.Remove(slime);
                SlimesPoints.Remove(slime);
            }

            // 清理队列中的无效Pawn
            foreach (var queue in originalSlimeQueues.Values)
            {
                var validPawns = new List<Pawn>();
                foreach (var pawn in queue)
                {
                    if (pawn != null && !pawn.Destroyed)
                    {
                        validPawns.Add(pawn);
                    }
                }
                queue.Clear();
                foreach (var pawn in validPawns) queue.Enqueue(pawn);
            }

            // 清理SlimesPoints中的无效引用
            invalidSlimes = SlimesPoints.Keys.Where(slime => slime == null || slime.Destroyed).ToList();
            foreach (var slime in invalidSlimes)
            {
                SlimesPoints.Remove(slime);
            }

            invalidSlimes = SlimesHair.Keys.Where(slime => slime == null || slime.Destroyed).ToList();
            foreach (var slime in invalidSlimes)
            {
                SlimesHair.Remove(slime);
            }
        }
    }
}