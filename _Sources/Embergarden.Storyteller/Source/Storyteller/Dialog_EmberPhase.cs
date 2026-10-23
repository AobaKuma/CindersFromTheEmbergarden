using RimWorld;
using UnityEngine;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class Dialog_EmberPhase : Dialog_MessageBox
    {
        private const float WindowWidth = 900f;
        private const float WindowHeight = 250f;
        private const float ContentMargin = 14f;
        private const float PortraitBorderInset = 1f;
        private const float ColumnGap = 24f;
        private const float TitleHeight = 32f;
        private const float SectionGap = 8f;
        private const float MinimumButtonWidth = 180f;
        private const float MinimumButtonHeight = 40f;
        private const float ButtonPadding = 24f;
        private const float ScrollbarWidth = 16f;
        private readonly Texture2D portrait;
        private readonly EmberDialogueSession session;
        private Vector2 bodyScroll;

        public override Vector2 InitialSize => new Vector2(WindowWidth, WindowHeight);
        protected override float Margin => 0f;

        public Dialog_EmberPhase(EmberCycleGame owner, bool battle)
            : this(new EmberDialogueSession(owner, owner.Dialogue), battle) { }

        private Dialog_EmberPhase(EmberDialogueSession session, bool battle)
            : base(session.Text, session.ConfirmationText)
        {
            this.session = session;
            portrait = battle ? StorytellerPortraits.BattleDialog : StorytellerPortraits.Calm;
            title = DefDatabase<StorytellerDef>.GetNamed(CycleTuning.StorytellerName).LabelCap;
            forcePause = true;
            absorbInputAroundWindow = true;
            preventCameraMotion = true;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Color oldColor = GUI.color;
            Color oldContentColor = GUI.contentColor;
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            try
            {
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                Rect image = PortraitRect(inRect);
                Widgets.DrawTextureFitted(image, portrait, 1f);
                Rect dialogue = inRect.ContractedBy(ContentMargin);
                float occupiedRight = PortraitLayoutMetrics.OccupiedRightFraction(portrait);
                dialogue.xMin = Mathf.Min(image.xMax, image.x + image.width * occupiedRight)
                    + ColumnGap;
                GUI.color = EmberDialogueTheme.TextColor;
                DrawDialogue(dialogue);
            }
            finally
            {
                GUI.color = oldColor;
                GUI.contentColor = oldContentColor;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
            }
        }

        private Rect PortraitRect(Rect rect)
        {
            float height = Mathf.Max(0f, rect.height - ContentMargin);
            float width = height * portrait.width / portrait.height;
            return new Rect(rect.x + PortraitBorderInset,
                rect.yMax - PortraitBorderInset - height, width, height);
        }

        private void DrawDialogue(Rect rect)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, TitleHeight), title);
            Text.Font = GameFont.Small;
            Rect confirm = ConfirmationRect(rect);
            Rect body = Rect.MinMaxRect(rect.x, rect.y + TitleHeight + SectionGap,
                rect.xMax, confirm.y - SectionGap);
            DrawBody(body);
            Color oldColor = GUI.color;
            GUI.color = Color.white;
            Widgets.DrawButtonGraphic(confirm);
            GUI.color = oldColor;
            if (Widgets.ButtonText(confirm, buttonAText, false, true,
                Color.white, true, TextAnchor.MiddleCenter)) Close();
        }

        private Rect ConfirmationRect(Rect rect)
        {
            float width = Mathf.Min(rect.width, Mathf.Max(MinimumButtonWidth,
                Text.CalcSize(buttonAText).x + ButtonPadding));
            float height = Mathf.Max(ButtonHeight,
                Text.CalcHeight(buttonAText, width - ButtonPadding) + SectionGap);
            height = Mathf.Max(MinimumButtonHeight, height);
            return new Rect(rect.xMax - width, rect.yMax - height, width, height);
        }

        private void DrawBody(Rect rect)
        {
            float height = Text.CalcHeight(text, rect.width);
            if (height <= rect.height)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect, text);
                return;
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float width = rect.width - ScrollbarWidth;
            Rect content = new Rect(0f, 0f, width, Text.CalcHeight(text, width));
            Widgets.BeginScrollView(rect, ref bodyScroll, content);
            Widgets.Label(content, text);
            Widgets.EndScrollView();
        }

        public override void PostClose()
        {
            session.Close();
            base.PostClose();
        }
    }
}
