using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Embergarden.Storytelling
{
    public enum EmberDialoguePhase { BattleStart, BattleEnd, FollowUp }
    public enum EmberDialogueTag { None, Survived, Wounded, Death }
    public enum DialogueMetric { Deaths, Downed, Colonists, Waves, CalmDays, Day, Mood, ThreatScale }

    public class EmberDialogueCondition
    {
        public DialogueMetric metric;
        public float min = float.MinValue;
        public float max = float.MaxValue;
        public bool invert;

        public virtual bool Matches(DialogueContext context)
        {
            float value = context.Value(metric);
            bool within = value >= min && value <= max;
            return invert ? !within : within;
        }
    }

    public sealed class EmberDialogueDef : Def
    {
        public EmberDialoguePhase phase;
        public EmberDialogueTag tag;
        [MustTranslate] public string text;
        [MustTranslate] public string confirmationText;
        public float weight = 1f;
        public int priority;
        public bool matchAny;
        public List<EmberDialogueCondition> conditions = new List<EmberDialogueCondition>();
        public List<EmberDialogueReply> replies = new List<EmberDialogueReply>();
        public EmberDialogueDef defaultNextDialogue;

        public bool Matches(EmberDialoguePhase requestedPhase, DialogueContext context)
        {
            if (phase != requestedPhase || weight <= 0f || string.IsNullOrWhiteSpace(text)) return false;
            if (conditions.Count == 0) return true;
            return matchAny ? conditions.Any(c => c.Matches(context)) : conditions.All(c => c.Matches(context));
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (string.IsNullOrWhiteSpace(text)) yield return "[CinderStory] 对话文本为空";
            if (weight <= 0f) yield return "[CinderStory] 对话权重必须大于零";
            if (conditions.Any(c => c.min > c.max)) yield return "[CinderStory] 对话条件上下限颠倒";
            if (replies.Any(r => string.IsNullOrWhiteSpace(r.text))) yield return "[CinderStory] 回复文本为空";
        }
    }

    public sealed class EmberDialogueReply
    {
        [MustTranslate] public string text;
        public EmberDialogueDef nextDialogue;
        public List<EmberDialogueCondition> conditions = new List<EmberDialogueCondition>();
    }
}
