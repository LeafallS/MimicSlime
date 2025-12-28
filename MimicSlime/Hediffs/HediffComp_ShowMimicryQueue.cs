using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MimicSlime
{
    public class HediffComp_ShowMimicryQueue : HediffComp
    {
        public HediffCompProperties_ShowMimicryQueue Props => (HediffCompProperties_ShowMimicryQueue)props;

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (Pawn.IsColonistPlayerControlled)
            {
                if (Pawn.def == MimicSlimeDefOf.MimicSlimeRace ||
                    (parent.def == MimicSlimeDefOf.ActuallySlime && Pawn.abilities.GetAbility(MimicSlimeDefOf.MimicryAbility) != null))
                {
                    var manager = MimicSlimeGlobalManager.Instance;
                    yield return new Command_Action
                    {
                        defaultLabel = "MimicSlime_MemoryLabel".Translate(),
                        defaultDesc = "MimicSlime_MemoryDesc".Translate(),
                        icon = TexCommand.ForbidOff,
                        action = () => Find.WindowStack.Add(new Dialog_Mimicry(Pawn, manager, onlyread: true))
                    };
                }
            }
        }
    }

    public class HediffCompProperties_ShowMimicryQueue : HediffCompProperties
    {
        public HediffCompProperties_ShowMimicryQueue()
        {
            compClass = typeof(HediffComp_ShowMimicryQueue);
        }
    }
}
