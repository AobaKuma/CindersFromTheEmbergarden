using Embergarden.Storytelling;

internal static class DialogueTests
{
    public static void Run()
    {
        CheckCasualtyConditions();
        CheckTokensAndReplies();
        Console.WriteLine("[CinderStory] 对话条件 回复与占位符测试通过");
    }

    private static void CheckCasualtyConditions()
    {
        EmberDialogueDef grief = new EmberDialogueDef
        {
            phase = EmberDialoguePhase.BattleEnd,
            text = "死亡 {DEATHS}",
            conditions = { new EmberDialogueCondition { metric = DialogueMetric.Deaths, min = 1 } }
        };
        Program.Check(!grief.Matches(EmberDialoguePhase.BattleEnd, new DialogueContext()), "无死亡时选中悼念对话");
        Program.Check(grief.Matches(EmberDialoguePhase.BattleEnd, new DialogueContext { Deaths = 1 }), "死亡条件失效");
        Program.Check(!grief.Matches(EmberDialoguePhase.BattleStart, new DialogueContext { Deaths = 1 }), "阶段筛选失效");
        var range = new EmberDialogueCondition { metric = DialogueMetric.Mood, min = 20, max = 50, invert = true };
        Program.Check(!range.Matches(new DialogueContext { Mood = 30 }), "区间取反失效");
        Program.Check(range.Matches(new DialogueContext { Mood = 70 }), "区间取反未通过");
    }

    private static void CheckTokensAndReplies()
    {
        DialogueContext context = new DialogueContext { Deaths = 2, Waves = 8, CalmDays = 22 };
        EmberDialogueDef next = new EmberDialogueDef { text = "继续" };
        EmberDialogueDef dialogue = new EmberDialogueDef
        {
            text = "{WAVES}波 {DEATHS}人 {CALM_DAYS}天", confirmationText = "确认{WAVES}波",
            defaultNextDialogue = next,
            replies = { new EmberDialogueReply { text = "记住{DEATHS}人", nextDialogue = next } }
        };
        DialoguePlayback playback = DialoguePlayback.FromDefinition(dialogue, context);
        Program.Check(playback.Text == "8波 2人 22天", "占位符未展开");
        Program.Check(playback.ConfirmationText == "确认8波", "确认文本占位符未展开");
        Program.Check(playback.Replies[0].Text == "记住2人", "回复占位符未展开");
        Program.Check(playback.Replies[0].NextDialogue == next && playback.DefaultNextDialogue == next, "对话跳转丢失");
    }

}
