using Embergarden.Storytelling;
using Verse;

internal static class ScribeTests
{
    public static void Run()
    {
        CheckCycleRoundTrip();
        CheckDialogueRoundTrip();
        Console.WriteLine("[CinderStory] 原版 Scribe 存取测试通过");
    }

    private static void CheckCycleRoundTrip()
    {
        EmberCycleGame original = new EmberCycleGame(null);
        original.Schedule.StartCalm(100, 900000, 300000);
        original.Schedule.RecordSuccess();
        original.Schedule.StartBattle(900100, 9);
        original.Schedule.RecordSuccess();
        original.Schedule.RecordSuccess();
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        Scribe.saver.InitSaving(path, "test");
        original.ExposeData();
        Scribe.saver.FinalizeSaving();
        EmberCycleGame loaded = new EmberCycleGame(null);
        Scribe.loader.InitLoading(path);
        loaded.ExposeData();
        Scribe.loader.FinalizeLoading();
        Program.Check(loaded.Schedule.Battle && loaded.Schedule.WaveGoal == 9
            && loaded.Schedule.WavesSent == 2 && loaded.Schedule.CalmThreatSpent, "周期读档丢失状态");
        File.Delete(path);
    }

    private static void CheckDialogueRoundTrip()
    {
        DialoguePlayback original = new DialoguePlayback
        {
            Text = "测试对话", ConfirmationText = "测试确认", VisibleCharacters = 2,
            Context = new DialogueContext { Deaths = 1, Waves = 8 },
            Replies = { new DialogueReplySnapshot { Text = "回复" } }
        };
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        Scribe.saver.InitSaving(path, "test");
        original.ExposeData();
        Scribe.saver.FinalizeSaving();
        DialoguePlayback loaded = new DialoguePlayback();
        Scribe.loader.InitLoading(path);
        loaded.ExposeData();
        Scribe.loader.FinalizeLoading();
        Program.Check(loaded.Text == original.Text && loaded.VisibleCharacters == 2, "对话进度丢失");
        Program.Check(loaded.ConfirmationText == "测试确认", "单确认文本丢失");
        Program.Check(loaded.Context.Deaths == 1 && loaded.Replies[0].Text == "回复", "对话上下文或回复丢失");
        File.Delete(path);
    }
}
