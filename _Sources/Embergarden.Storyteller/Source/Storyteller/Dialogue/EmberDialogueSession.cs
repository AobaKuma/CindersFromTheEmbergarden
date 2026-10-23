using Verse;

namespace Embergarden.Storytelling
{
    public sealed class EmberDialogueSession
    {
        private readonly EmberCycleGame owner;
        private readonly DialoguePlayback current;

        public string Text => current.Text ?? string.Empty;
        public string ConfirmationText => string.IsNullOrWhiteSpace(current.ConfirmationText)
            ? "Cinder_DefaultReply".Translate().ToString() : current.ConfirmationText;

        public EmberDialogueSession(EmberCycleGame owner, DialoguePlayback playback)
        {
            this.owner = owner;
            current = playback;
        }

        public void Close() => owner.AcknowledgeDialog(current);
    }
}
