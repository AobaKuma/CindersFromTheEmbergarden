using System.Linq;
using Verse;

namespace Embergarden.Storytelling
{
    public static class DialogueSelection
    {
        public static EmberDialogueDef Select(EmberDialoguePhase phase, DialogueContext context)
            => Select(phase, EmberDialogueTag.None, context);

        public static EmberDialogueDef SelectBattleEnd(DialogueContext context)
        {
            EmberDialogueTag tag = context.Deaths > 0 ? EmberDialogueTag.Death
                : context.Downed > 0 ? EmberDialogueTag.Wounded : EmberDialogueTag.Survived;
            return Select(EmberDialoguePhase.BattleEnd, tag, context)
                ?? Select(EmberDialoguePhase.BattleEnd, EmberDialogueTag.None, context);
        }

        private static EmberDialogueDef Select(EmberDialoguePhase phase,
            EmberDialogueTag tag, DialogueContext context)
        {
            var matching = DefDatabase<EmberDialogueDef>.AllDefsListForReading
                .Where(d => d.Matches(phase, context) && d.tag == tag).ToList();
            if (matching.Count == 0) return null;
            int priority = matching.Max(d => d.priority);
            return matching.Where(d => d.priority == priority).RandomElementByWeight(d => d.weight);
        }
    }
}
