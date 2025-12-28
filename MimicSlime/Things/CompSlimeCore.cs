using RimWorld;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using static System.Collections.Specialized.BitVector32;

namespace MimicSlime
{
    public class CompProperties_SlimeCore : CompProperties
    {
        public int incubationTicks = (int)(MimicSlimeModSettings.SlimeRebornDays * 60000);
        public EffecterDef hatchEffect;
        public SoundDef hatchSound;
        public ThingDef hatchedPawnKind;

        public CompProperties_SlimeCore()
        {
            compClass = typeof(CompSlimeCore);
        }
    }

    public class CompSlimeCore : ThingComp, IThingHolder
    {
        private int incubationTicks;
        private ThingOwner<Pawn> innerContainer;
        private Faction originalFaction;

        public CompProperties_SlimeCore Props => (CompProperties_SlimeCore)props;
        public float IncubationProgress => incubationTicks / (float)Props.incubationTicks;

        public CompSlimeCore()
        {
            this.innerContainer = new ThingOwner<Pawn>(this, true, LookMode.Deep);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, innerContainer);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public Pawn GetDirectlyHeldPawn()
        {
            return innerContainer[0];
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (innerContainer == null) innerContainer = new ThingOwner<Pawn>(this, true, LookMode.Deep);
            incubationTicks = 0;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (incubationTicks < Props.incubationTicks)
            {
                incubationTicks++;
                if (incubationTicks >= Props.incubationTicks)
                {
                    Hatch();
                }
            }
        }

        private void Hatch()
        {
            Map map = parent.Map;
            IntVec3 position = parent.Position;
            if (innerContainer.Count == 0)
            {
                GenerateNewColonist();
            }

            if (innerContainer.Count > 0)
            {
                Pawn containedPawn = innerContainer[0];
                innerContainer.Remove(containedPawn);

                // 复活
                if (containedPawn.Dead)
                {
                    ResurrectionUtility.TryResurrect(containedPawn);
                }

                // 生成殖民者
                if (containedPawn.Spawned) containedPawn.DeSpawn();
                GenSpawn.Spawn(containedPawn, position, map, WipeMode.Vanish);

                if (originalFaction != null && containedPawn.Faction != originalFaction)
                {
                    containedPawn.SetFaction(originalFaction);
                }

                // 播放孵化效果和声音
                Props.hatchEffect?.Spawn(position, map, 1f);
                Props.hatchSound?.PlayOneShot(new TargetInfo(position, map));

                // 发送消息通知
                Messages.Message(
                    "SlimeCoreHatched".Translate(containedPawn.Name.ToStringShort),
                    containedPawn,
                    MessageTypeDefOf.PositiveEvent
                );
            }

            parent.Destroy();
        }

        private void GenerateNewColonist()
        {
            if (innerContainer.Count > 0)
            {
                Log.Error($"Slime core already contain a slime");
                return;
            }

            // 创建史莱姆
            PawnGenerationRequest request = new PawnGenerationRequest(
                MimicSlimeDefOf.MimicSlime_Colonist,
                Faction.OfPlayer,
                PawnGenerationContext.PlayerStarter,
                forceGenerateNewPawn: true
            );

            Pawn newPawn = PawnGenerator.GeneratePawn(request);
            newPawn.needs.AddOrRemoveNeedsAsAppropriate();

            if (newPawn.Spawned)
            {
                newPawn.DeSpawn();
            }

            innerContainer.TryAdd(newPawn);
        }

        public void ImprintPawn(Pawn pawn)
        {
            innerContainer.Clear();
            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }
            innerContainer.TryAdd(pawn);
            incubationTicks = 0; // 重置孵化计时器
            originalFaction = pawn.Faction;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref incubationTicks, "incubationTicks", 0);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref originalFaction, "originalFaction");
        }


        public override string CompInspectStringExtra()
        {
            string baseInfo;
            if (innerContainer.Count <= 0 || innerContainer[0] == null)
            {
                baseInfo = "SlimeCoreEmpty".Translate();
            }
            else
            {
                Pawn containedPawn = innerContainer[0];

                baseInfo = "SlimeCoreIncubating".Translate(
                                      containedPawn.Name.ToStringShort,
                                      IncubationProgress.ToStringPercent());

                baseInfo += "\n" + "SlimeCoreIncubationDays".Translate(
                                incubationTicks.TicksToDays().ToString("F1"),
                                Props.incubationTicks.TicksToDays().ToString("F1"));
            }

            return baseInfo;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Instant Hatch",
                    action = Hatch
                };
            }

            if (innerContainer.Count > 0)
            {
                string label = "SlimeCoreViewSlime".Translate();
                var manager = MimicSlimeGlobalManager.Instance;
                yield return new Command_Action
                {
                    defaultLabel = label,
                    defaultDesc = label,
                    icon = TexCommand.ForbidOff,
                    action = () => Find.WindowStack.Add(new Dialog_Mimicry(GetDirectlyHeldPawn(), manager, onlyread : true))
                };
            }
        }
    }
}
