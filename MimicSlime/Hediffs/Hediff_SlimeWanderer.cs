using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MimicSlime
{
    public class Hediff_MimicSlimeWanderer : HediffWithComps
    {
        private bool IncidentEnd = false;

        public override void PostRemoved()
        {
            if (!IncidentEnd)
            {
                Pawn slime = MimicSlimeUtility.GetOriginalSlime(pawn);
                if (slime == null)
                {
                    slime = MimicSlimeUtility.CreateSlimePawn(MimicSlimeDefOf.MimicSlime_Colonist, Faction.OfPlayer, forceNoGear: false);
                    var manager = MimicSlimeGlobalManager.Instance;
                    manager.RegisterOriginalSlime(slime);
                    manager.AddEvoluPoints(slime, 10);
                    manager.AddPawnToQueueNoRemove(slime, pawn);
                }
                pawn.guilt.Notify_Guilty();
                slime.guilt.Notify_Guilty();
                if (pawn.Spawned && pawn.MapHeld != null)
                {
                    if (slime.Spawned) slime.DeSpawn();
                    GenSpawn.Spawn(slime, pawn.PositionHeld, pawn.MapHeld);
                    MimicSlimeUtility.RemovePawn(pawn);
                }
                Pawn finder = PawnsFinder.HomeMaps_FreeColonistsSpawned.RandomElement<Pawn>();
                TaggedString letterText = "LetterSlimeWandererExpose".Translate(pawn.Named("PAWN"), finder.Named("FINDER")).AdjustedFor(pawn);
                SendLetter(letterText, slime);
            }
            base.PostRemoved();
        }

        public override void Notify_PawnKilled()
        {
            TaggedString letterText = "LetterSlimeWandererKilled".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
            SendLetter(letterText, null);
            pawn.health.RemoveHediff(this);
            base.Notify_PawnKilled();
        }

        private void SendLetter(TaggedString letterText, Pawn pawn)
        {
            IncidentEnd = true;
            TaggedString title = "LetterLabelSlimeWandererExpose".Translate();
            Find.LetterStack.ReceiveLetter(title, letterText, LetterDefOf.NeutralEvent, new LookTargets(pawn));
        }
    }
}
