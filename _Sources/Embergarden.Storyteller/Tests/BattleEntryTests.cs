using Embergarden.Storytelling;

internal static class BattleEntryTests
{
    private const int EntryTick = 900000;

    public static void Run()
    {
        CheckSuccessfulEntry();
        CheckFailedEntry();
        Console.WriteLine("[CinderStory] 战斗首波真实入口与失败计数测试通过");
    }

    private static void CheckSuccessfulEntry()
    {
        using BattleEntryScope scope = new BattleEntryScope(true);
        scope.Owner.BeginBattle(EntryTick);
        CheckEntry(scope);
        Program.Check(scope.Owner.Schedule.WavesSent == 1, "首波成功后未增加波次");
        Program.Check(scope.WavesAtAnnouncement == 1, "开场通知先于首波计数");
        Program.Check(scope.HasEncounter, "首波成功后未记录战斗对象");
    }

    private static void CheckFailedEntry()
    {
        using BattleEntryScope scope = new BattleEntryScope(false);
        scope.Owner.BeginBattle(EntryTick);
        CheckEntry(scope);
        Program.Check(scope.Owner.Schedule.WavesSent == 0, "首波失败却增加波次");
        Program.Check(scope.WavesAtAnnouncement == 0, "首波失败时通知读取了虚假波次");
        Program.Check(!scope.HasEncounter, "首波失败却记录了战斗对象");
    }

    private static void CheckEntry(BattleEntryScope scope)
    {
        Program.Check(scope.Owner.Schedule.Battle, "真实入口未进入战斗期");
        Program.Check(scope.ThreatCalls == 1, "进入战斗期未立即尝试唯一首波");
        Program.Check(scope.CorrectThreatArguments, "首波未使用当前地图和周期组件");
        Program.Check(scope.Owner.Schedule.WaveGoal >= CycleTuning.WaveMin
            && scope.Owner.Schedule.WaveGoal <= CycleTuning.WaveMax, "真实入口波次目标越界");
        Program.Check(scope.Owner.Schedule.NextThreatTick == EntryTick + CycleTuning.CheckInterval,
            "首波尝试后未安排重试时间");
        Program.Check(scope.Steps.SequenceEqual(new[] { BattleEntryStep.Threat, BattleEntryStep.Announcement }),
            "真实入口未在开场通知前尝试首波");
    }
}
