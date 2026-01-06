using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MimicSlime
{
    public class Dialog_Mimicry : Window
    {
        private readonly Pawn caster;
        private readonly MimicSlimeGlobalManager manager;
        private Pawn selectedPawn;
        private Vector2 scrollPosition;
        private bool forceReload;
        private const float RowHeight = 30f;
        private const float ButtonWidth = 100f;
        private const float ButtonHeight = 35f;
        private const float Margin = 10f;
        private bool onlyread;
        private Pawn originalSlime;
        private float statusHeight;

        public Dialog_Mimicry(Pawn caster, MimicSlimeGlobalManager manager, bool onlyread = false)
        {
            if (caster == null)
            {
                Log.Error("Ability_Mimicry : Create dialog error: caster is null");
                Close();
                return;
            }

            if (manager == null)
            {
                Log.Error("Ability_Mimicry : Create dialog error: manager is null");
                Close();
                return;
            }

            this.caster = caster;
            this.manager = manager;
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = true;
            this.originalSlime = MimicSlimeUtility.GetOriginalSlime(caster);
            this.onlyread = onlyread;

            if (originalSlime == null)
            {
                Log.Error("Ability_Mimicry : can't find original slime");
                Close();
                return;
            }
            else
            {
                MimicSlimeUtility.ResurrectionThenCureAll(originalSlime);
                MimicSlimeUtility.AddToFaction(caster, originalSlime);
            }

            foreach (var p in manager.GetOriginalSlimeQueue(originalSlime))
            {
                if (p != null)
                {
                    MimicSlimeUtility.ResurrectionThenCureAll(p);
                    MimicSlimeUtility.AddToFaction(originalSlime, p);
                }
            }

            CalculateStatusHeight();
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        // 计算状态区域所需高度
        private void CalculateStatusHeight()
        {
            string statusText = GetStatusText();
            Text.Font = GameFont.Small;
            Vector2 textSize = Text.CalcSize(statusText);
            statusHeight = Mathf.Max(40f, textSize.y + Margin * 2);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (forceReload) { forceReload = false; return; }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 40f), "MimicryDiaglogLabel".Translate());
            Text.Font = GameFont.Small;

            // 状态提示区域（点数、下一阶）
            Rect statusRect = new Rect(0, 35f, inRect.width, statusHeight);
            DrawStatus(statusRect);

            // 列表区域
            float listTop = statusRect.yMax + Margin;
            float listHeight = inRect.height - listTop - (ButtonHeight * 2 + Margin * 3);
            Rect listRect = new Rect(0, listTop, inRect.width, listHeight);
            DrawPawnList(listRect);

            // 按钮区域
            if (onlyread) DrawButtons_ReadOnly(inRect); 
            else DrawButtons(inRect);
            DrawButtons_RightVertical(inRect);
        }

        private void DrawStatus(Rect rect)
        {
            string statusText = GetStatusText();
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect, statusText);
        }

        private string GetStatusText()
        {
            int points = manager.GetEvoluPoints(originalSlime);
            string nextTxt;
            if (points < 5) nextTxt = "5";
            else if (points < 20) nextTxt = "20";
            else if (points < 30) nextTxt = "30";
            else if (points < 50) nextTxt = "50";
            else nextTxt = null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Points".Translate() + ": " + points);
            if (nextTxt != null)
                sb.AppendLine("PointsNeededNextLevel".Translate() + ": " + nextTxt);

            return sb.ToString().TrimEndNewlines();
        }

        private void DrawPawnList(Rect rect)
        {
            var queue = new Queue<Pawn>();
            foreach (var p in manager.GetOriginalSlimeQueue(originalSlime))
                if (p != caster) queue.Enqueue(p);

            if (queue.Count == 0)
            {
                Widgets.Label(rect, "NoAbsorbedPawns".Translate());
                return;
            }

            Rect viewRect = new Rect(0, 0, rect.width - 20f, queue.Count * RowHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float y = 0;
            foreach (var pawn in queue.ToList())
            {
                var row = new Rect(0, y, viewRect.width, RowHeight);
                DrawPawnRow(row, pawn);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private void DrawPawnRow(Rect rect, Pawn pawn)
        {

            if (selectedPawn == pawn) Widgets.DrawHighlightSelected(rect);
            else if (Mouse.IsOver(rect)) Widgets.DrawHighlight(rect);

            if (Widgets.ButtonInvisible(rect)) selectedPawn = pawn;

            string points = PawnPoints(pawn).ToString();

            string name = (pawn.Name?.ToStringFull ?? "UnnamedPawn".Translate())+ "\t" + ("Points".Translate() + ": " + points);

            Rect textRect = new Rect(10, rect.y, rect.width - 40, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, name);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = Color.white;
        }

        private void DrawButtons(Rect inRect)
        {
            float topRowY = inRect.height - ButtonHeight * 2 - Margin * 2;

            // 第一行
            float topRowX = (inRect.width - (ButtonWidth * 3 + Margin * 2)) / 2;
            var confirm = new Rect(topRowX, topRowY, ButtonWidth, ButtonHeight);
            GUI.enabled = selectedPawn != null;
            if (Widgets.ButtonText(confirm, "Confirm".Translate()))
            {
                HandleConfirm(); Close();
            }

            var delete = new Rect(confirm.xMax + Margin, topRowY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(delete, "Delete".Translate()))
                HandleDeletePawn(selectedPawn);

            var details = new Rect(delete.xMax + Margin, topRowY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(details, "Details".Translate()))
                HandleShowDetails(selectedPawn);

            // 第二行
            float bottomRowX = (inRect.width - (ButtonWidth * 2 + Margin)) / 2;
            var unmorph = new Rect(bottomRowX, topRowY + ButtonHeight + Margin, ButtonWidth, ButtonHeight);
            GUI.enabled = !MimicSlimeUtility.IsOriginalSlime(caster);
            if (Widgets.ButtonText(unmorph, "ReleaseMimic".Translate()))
            {
                HandleUnmorph(); Close();
            }

            var cancel = new Rect(unmorph.xMax + Margin, unmorph.y, ButtonWidth, ButtonHeight);
            GUI.enabled = true;
            if (Widgets.ButtonText(cancel, "Cancel".Translate()))
                Close();
        }

        private void DrawButtons_ReadOnly(Rect inRect)
        {
            float rowY = inRect.height - ButtonHeight - Margin;
            float totalW = ButtonWidth * 3 + Margin;
            float startX = (inRect.width - totalW) / 3;

            GUI.enabled = selectedPawn != null;
            var details = new Rect(startX, rowY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(details, "Details".Translate()))
                HandleShowDetails(selectedPawn);

            GUI.enabled = caster != null && selectedPawn != null;
            var delete = new Rect(details.xMax + Margin, rowY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(delete, "Delete".Translate()))
                HandleDeletePawn(selectedPawn);

            GUI.enabled = true;
            var cancel = new Rect(delete.xMax + Margin, rowY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(cancel, "Cancel".Translate()))
                Close();
        }

        private void DrawButtons_RightVertical(Rect inRect)
        {
            float startX = inRect.width - ButtonWidth - Margin;
            float startY = inRect.height - ButtonHeight * 3 - Margin * 3 + ButtonHeight + Margin;

            GUI.enabled = MimicSlimeUtility.IsOriginalSlime(caster) && (manager.GetEvoluPoints(caster) > 20 || MimicSlimeModSettings.IgnoreLevel);
            var shape = new Rect(startX, startY, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(shape, "ChangeShape".Translate()))
            {
                ChangeShape(caster); Close();
            }
            var hair = new Rect(startX, shape.yMax + Margin, ButtonWidth, ButtonHeight);
            if (Widgets.ButtonText(hair, "ChangeHair".Translate()))
            {
                MimicSlimeUtility.ChangeHair(caster, manager.GetHair(caster), true);
                caster.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        private int PawnPoints(Pawn pawn)
        {
            int points = 1;
            float wealthImpact = pawn.MarketValue / MimicSlimeModSettings.PointDivisor;
            points += Mathf.RoundToInt(wealthImpact);
            points = Mathf.Clamp(points, 0, 100);

            return points;
        }

        private void HandleDeletePawn(Pawn pawn)
        {
            if (pawn == null) return;

            // 增加点数
            manager.AddEvoluPoints(pawn, PawnPoints(pawn));

            SkillGainByDelete(pawn, originalSlime);
            if (MimicSlimeModSettings.GainTrait && Rand.Chance(MimicSlimeModSettings.GainTraitInt/100))
            {
                TraitGainByDelete(pawn, originalSlime);
            }

            // 销毁角色
            manager.RemoveFromQueue(originalSlime, pawn);
            if (!pawn.Destroyed) pawn.Destroy();

            selectedPawn = null;
            scrollPosition = Vector2.zero;
            forceReload = true;
        }

        private void HandleConfirm()
        {
            if (selectedPawn == null)
            {
                Messages.Message("NoPawnSelected".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            // 传送
            if (caster.MapHeld != null)
            {
                // 传回
                MimicSlimeUtility.ResurrectionThenCureAll(selectedPawn);
                if (selectedPawn.Spawned) selectedPawn.DeSpawn();
                GenSpawn.Spawn(selectedPawn, caster.Position, caster.MapHeld);
                MimicSlimeUtility.AddToFaction(caster, selectedPawn);
                MimicSlimeUtility.AddAbility(MimicSlimeDefOf.SlimeAbsorbAbility, selectedPawn);
                MimicSlimeUtility.AddAbility(MimicSlimeDefOf.MimicryAbility, selectedPawn);
                MimicSlimeUtility.ChangeData(caster, selectedPawn);

                manager.AddPawnToQueueAndRemovePawn(originalSlime, caster, MimicSlimeModSettings.StripClothing);
            }
        }

        // 解除拟态逻辑
        private void HandleUnmorph()
        {
            // 召回原始史莱姆
            if (caster.MapHeld != null)
            {
                MimicSlimeUtility.AddToFaction(caster, originalSlime);
                MimicSlimeUtility.ResurrectionThenCureAll(originalSlime);

                // 传回原始史莱姆
                if (originalSlime.Spawned) originalSlime.DeSpawn();
                GenSpawn.Spawn(originalSlime, caster.Position, caster.MapHeld);
                MimicSlimeUtility.ChangeData(caster, originalSlime);

                // 移除当前拟态Pawn
                manager.AddPawnToQueueAndRemovePawn(originalSlime, caster, MimicSlimeModSettings.StripClothing);
            }
        }
        private void HandleShowDetails(Pawn pawn)
        {
            if (pawn == null) return;
            if (pawn.Dead)
            {
                MimicSlimeUtility.ResurrectionThenCureAll(pawn);
            }

            try
            {
                Find.WindowStack.Add(new Dialog_InfoCard(pawn));
            }
            catch (Exception ex)
            {
                Log.Error($"Error showing pawn details: {ex}");
            }
        }

        private void SkillGainByDelete(Pawn pawn, Pawn slime)
        {
            int highestSkill = 0;
            SkillDef skillToAdd = SkillDefOf.Melee;
            foreach (var skill in pawn.skills.skills)
            {
                if (skill.levelInt > highestSkill)
                {
                    highestSkill = skill.levelInt;
                    skillToAdd = skill.def;
                }
            }

            slime.skills.GetSkill(skillToAdd).levelInt += 1;
            Messages.Message("SlimeSkillGainByDelete".Translate(pawn.LabelShort, slime.LabelShort, skillToAdd.label), MessageTypeDefOf.SilentInput);
        }

        private void TraitGainByDelete(Pawn pawn, Pawn slime)
        {
            List<Trait> traits = pawn.story.traits.allTraits.ToList();
            traits.Shuffle();
            foreach (Trait trait in traits)
            {
                if (MimicSlimeUtility.CanAddTarit(trait.def))
                {
                    slime.story.traits.GainTrait(trait);
                    Messages.Message("SlimeTraitGainByDelete".Translate(pawn.LabelShort, slime.LabelShort, trait.Label), MessageTypeDefOf.SilentInput);
                    return;
                }
            }
        }

        private void ChangeShape(Pawn pawn)
        {
            if (pawn == null) return;

            var health = pawn.health;

            Hediff slime = health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.MimicSlime);
            if (slime != null)
            {
                health.RemoveHediff(slime);
                health.AddHediff(MimicSlimeDefOf.HumanoidMimicSlime);

                pawn.story.bodyType = MimicSlimeDefOf.MimicSlimeBody_HumanoidType;
                MimicSlimeUtility.ChangeHair(pawn, manager.GetHair(pawn));
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                return;
            }

            slime = health.hediffSet.GetFirstHediffOfDef(MimicSlimeDefOf.HumanoidMimicSlime);
            if (slime != null)
            {
                health.RemoveHediff(slime);
                health.AddHediff(MimicSlimeDefOf.MimicSlime);

                pawn.story.bodyType = MimicSlimeDefOf.MimicSlimeBody_SlimeType;
                pawn.story.hairDef = HairDefOf.Bald;
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }
}