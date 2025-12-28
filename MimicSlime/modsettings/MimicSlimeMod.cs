using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MimicSlime
{
    [StaticConstructorOnStartup]
    public class MimicSlimeMod : Mod
    {
        public static Harmony harmony;

        private MimicSlimeModSettings settings;

        static MimicSlimeMod()
        {
            var harmony = new Harmony("com.LF.mimicslime");
            harmony.PatchAll();
            Log.Message("MimicSlime: Harmony patches applied");
        }

        public MimicSlimeMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MimicSlimeModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            ls.CheckboxLabeled("CognitionFilter".Translate(),
                ref MimicSlimeModSettings.CognitionFilter,
                "CognitionFilterTip".Translate());

            ls.CheckboxLabeled("SpawnGiantSlimeInSlimeRain".Translate(),
                ref MimicSlimeModSettings.SpawnGiantSlimeInSlimeRain);

            ls.CheckboxLabeled("SpawnSlimeKingInSlimeRain".Translate(),
                ref MimicSlimeModSettings.SpawnSlimeKingInSlimeRain);

            ls.CheckboxLabeled("IgnoreLevel".Translate(),
                ref MimicSlimeModSettings.IgnoreLevel);

            ls.CheckboxLabeled("IgnoreDigest".Translate(),
                ref MimicSlimeModSettings.IgnoreDigest);

            ls.CheckboxLabeled("MeltingLoveSpreadByTime".Translate(),
                ref MimicSlimeModSettings.MeltingLoveSpreadByTime,
                "MeltingLoveSpreadByTimeTip".Translate());

            ls.CheckboxLabeled("GainTrait".Translate(),
                ref MimicSlimeModSettings.GainTrait);

            string GainTraitIntStr = MimicSlimeModSettings.GainTraitInt.ToString();
            ls.Label("GainTraitInt".Translate());
            ls.TextFieldNumeric(ref MimicSlimeModSettings.GainTraitInt, ref GainTraitIntStr, 0, 100);
            MimicSlimeModSettings.GainTraitInt = Mathf.Clamp(MimicSlimeModSettings.GainTraitInt, 0, 100);

            string ActuallySlimeChance = MimicSlimeModSettings.ActuallySlimeChance.ToString();
            ls.Label("ActuallySlimeChance".Translate(), tooltip : "ActuallySlimeChanceTip".Translate());
            ls.TextFieldNumeric(ref MimicSlimeModSettings.ActuallySlimeChance, ref ActuallySlimeChance, 0f, 100f);
            MimicSlimeModSettings.ActuallySlimeChance = Mathf.Clamp(MimicSlimeModSettings.ActuallySlimeChance, 0f, 100f);

            string QueueLimStr = MimicSlimeModSettings.QueueStackLimit.ToString();
            ls.Label("QueueStackLimit".Translate());
            ls.TextFieldNumeric(ref MimicSlimeModSettings.QueueStackLimit, ref QueueLimStr, 1, 10);
            MimicSlimeModSettings.QueueStackLimit = Mathf.Clamp(MimicSlimeModSettings.QueueStackLimit, 1, 10);

            string PointDivisorStr = MimicSlimeModSettings.PointDivisor.ToString();
            ls.Label("PointDivisor".Translate());
            ls.TextFieldNumeric(ref MimicSlimeModSettings.PointDivisor, ref PointDivisorStr, 100, 10000);
            MimicSlimeModSettings.PointDivisor = Mathf.Clamp(MimicSlimeModSettings.PointDivisor, 100, 10000);

            string SlimeRebornDays = MimicSlimeModSettings.SlimeRebornDays.ToString();
            ls.Label("SlimeRebornDays".Translate());
            ls.TextFieldNumeric(ref MimicSlimeModSettings.SlimeRebornDays, ref SlimeRebornDays, 1f, 30f);
            MimicSlimeModSettings.SlimeRebornDays = Mathf.Clamp(MimicSlimeModSettings.SlimeRebornDays, 1f, 30f);

            ls.End();
            base.DoSettingsWindowContents(inRect);

            // 每帧比对并触发
            MimicSlimeModSettings.CheckAndFire();
        }

        public override string SettingsCategory()
        {
            return "MimicSlimeMod_Name".Translate();
        }
    }
}
