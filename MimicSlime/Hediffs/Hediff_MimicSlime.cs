using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class Hediff_MimicSlime : HediffWithComps
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (pawn.def == MimicSlimeDefOf.MimicSlimeRace && pawn.IsColonistPlayerControlled)
            {
                var manager = MimicSlimeGlobalManager.Instance;
                yield return new Command_Action
                {
                    defaultLabel = "MimicSlime_MemoryLabel".Translate(),
                    defaultDesc = "MimicSlime_MemoryDesc".Translate(),
                    icon = TexCommand.ForbidOff,
                    action = () => Find.WindowStack.Add(new Dialog_Mimicry(pawn, manager, onlyread: true))
                };
            }

            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                if (apparel.TryGetComp<CompTraitsOnSpawn>(out CompTraitsOnSpawn comp))
                {
                    comp.Notify_KilledPawn(pawn);
                }
            }
        }
    }
}
