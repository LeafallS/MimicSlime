using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace MimicSlime
{
    public class Hediff_SlimeParasite : HediffWithComps
    {
        private bool moderateNotify = false;
        private bool severeNotify = false;
        private Faction faction = null;

        public Faction Faction 
        {
            get
            {
                return faction;
            }
            set
            {
                faction = value;
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            this.Severity = 0.01f;
        }

        public override void PostTickInterval(int delta)
        {
            base.PostTickInterval(delta);
            CheckSeverity();
        }

        public override bool CauseDeathNow()
        {
            return false;
        }

        private void CheckSeverity()
        {
            if (this.CurStageIndex == 2 && !moderateNotify)
            {
                moderateNotify = true;
                Messages.Message(
                    "Hediff_SlimeParasite_Moderate".Translate(pawn.LabelShort),
                    MessageTypeDefOf.SilentInput
                );
            }
            else if (this.CurStageIndex == 3 && !severeNotify)
            {
                severeNotify = true;
                Find.LetterStack.ReceiveLetter(
                    "SlimeParasite_SevereTitle".Translate(),
                    "SlimeParasite_SevereDesc".Translate(
                        pawn.NameShortColored
                    ),
                    LetterDefOf.NegativeEvent,
                    new LookTargets(pawn)
                );
            }
            else if (Severity >= 1f) {
                try
                {
                    // 创建史莱姆Pawn
                    Pawn slimePawn = MimicSlimeUtility.CreateSlimePawn(MimicSlimeDefOf.MimicSlime_Transformed, faction ?? pawn.Faction, 
                        pawn.ageTracker.AgeBiologicalYearsFloat, pawn.ageTracker.AgeChronologicalYearsFloat);

                    // 转换原Pawn
                    MimicSlimeGlobalManager.Instance.AddPawnToQueueNoRemove(slimePawn, pawn);
                    MimicSlimeUtility.AddAbility(MimicSlimeDefOf.SlimeAbsorbAbility, pawn);
                    MimicSlimeUtility.AddAbility(MimicSlimeDefOf.MimicryAbility, pawn);
                    List<Hediff> slimeBodyParts;
                    if (ModsConfig.AnomalyActive)
                    {
                        slimeBodyParts = pawn.health.hediffSet.hediffs.FindAll(h => h.def == DefDatabase<HediffDef>.GetNamed("SlimeStomach") || h.def == DefDatabase<HediffDef>.GetNamed("SlimeTentacle"));
                        foreach (var hediff in slimeBodyParts)
                        {
                            HealthUtility.Cure(hediff);
                        }
                    }
                    slimeBodyParts = pawn.health.hediffSet.hediffs.FindAll(h => h is Hediff_MissingPart);
                    foreach (var hediff in slimeBodyParts)
                    {
                        HealthUtility.Cure(hediff);
                    }

                    MimicSlimeUtility.AddAbsorbedThought(pawn, slimePawn);
                    if (slimePawn.Faction != null)
                    {
                        this.faction = slimePawn.Faction;
                    }
                    if (pawn.Faction != faction) MimicSlimeUtility.AddToFaction(slimePawn, pawn);
                    AddTrait(slimePawn);
                    AddSkill(slimePawn);
                    slimePawn.Name = pawn.Name;

                    Messages.Message(
                        "Hediff_SlimeParasite_Complete".Translate(pawn.LabelShort),
                        new LookTargets(pawn),
                        MessageTypeDefOf.PositiveEvent
                    );

                    pawn.health.RemoveHediff(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"Slime Parasite transform failed: {ex}\n{ex.StackTrace}");
                }
            }
        }

        private void AddTrait(Pawn slime)
        {
            List<Trait> traits = new List<Trait>();
            if (pawn.story?.traits != null)
            {
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    if (trait.sourceGene == null && MimicSlimeUtility.CanAddTarit(trait.def))
                        traits.Add(new Trait(trait.def, trait.Degree, false));
                }
            }

            if (traits != null && slime.story?.traits != null)
            {
                var toRemove = slime.story.traits.allTraits.Where(t => t.sourceGene == null && t.def != TraitDefOf.Bisexual).ToList();
                foreach (var t in toRemove) slime.story.traits.RemoveTrait(t);
                foreach (var trait in traits) slime.story.traits.GainTrait(new Trait(trait.def, trait.Degree, false));
            }
        }

        private void AddSkill(Pawn slime)
        {
            Dictionary<SkillDef, int> dic = new Dictionary<SkillDef, int>();
            foreach (var backStory in pawn.story.AllBackstories)
            {
                foreach (var skillGain in backStory.skillGains)
                {
                    SkillDef skill = skillGain.skill;
                    int amount = skillGain.amount;
                    if (dic.ContainsKey(skill))
                    {
                        dic[skill] += amount;
                    }
                    else
                    {
                        dic.Add(skill, amount);
                    }
                }
            }

            List<SkillRecord> skills = new List<SkillRecord>();
            if (pawn.skills != null)
            {
                foreach (SkillRecord skill in pawn.skills.skills)
                {
                    skills.Add(new SkillRecord
                    {
                        def = skill.def,
                        levelInt = skill.levelInt,
                        passion = skill.passion,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight
                    });
                }
            }

            slime.skills.skills.Clear();
            foreach (var skill in skills)
                slime.skills.skills.Add(new SkillRecord(slime, skill.def)
                {
                    levelInt = skill.levelInt,
                    passion = skill.passion,
                    xpSinceLastLevel = skill.xpSinceLastLevel,
                    xpSinceMidnight = skill.xpSinceMidnight
                });
            foreach (var kyp in dic)
            {
                slime.skills.GetSkill(kyp.Key).Level -= kyp.Value;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref moderateNotify, "moderateNotify", false);
            Scribe_Values.Look(ref severeNotify, "severeNotify", false);
            Scribe_References.Look(ref faction, "faction");
        }
    }
}
