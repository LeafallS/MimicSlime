using System;
using Verse;

namespace MimicSlime
{
    public class MimicSlimeModSettings : ModSettings
    {
        public static bool CognitionFilter = true;
        public static bool TransformIdeo = false;
        public static bool StripClothing = true;
        public static bool SpawnGiantSlimeInSlimeRain = true;
        public static bool SpawnSlimeKingInSlimeRain = true;
        public static bool IgnoreLevel = false;
        public static bool IgnoreDigest = false;
        public static bool MeltingLoveSpreadByTime = false;
        public static bool SlimeRainAssault = true;
        public static bool GainTrait = true;
        public static int GainTraitInt = 5;
        public static float ActuallySlimeChance = 0.1f;
        public static int QueueStackLimit = 5;
        public static int PointDivisor = 2000;
        public static float SlimeRebornDays = 3f;
        public static int SlimeRainChance = 100;

        // 值改变时触发
        public static event Action<bool> OnCognitionFilterChanged;

        // 内部缓存，用来比对
        private static bool lastValue = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref CognitionFilter, "CognitionFilter", true);
            Scribe_Values.Look(ref TransformIdeo, "TransformIdeo", false);
            Scribe_Values.Look(ref StripClothing, "StripClothing", true);
            Scribe_Values.Look(ref SpawnGiantSlimeInSlimeRain, "SpawnGiantSlimeInSlimeRain", true);
            Scribe_Values.Look(ref SpawnSlimeKingInSlimeRain, "SpawnSlimeKingInSlimeRain", true);
            Scribe_Values.Look(ref IgnoreLevel, "IgnoreLevel", false);
            Scribe_Values.Look(ref IgnoreDigest, "IgnoreDigest", false);
            Scribe_Values.Look(ref MeltingLoveSpreadByTime, "MeltingLoveSpreadByTime", false);
            Scribe_Values.Look(ref SlimeRainAssault, "SlimeRainAssault", true);
            Scribe_Values.Look(ref GainTrait, "GainTrait", true);
            Scribe_Values.Look(ref GainTraitInt, "GainTraitInt", 5);
            Scribe_Values.Look(ref ActuallySlimeChance, "ActuallySlimeChance", 0.1f);
            Scribe_Values.Look(ref QueueStackLimit, "QueueStackLimit", 5);
            Scribe_Values.Look(ref PointDivisor, "PointDivisor", 2000);
            Scribe_Values.Look(ref SlimeRebornDays, "SlimeRebornDays", 3f);
            Scribe_Values.Look(ref SlimeRainChance, "SlimeRainChance", 100);
            // 读档后同步缓存
            lastValue = CognitionFilter;
        }

        public static void CheckAndFire()
        {
            if (CognitionFilter == lastValue) return;
            lastValue = CognitionFilter;
            OnCognitionFilterChanged?.Invoke(CognitionFilter);
        }
    }
}
