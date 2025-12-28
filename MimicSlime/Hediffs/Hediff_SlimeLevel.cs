using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace MimicSlime
{
    public class Hediff_SlimeLevel : Hediff_Level
    {
        public bool updateNotify = false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.SetLevelTo(1);
            this.Severity = 0.1f;
            base.PostAdd(dinfo);
        }

        public void UpdateLevel(int points)
        {
            if (points <= 5) SetLevelTo(1);
            else if (points <= 20) SetLevelTo(2);
            else if (points <= 30) 
            {
                if (!updateNotify && pawn.Spawned)
                {
                    updateNotify = true;

                    MimicSlimeUtility.AddAbility(MimicSlimeDefOf.ParasiteAbility, this.pawn);
                    if (pawn.Faction.IsPlayer)
                    {
                        Find.LetterStack.ReceiveLetter(
                            "SlimeLevelUpTitle".Translate(),
                            "SlimeLevelUpDesc".Translate(
                                MimicSlimeUtility.GetOriginalSlime(this.pawn).NameShortColored
                            ),
                            LetterDefOf.PositiveEvent,
                            new LookTargets(this.pawn)
                        );
                    }
                }

                SetLevelTo(3); 
            }
            else if (points <= 50) SetLevelTo(4);
            else SetLevelTo(5);
        }

        public override void SetLevelTo(int targetLevel)
        {
            if (targetLevel != level)
            {
                base.ChangeLevel(targetLevel - level);
            }
            Severity = level;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: +1 slime point",
                    action = () =>
                    {
                        var mgr = MimicSlimeGlobalManager.Instance;
                        if (mgr == null) return;
                        mgr.AddEvoluPoints(pawn, 1);
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEV: -1 slime point",
                    action = () =>
                    {
                        var mgr = MimicSlimeGlobalManager.Instance;
                        if (mgr == null) return;
                        mgr.AddEvoluPoints(pawn, -1);
                    }
                };
            }
            else yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref updateNotify, "updateNotify", false);
        }
    }
}
