using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class DialoguePlayback : IExposable
    {
        public string Text;
        public string ConfirmationText;
        public DialogueContext Context;
        public List<DialogueReplySnapshot> Replies = new List<DialogueReplySnapshot>();
        public EmberDialogueDef DefaultNextDialogue;

        public static DialoguePlayback Create(bool battle, DialogueContext context)
        {
            EmberDialoguePhase phase = battle
                ? EmberDialoguePhase.BattleStart : EmberDialoguePhase.BattleEnd;
            EmberDialogueDef dialogue = battle
                ? DialogueSelection.Select(phase, context)
                : DialogueSelection.SelectBattleEnd(context);
            string fallback = (battle ? "Cinder_BattleFallback" : "Cinder_CalmFallback").Translate();
            DialoguePlayback playback = FromDefinition(dialogue, context);
            if (dialogue == null) playback.Text = context.Format(fallback);
            return playback;
        }

        public static DialoguePlayback FromDefinition(EmberDialogueDef dialogue, DialogueContext context)
        {
            return new DialoguePlayback
            {
                Text = context.Format(dialogue?.text ?? string.Empty),
                ConfirmationText = string.IsNullOrWhiteSpace(dialogue?.confirmationText)
                    ? null : context.Format(dialogue.confirmationText),
                Context = context,
                DefaultNextDialogue = dialogue?.defaultNextDialogue,
                Replies = dialogue?.replies.Where(r => r.conditions.All(c => c.Matches(context)))
                    .Select(r => new DialogueReplySnapshot
                    { Text = context.Format(r.text), NextDialogue = r.nextDialogue }).ToList()
                    ?? new List<DialogueReplySnapshot>()
            };
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Text, "text");
            Scribe_Values.Look(ref ConfirmationText, "confirmationText");
            Scribe_Deep.Look(ref Context, "context");
            Scribe_Collections.Look(ref Replies, "replies", LookMode.Deep);
            Scribe_Defs.Look(ref DefaultNextDialogue, "defaultNextDialogue");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Replies == null)
                Replies = new List<DialogueReplySnapshot>();
        }
    }

    public sealed class DialogueReplySnapshot : IExposable
    {
        public string Text;
        public EmberDialogueDef NextDialogue;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Text, "text");
            Scribe_Defs.Look(ref NextDialogue, "nextDialogue");
        }
    }
}
