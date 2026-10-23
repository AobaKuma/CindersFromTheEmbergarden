using System.Linq;
using Verse;

namespace Embergarden.Storytelling
{
    public static class DialogueSelection
    {
        public static EmberDialogueDef Select(EmberDialoguePhase phase, DialogueContext context)
        {
            var matching = DefDatabase<EmberDialogueDef>.AllDefsListForReading
                .Where(d => d.Matches(phase, context)).ToList();
            if (matching.Count == 0) return null;
            int priority = matching.Max(d => d.priority);
            return matching.Where(d => d.priority == priority).RandomElementByWeight(d => d.weight);
        }
    }
}
