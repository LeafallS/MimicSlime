using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class SlimeData : IExposable
    {
        #region 字段
        // 基础
        private Pawn_NeedsTracker needs;

        // 皇室称号
        private List<RoyalTitle> royalTitles;
        private Dictionary<Faction, int> favor;
        private Dictionary<Faction, Pawn> heirs;
        private List<Thing> bondedThings;
        private List<FactionPermit> factionPermits;

        // 意识形态
        private Precept_RoleMulti precept_RoleMulti;
        private Precept_RoleSingle precept_RoleSingle;

        // 灵能数据
        private Hediff savedPsylinkSource;
        private int? psylinkLevel;
        private List<AbilityDef> abilities;
        private float currentEntropy;
        private float currentPsyfocus;

        // 原始Pawn引用
        private Pawn originalPawn;
        #endregion

        public SlimeData() { }

        #region 捕获
        public void CaptureFromPawn(Pawn pawn)
        {
            originalPawn = pawn;

            needs = new Pawn_NeedsTracker();
            foreach (var need in pawn.needs.AllNeeds)
            {
                if (needs.TryGetNeed(need.def) != null)
                {
                    needs.TryGetNeed(need.def).CurLevel = need.CurLevel;
                }
            }

            // 皇室
            if (ModsConfig.RoyaltyActive && pawn.royalty != null)
            {
                royalTitles = new List<RoyalTitle>();
                foreach (var t in pawn.royalty.AllTitlesForReading)
                {
                    royalTitles.Add(new RoyalTitle
                    {
                        def = t.def,
                        faction = t.faction,
                        receivedTick = t.receivedTick,
                        conceited = t.conceited,
                        pawn = null // 稍后恢复
                    });
                }

                favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>().CopyDict();
                heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>().CopyDict();

                bondedThings = new List<Thing>();
                foreach (var m in Find.Maps)
                    foreach (var t in m.listerThings.AllThings)
                    {
                        var comp = t.TryGetComp<CompBladelinkWeapon>();
                        if (comp?.CodedPawn == pawn) bondedThings.Add(t);
                    }

                factionPermits = new List<FactionPermit>(pawn.royalty.AllFactionPermits);
            }

            // 意识形态
            if (ModsConfig.IdeologyActive && pawn.ideo != null)
            {
                var role = pawn.ideo.Ideo?.GetRole(pawn);
                precept_RoleMulti = role as Precept_RoleMulti;
                precept_RoleSingle = role as Precept_RoleSingle;
            }

            // 灵能
            if (ModsConfig.RoyaltyActive)
            {
                if (pawn.HasPsylink)
                {
                    psylinkLevel = pawn.GetPsylinkLevel();
                    savedPsylinkSource = pawn.GetMainPsylinkSource();
                }

                abilities = new List<AbilityDef>();
                abilities = pawn.abilities.abilities.Select(a => a.def).ToList().FindAll(a => a.IsPsycast);

                var pe = pawn.psychicEntropy;
                if (pe != null)
                {
                    currentEntropy = Traverse.Create(pe).Field("currentEntropy").GetValue<float>();
                    currentPsyfocus = Traverse.Create(pe).Field("currentPsyfocus").GetValue<float>();
                }
            }
        }
        #endregion

        #region 应用
        public void ApplyToPawn(Pawn pawn, bool basic = true, bool title = true, bool psy = true, bool ideo = true)
        {
            if (basic)
            {
                foreach (var need in needs.AllNeeds)
                {
                    if (pawn.needs.TryGetNeed(need.def) != null)
                    {
                        pawn.needs.TryGetNeed(need.def).CurLevel = need.CurLevel;
                    }
                }
            }

            // 皇室
            if (ModsConfig.RoyaltyActive)
            {
                if (title)
                {
                    ApplyTitle(pawn);
                }

                if (psy)
                {
                    ApplyPsy(pawn);
                }
            }

            // 意识形态
            if (ModsConfig.IdeologyActive && ideo)
            {
                ApplyIdeo(pawn);
            }

            // 通知刷新
            pawn.Notify_DisabledWorkTypesChanged();
            pawn.needs.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
        }

        private void ApplyTitle(Pawn pawn)
        {
            if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn);

            if (royalTitles != null)
            {
                var titles = new List<RoyalTitle>();
                foreach (var t in royalTitles)
                    titles.Add(new RoyalTitle
                    {
                        def = t.def,
                        faction = t.faction,
                        receivedTick = t.receivedTick,
                        conceited = t.conceited,
                        pawn = pawn // 恢复引用
                    });
                Traverse.Create(pawn.royalty).Field("titles").SetValue(titles);
            }
            if (favor != null) Traverse.Create(pawn.royalty).Field("favor").SetValue(favor);
            if (heirs != null) Traverse.Create(pawn.royalty).Field("heirs").SetValue(heirs);
            if (factionPermits != null) Traverse.Create(pawn.royalty).Field("factionPermits").SetValue(factionPermits.CopyList<FactionPermit>());

            if (bondedThings != null)
                foreach (var t in bondedThings)
                    t.TryGetComp<CompBladelinkWeapon>()?.CodeFor(pawn);
        }

        private void ApplyIdeo(Pawn pawn)
        {
            if (pawn.ideo == null) pawn.ideo = new Pawn_IdeoTracker(pawn);

            // 多重角色去重
            if (precept_RoleMulti != null)
            {
                if (precept_RoleMulti.chosenPawns == null)
                    precept_RoleMulti.chosenPawns = new List<IdeoRoleInstance>();
                if (!precept_RoleMulti.chosenPawns.Any(inst => inst.pawn == pawn))
                    precept_RoleMulti.chosenPawns.Add(new IdeoRoleInstance(precept_RoleMulti) { pawn = pawn });
                precept_RoleMulti.FillOrUpdateAbilities();
            }
            if (precept_RoleSingle != null)
            {
                if (precept_RoleSingle.chosenPawn?.pawn == null || precept_RoleSingle.chosenPawn.pawn.Dead)
                {
                    precept_RoleSingle.chosenPawn = new IdeoRoleInstance(precept_RoleSingle) { pawn = pawn };
                    precept_RoleSingle.FillOrUpdateAbilities();
                }
            }
        }

        private void ApplyPsy(Pawn pawn)
        {
            // 灵能
            if (psylinkLevel.HasValue)
            {
                int targetLvl = psylinkLevel.Value;
                Hediff_Psylink psylink = pawn.GetMainPsylinkSource();

                // 没有启灵神经就新建
                if (psylink == null)
                {
                    BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
                    if (brain != null)
                    {
                        psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, brain) as Hediff_Psylink;
                        pawn.health.AddHediff(psylink, brain, null, null);
                    }
                }

                // 升级
                if (psylink != null)
                {
                    int oldLvl = psylink.level;
                    psylink.level = targetLvl;
                }

                List<AbilityDef> psyAbilityCahce = pawn.abilities.abilities.Select(a => a.def).ToList();
                foreach (AbilityDef def in psyAbilityCahce)
                {
                    if (def.IsPsycast)
                    {
                        pawn.abilities.RemoveAbility(def);
                    }
                }
            }

            if (abilities != null)
            {
                foreach (AbilityDef def in abilities)
                {
                    if (!pawn.abilities.abilities.Any(a => a.def == def))
                        pawn.abilities.GainAbility(def);
                }
            }

            var pe = pawn.psychicEntropy;
            if (pe != null)
            {
                Traverse.Create(pe).Field("currentEntropy").SetValue(currentEntropy);
                Traverse.Create(pe).Field("currentPsyfocus").SetValue(currentPsyfocus);
            }
        }
        #endregion

        #region 序列化
        public void ExposeData()
        {
            // 皇室称号
            Scribe_Collections.Look(ref royalTitles, "royalTitles", LookMode.Deep);
            List<Faction> favorKeys = null;
            List<int> favorValues = null;
            if (Scribe.mode == LoadSaveMode.Saving && favor != null)
            {
                favorKeys = favor.Keys.ToList();
                favorValues = favor.Values.ToList();
            }

            Scribe_Collections.Look(ref favorKeys, "favorKeys", LookMode.Reference);
            Scribe_Collections.Look(ref favorValues, "favorValues", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                favor = new Dictionary<Faction, int>();
                if (favorKeys != null && favorValues != null && favorKeys.Count == favorValues.Count)
                {
                    for (int i = 0; i < favorKeys.Count; i++)
                    {
                        favor[favorKeys[i]] = favorValues[i];
                    }
                }
            }

            List<Faction> heirsKeys = null;
            List<Pawn> heirsValues = null;
            if (Scribe.mode == LoadSaveMode.Saving && heirs != null)
            {
                heirsKeys = heirs.Keys.ToList();
                heirsValues = heirs.Values.ToList();
            }

            Scribe_Collections.Look(ref heirsKeys, "heirsKeys", LookMode.Reference);
            Scribe_Collections.Look(ref heirsValues, "heirsValues", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                heirs = new Dictionary<Faction, Pawn>();
                if (heirsKeys != null && heirsValues != null && heirsKeys.Count == heirsValues.Count)
                {
                    for (int i = 0; i < heirsKeys.Count; i++)
                    {
                        heirs[heirsKeys[i]] = heirsValues[i];
                    }
                }
            }

            Scribe_Collections.Look(ref bondedThings, "bondedThings", LookMode.Reference);
            Scribe_Collections.Look(ref factionPermits, "factionPermits", LookMode.Deep);

            // 意识形态
            Scribe_References.Look(ref precept_RoleMulti, "precept_RoleMulti");
            Scribe_References.Look(ref precept_RoleSingle, "precept_RoleSingle");

            // 灵能数据
            Scribe_Deep.Look(ref savedPsylinkSource, "savedPsylinkSource");
            Scribe_Values.Look(ref psylinkLevel, "psylinkLevel");
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Def);
            Scribe_Values.Look(ref currentEntropy, "currentEntropy");
            Scribe_Values.Look(ref currentPsyfocus, "currentPsyfocus");

            // 原始Pawn引用
            Scribe_References.Look(ref originalPawn, "originalPawn");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                royalTitles?.RemoveAll(t => t == null || t.def == null || t.faction == null);

                if (favor != null)
                {
                    var keysToRemove = favor.Keys.Where(k => k == null).ToList();
                    foreach (var key in keysToRemove)
                    {
                        favor.Remove(key);
                    }
                }

                if (heirs != null)
                {
                    var keysToRemove = heirs.Keys.Where(k => k == null || heirs[k] == null).ToList();
                    foreach (var key in keysToRemove)
                    {
                        heirs.Remove(key);
                    }
                }

                bondedThings?.RemoveAll(t => t == null);
                factionPermits?.RemoveAll(p => p == null);

                abilities?.RemoveAll(a => a == null);
            }
        }
        #endregion

        #region 辅助
        public bool HasData => originalPawn != null;
        public Pawn OriginalPawn => originalPawn;
        #endregion
    }
}