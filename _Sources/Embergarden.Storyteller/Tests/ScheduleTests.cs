using Embergarden.Storytelling;

internal static class ScheduleTests
{
    public static void Run()
    {
        CheckCalmLimit();
        CheckCalmWithoutThreat();
        CheckWaveBoundaries();
        CheckFailedRetry();
        CheckBattleIntervalRange();
        CheckImmediateFirstWave();
        CheckEncounterIntervalsStartAfterClear();
        Console.WriteLine("[CinderStory] 周期边界与波次测试通过");
    }

    private static void CheckCalmLimit()
    {
        CycleSchedule cycle = new CycleSchedule();
        cycle.StartCalm(100, 900000, 450000);
        Program.Check(!cycle.CanSendCalmThreat(450099), "平静期威胁提前触发");
        Program.Check(cycle.CanSendCalmThreat(450100), "平静期威胁未按时触发");
        cycle.RecordSuccess();
        Program.Check(!cycle.CanSendCalmThreat(600000), "平静期允许重复威胁");
        Program.Check(!cycle.CanSendCalmThreat(900100), "过期平静期仍有威胁");
    }

    private static void CheckCalmWithoutThreat()
    {
        CycleSchedule cycle = new CycleSchedule();
        cycle.StartCalm(0, 1800000, -1);
        for (int now = 0; now <= 1800000; now += 1000)
            Program.Check(!cycle.CanSendCalmThreat(now), "零威胁平静期生成了威胁");
    }

    private static void CheckWaveBoundaries()
    {
        for (int goal = 6; goal <= 10; goal++)
        {
            CycleSchedule cycle = new CycleSchedule();
            cycle.StartBattle(100, goal);
            Program.Check(cycle.NextThreatTick == 100, "战斗期首波未立即就绪");
            while (cycle.WavesSent < goal)
            {
                int count = cycle.BatchSize(true);
                Program.Check(count >= 1 && count <= 2, "同时威胁超过两波");
                for (int index = 0; index < count; index++) cycle.RecordSuccess();
            }
            Program.Check(cycle.WavesSent == goal && cycle.BatchSize(true) == 0, "双波越过本轮上限");
            cycle.StartCalm(2000000, 900000, -1);
            Program.Check(!cycle.Battle && cycle.WavesSent == 0 && !cycle.CalmThreatSpent, "下一轮未重置");
        }
    }

    private static void CheckFailedRetry()
    {
        CycleSchedule cycle = new CycleSchedule();
        cycle.StartBattle(0, 6);
        cycle.RetryLater(1000);
        Program.Check(cycle.WavesSent == 0 && cycle.NextThreatTick == 2000, "失败消耗了波次");
        cycle.RecordSuccess();
        cycle.WaitAfterBattle(2000, CycleTuning.BattleIntervalMinTicks);
        Program.Check(cycle.WavesSent == 1
            && cycle.NextThreatTick == 2000 + CycleTuning.BattleIntervalMinTicks, "最短战后间隔错误");
        cycle.WaitAfterBattle(2000, CycleTuning.BattleIntervalMaxTicks);
        Program.Check(cycle.NextThreatTick == 2000 + CycleTuning.BattleIntervalMaxTicks,
            "最长战后间隔错误");
    }

    private static void CheckBattleIntervalRange()
    {
        for (int hours = CycleTuning.BattleIntervalMinHours;
            hours <= CycleTuning.BattleIntervalMaxHours; hours++)
        {
            int ticks = hours * CycleTuning.TicksPerHour;
            Program.Check(ticks >= CycleTuning.BattleIntervalMinTicks
                && ticks <= CycleTuning.BattleIntervalMaxTicks, "袭击间隔超出3至12小时");
        }
        Program.Check(CycleTuning.DoubleWaveChance == 0.33f, "双波概率不是33%");
        Program.Check(CycleTuning.CradleSelectionChance == 0.25f, "摇篮派系偏好不是25%");
    }

    private static void CheckImmediateFirstWave()
    {
        CycleSchedule cycle = new CycleSchedule();
        cycle.StartBattle(900000, 7);
        Program.Check(cycle.NextThreatTick == 900000, "战斗期首波未立即就绪");
        Program.Check(cycle.WavesSent == 0, "创建战斗期时虚增已发送波次");
        cycle.RecordSuccess();
        Program.Check(cycle.WavesSent == 1, "首波没有计入波次");
    }

    private static void CheckEncounterIntervalsStartAfterClear()
    {
        CycleSchedule cycle = new CycleSchedule();
        cycle.StartBattle(1000, 6);
        cycle.RecordSuccess();
        cycle.WaitAfterBattle(1350, 7 * CycleTuning.TicksPerHour);
        Program.Check(cycle.NextThreatTick == 1350 + 7 * CycleTuning.TicksPerHour,
            "随机间隔未从上一波清除时开始");
        Program.Check(cycle.NextThreatTick - 1350 >= CycleTuning.BattleIntervalMinTicks
            && cycle.NextThreatTick - 1350 <= CycleTuning.BattleIntervalMaxTicks,
            "战后间隔超出范围");
    }
}
