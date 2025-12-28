using MimicSlime;
using RimWorld;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

public static class MimicSlimeUtility
{
    
    public static bool IsValidTarget(Pawn target)
    {
        return target != null &&
               !target.Destroyed &&
               !target.Discarded &&
               (target.Downed || target.InMentalState);
    }

    public static void AddToFaction(Pawn parent, Pawn pawn)
    {
        if (pawn.Faction != parent.Faction)
        {
            pawn.SetFaction(parent.Faction);
        }
    }

    public static void AddAbility(AbilityDef def, Pawn pawn)
    {
        pawn.abilities.GainAbility(def);
    }

    public static void RemovePawn(Pawn pawn)
    {
        if (pawn == null) return;

        // 卸下所有植入体,剥光装备
        if (pawn.Spawned)
        {
            StripPawn(pawn);
        }

        // 移除角色
        if (pawn.Spawned)
        {
            pawn.DeSpawn();
        }

        AddPawnToWorld(pawn);

        // 完全治愈
        ResurrectionThenCureAll(pawn);
    }

    public static void StripPawn(Pawn pawn)
    {
        if (pawn == null) return;

        // 剥光
        pawn.Strip();

        // 移除所有植入体
        RemoveImplantsAndDrop(pawn);
    }

    private static void RemoveImplantsAndDrop(Pawn pawn)
    {
        if (pawn?.health?.hediffSet?.hediffs == null) return;

        var implants = pawn.health.hediffSet.hediffs
            .OfType<Hediff_Implant>()
            .ToList();

        foreach (var implant in implants)
        {
            // 获取植入体对应的物品定义
            ThingDef implantDef = GetImplantThingDef(implant);

            if (implantDef != null)
            {
                // 创建物品
                Thing implantItem = ThingMaker.MakeThing(implantDef);
                if (pawn.Spawned)
                {
                    GenPlace.TryPlaceThing(
                        implantItem,
                        pawn.Position,
                        pawn.Map,
                        ThingPlaceMode.Near
                    );
                }
            }

            // 移除健康状态
            pawn.health.RemoveHediff(implant);
        }
    }

    private static ThingDef GetImplantThingDef(Hediff implant)
    {
        // 优先使用植入体定义中指定的掉落物品
        if (implant.def.spawnThingOnRemoved != null)
        {
            return implant.def.spawnThingOnRemoved;
        }

        // 身体部位获取定义
        if (implant.Part?.def?.spawnThingOnRemoved != null)
        {
            return implant.Part.def.spawnThingOnRemoved;
        }

        // 默认返回植入体同名的物品定义
        return DefDatabase<ThingDef>.GetNamedSilentFail(implant.def.defName);
    }

    // 创建史莱姆Pawn
    public static Pawn CreateSlimePawn(PawnKindDef slimeKind, Faction faction ,float? age_Bio = null, float? age_Chr = null, bool forceNoGear = true)
    {
        // 创建史莱姆Pawn
        PawnGenerationRequest request = new PawnGenerationRequest(
            slimeKind,
            faction,
            PawnGenerationContext.NonPlayer,
            -1,
            forceGenerateNewPawn: true,
            allowDead: false,
            allowDowned: false,
            canGeneratePawnRelations: false,
            mustBeCapableOfViolence: false,
            allowFood: false,
            fixedBiologicalAge: age_Bio,
            fixedChronologicalAge: age_Chr,
            forceNoGear: forceNoGear
        );

        Pawn slime = PawnGenerator.GeneratePawn(request);

        if (slime.def == MimicSlimeDefOf.MimicSlimeRace)
        {
            MimicSlimeGlobalManager.Instance?.RegisterOriginalSlime(slime);
        }

        return slime;
    }

    public static Pawn GetOriginalSlime(Pawn pawn)
    {
        return MimicSlimeGlobalManager.Instance.GetOriginalSlimeFromPawn(pawn);
    }

    public static bool IsOriginalSlime(Pawn slime)
    {
        return slime.def == MimicSlimeDefOf.MimicSlimeRace;
    }

    public static bool IsSlime(Pawn pawn)
    {
        if (IsOriginalSlime(pawn))
        {
            return true;
        }
        if (pawn.health.hediffSet.HasHediff(MimicSlimeDefOf.ActuallySlime))
        {
            return true;
        }
        if (GetOriginalSlime(pawn) != null)
        {
            pawn.health.AddHediff(MimicSlimeDefOf.ActuallySlime);
            return true;
        }
        return false;
    }

    public static bool IsDigesting(Pawn pawn)
    {
        Ability_SlimeAbsorb absorbAbility = pawn.abilities?.AllAbilitiesForReading
            .OfType<Ability_SlimeAbsorb>()
            .FirstOrDefault();

        if (absorbAbility == null)
        {
            AddAbility(MimicSlimeDefOf.SlimeAbsorbAbility, pawn);
            absorbAbility = pawn.abilities?.AllAbilitiesForReading
                .OfType<Ability_SlimeAbsorb>()
                .FirstOrDefault();
        }

        return absorbAbility.OnCooldown;
    }

    public static void ResurrectionThenCureAll(Pawn pawn)
    {
        if (pawn.Dead)
        {
            ResurrectionUtility.TryResurrect(pawn);
        }

        // 治疗所有符合条件的Hediff
        List<Hediff> hediffsCopy = pawn.health.hediffSet.hediffs.ToList();
        foreach (Hediff hediff in hediffsCopy)
        {
            if (JudgeHediff(hediff)) HealthUtility.Cure(hediff);
        }
    }

    private static bool JudgeHediff(Hediff hediff)
    {
        if (hediff.def.isBad || 
            hediff.def.isInfection || 
            hediff is Hediff_MissingPart ||
            HookForShouldRemoveHediff().Contains<HediffDef>(hediff.def)) 
            return true;
        return false;
    }

    private static List<HediffDef> HookForShouldRemoveHediff()
    {
        List<HediffDef> defs = new List<HediffDef>
        {
            HediffDefOf.Anesthetic,
            HediffDefOf.Pregnant
        };
        return defs;
    }

    public static IEnumerable<Pawn> AllPawnsEverywhere()
    {
        if (Find.CurrentMap != null)
            foreach (var p in Find.CurrentMap.mapPawns.AllPawns)
                yield return p;

        foreach (var caravan in Find.WorldObjects.Caravans)
            foreach (var p in caravan.PawnsListForReading)
                yield return p;

        foreach (var p in Find.WorldPawns.AllPawnsAliveOrDead)
            yield return p;
    }

    public static void ChangeData(Pawn from, Pawn to)
    {
        SlimeData fromData = new SlimeData();
        fromData.CaptureFromPawn(from);

        RemoveIdeoRole(from);

        fromData.ApplyToPawn(to);
    }

    public static void RemoveIdeoRole(Pawn pawn)
    {
        if (pawn?.Ideo == null) return;
        var role = pawn.Ideo.GetRole(pawn);
        if (role == null) return;

        switch (role)
        {
            case Precept_RoleSingle single:
                single.chosenPawn = null;
                break;
            case Precept_RoleMulti multi:
                multi.chosenPawns.RemoveAll(r => r.pawn == pawn);
                break;
        }
    }

    public static void ChangeHair(Pawn pawn, int hair, bool toNext = false)
    {
        var list = MimicSlimeHairUtility.AllSlimeHairs as List<HairDef>;
        if (list.NullOrEmpty()) return;

        if (hair < 0)
        {
            hair = Rand.Range(0, list.Count);
        }

        HairDef target = toNext
            ? MimicSlimeHairUtility.NextHair(pawn.story.hairDef)
            : (hair < 0 || hair >= list.Count ? list[0] : list[hair]);

        pawn.story.hairDef = target;
        MimicSlimeGlobalManager.Instance?.ChangeHair(pawn, list.IndexOf(target));
    }

    public static Pawn SpawnSlimeKing(Map map, IntVec3 spawnPoint, Faction faction)
    {
        IntVec3 dropPos = spawnPoint;
        dropPos.z = map.Size.z - 1;
        PawnKindDef slimeKind = DefDatabase<PawnKindDef>.GetNamed("SlimeKing");
        Pawn boss = PawnGenerator.GeneratePawn(slimeKind, faction);
        GenSpawn.Spawn(boss, dropPos, map);
        PawnFlyer flyer = PawnFlyer.MakeFlyer(
            ThingDef.Named("SlimeKing_JumpWithDamageFlyer"),
            boss,
            spawnPoint,
            null,
            null
        );

        if (flyer != null)
        {
            if (!spawnPoint.InBounds(map))
            {
                Log.Warning("Invalid spawn position for slime king flyer. Adjusting to map center.");
                spawnPoint = map.Center;
            }

            GenSpawn.Spawn(flyer, spawnPoint, map);
        }
        else
        {
            GenSpawn.Spawn(boss, spawnPoint, map);
        }

        Messages.Message("SlimeKingSpawned".Translate(boss.LabelCap), boss, MessageTypeDefOf.ThreatBig);
        return boss;
    }

    public static bool CanAddTarit(TraitDef def)
    {
        if (HookForShouldRemoveTarit().Contains<TraitDef>(def))
        {
            return false;
        }
        return true;
    }

    private static List<TraitDef> HookForShouldRemoveTarit()
    {
        List<TraitDef> defs = new List<TraitDef>
        {
            TraitDefOf.CreepyBreathing,
            TraitDefOf.Nudist,
            TraitDefOf.Transhumanist,
            TraitDefOf.Pyromaniac,
            TraitDefOf.DislikesMen,
            TraitDefOf.DislikesWomen,
            TraitDefOf.DrugDesire,
            TraitDefOf.Asexual,
            TraitDefOf.Gay,
            TraitDefOf.AnnoyingVoice,
            DefDatabase<TraitDef>.GetNamed("Beauty"),
            DefDatabase<TraitDef>.GetNamed("Immunity"),
            DefDatabase<TraitDef>.GetNamed("PsychicSensitivity"),
            DefDatabase<TraitDef>.GetNamed("Delicate")
        };
        return defs;
    }

    public static void AddAbsorbedThought(Pawn victim)
    {
        foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
        {
            if (!PawnUtility.ShouldGetThoughtAbout(p, victim) || victim.Faction != Faction.OfPlayer || p.needs?.mood?.thoughts?.memories == null) continue;

            Thought_Memory thought;
            PawnRelationDef mostImportantRelation = p.GetMostImportantRelation(victim);
            if (mostImportantRelation != null && mostImportantRelation.familyByBloodRelation)
            {
                if (p.ideo.Ideo.HasPrecept(MimicSlimeDefOf.Absorb_Preferred))
                {
                    thought = ThoughtMaker.MakeThought(MimicSlimeDefOf.KinAbsorbedByDeliria, 1);
                }
                else
                {
                    thought = ThoughtMaker.MakeThought(MimicSlimeDefOf.KinAbsorbedByDeliria, 0);
                }
                p.needs.mood.thoughts.memories.TryGainMemory(thought, victim);
            }
            else
            {
                if (p.ideo.Ideo.HasPrecept(MimicSlimeDefOf.Absorb_Preferred))
                {
                    thought = ThoughtMaker.MakeThought(MimicSlimeDefOf.ColonistAbsorbedByDeliria, 1);
                }
                else
                {
                    thought = ThoughtMaker.MakeThought(MimicSlimeDefOf.ColonistAbsorbedByDeliria, 0);
                }
                p.needs.mood.thoughts.memories.TryGainMemory(thought, victim);
            }
        }
    }

    public static void AddPawnToWorld(Pawn pawn)
    {
        Faction faction = pawn.Faction;
        if (!Find.WorldPawns.Contains(pawn) && !pawn.Spawned)
        {
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
        }
        if (faction != null && faction != pawn.Faction)
        {
            pawn.SetFaction(faction);
        }
    }

    public static List<T> CopyList<T>(this List<T> list)
    {
        if (list == null)
        {
            return new List<T>();
        }
        return list.ListFullCopy<T>();
    }

    public static void CleanupList<T>(this List<T> list, Predicate<T> predicate = null)
    {
        if (list == null)
        {
            return;
        }
        if (predicate == null)
        {
            predicate = ((T x) => x.IsNullValue<T>());
        }
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
            }
        }
    }

    public static bool IsNullValue<T>(this T obj)
    {
        bool flag = obj == null;
        if (!flag)
        {
            FieldInfo field = typeof(T).GetField("def");
            if (field != null && field.GetValue(obj) == null)
            {
                flag = true;
            }
        }
        return flag;
    }

    public static Dictionary<K, V> CopyDict<K, V>(this Dictionary<K, V> dict)
    {
        if (dict == null)
        {
            return new Dictionary<K, V>();
        }
        return dict.ToDictionary((KeyValuePair<K, V> x) => x.Key, (KeyValuePair<K, V> x) => x.Value);
    }

    public static void CleanupDict<K, V>(this Dictionary<K, V> dict, Predicate<KeyValuePair<K, V>> predicate = null)
    {
        if (dict == null)
        {
            return;
        }
        if (predicate == null)
        {
            predicate = ((KeyValuePair<K, V> x) => x.Key.IsNullValue<K>() || x.Value.IsNullValue<V>());
        }
        dict.RemoveAll(predicate);
    }

    public static void NotDeath(Pawn pawn, bool noThoughts = true)
    {
        try
        {
            if (noThoughts)
            {
                ActuallySlimeHarmonyPatch_AppendThoughts_ForHumanlike.pawnNotDead = pawn;
                ActuallySlimeHarmonyPatch_AppendThoughts_Relations.pawnNotDead = pawn;
            }
            ActuallySlimeHarmonyPatch_Ideo_Notify_MemberDied.pawnNotDead = pawn;
            ActuallySlimeHarmonyPatch_Ideo_Notify_MemberCorpseDestroyed.pawnNotDead = pawn;
            ActuallySlimeHarmonyPatch_NotifyPlayerOfKilled.pawnNotDead = pawn;
            ActuallySlimeHarmonyPatch_Royalty_Notify_PawnKilled.pawnNotDead = pawn;
        }
        catch (Exception ex)
        {
            Log.Error($"Error in HandleSpecialDeath: {ex}");
        }
    }
}
