using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class CompProperties_TraitsOnSpawn : CompProperties
    {
        public List<WeaponTraitDef> traitsToAdd;
        public bool randomizeOrder = true;

        public CompProperties_TraitsOnSpawn()
        {
            compClass = typeof(CompTraitsOnSpawn);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;

            if (traitsToAdd.NullOrEmpty())
                yield return "traitsToAdd is empty!";
        }
    }

    public class CompTraitsOnSpawn : ThingComp
    {
        private List<WeaponTraitDef> traits;
        private int lastKillTick = -1;
        public List<WeaponTraitDef> TraitsListForReading => traits;
        public int TicksSinceLastKill
        {
            get
            {
                if (this.lastKillTick < 0)
                {
                    return 0;
                }
                return Find.TickManager.TicksAbs - this.lastKillTick;
            }
        }

        public override void PostPostMake()
        {
            traits = new List<WeaponTraitDef>();
            var p = (CompProperties_TraitsOnSpawn)props;
            if (!p.traitsToAdd.NullOrEmpty())
            {
                traits.AddRange(p.traitsToAdd);
                if (p.randomizeOrder) traits.Shuffle();
            }
            else
            {
                Log.Error("CompTraitsOnSpawn : traitsToAdd is empty!");
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (!ModLister.CheckRoyalty("Persona weapon"))
            {
                return;
            }
            base.Notify_Equipped(pawn);
            if (!this.traits.NullOrEmpty<WeaponTraitDef>())
            {
                for (int i = 0; i < this.traits.Count; i++)
                {
                    this.traits[i].Worker.Notify_Equipped(pawn);
                }
            }
        }

        public void Notify_EquipmentLost(Pawn pawn)
        {
            if (!this.traits.NullOrEmpty<WeaponTraitDef>())
            {
                for (int i = 0; i < this.traits.Count; i++)
                {
                    this.traits[i].Worker.Notify_EquipmentLost(pawn);
                }
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            this.lastKillTick = Find.TickManager.TicksAbs;
            if (!this.traits.NullOrEmpty<WeaponTraitDef>())
            {
                for (int i = 0; i < this.traits.Count; i++)
                {
                    this.traits[i].Worker.Notify_KilledPawn(pawn);
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if (!traits.NullOrEmpty())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Stat_Thing_PersonaWeaponTrait_Desc".Translate());
                sb.AppendLine();
                for (int i = 0; i < traits.Count; i++)
                {
                    sb.AppendLine(traits[i].LabelCap + ": " + traits[i].description);
                    if (i < traits.Count - 1) sb.AppendLine();
                }
                yield return new StatDrawEntry(
                    parent.def.IsMeleeWeapon ? StatCategoryDefOf.Weapon_Melee
                                           : StatCategoryDefOf.Weapon_Ranged,
                    "Stat_Thing_PersonaWeaponTrait_Label".Translate(),
                    (from t in traits select t.label).ToCommaList(false, false).CapitalizeFirst(),
                    sb.ToString(), 1104, null, null, false, false);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref traits, "traits", LookMode.Def);
            Scribe_Values.Look(ref lastKillTick, "lastKillTick", -1);
        }
    }
}