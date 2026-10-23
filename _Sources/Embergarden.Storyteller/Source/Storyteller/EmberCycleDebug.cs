using System;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public static class EmberCycleDebug
    {
        [DebugAction("CinderStory", "查看叙事者周期状态")]
        private static void ShowCycleState()
        {
            EmberCycleGame owner = EmberCycleGame.Current;
            if (owner == null) return;
            owner.EnsureInitialized();
            StorytellerDef current = Find.Storyteller.def;
            string active = EmberCycleGame.Selected ? "正在运行" : "未启用本叙事者 已停止调度";
            string status = $"[CinderStory]\n当前叙事者 {current?.label}\n"
                + $"当前defName {current?.defName}\n目标defName {CycleTuning.StorytellerName}\n"
                + $"周期调度 {active}\n{PhaseStatus(owner.Schedule, Find.TickManager.TicksGame)}\n"
                + $"等待阶段对话关闭 {owner.Dialogue != null}\n"
                + $"允许大型威胁 {Find.Storyteller.difficulty.allowBigThreats}\n"
                + $"叙事者已启用 {DebugSettings.enableStoryteller}\n"
                + $"基地存在活动威胁 {Find.Maps.Any(m => m.IsPlayerHome && m.dangerWatcher.DangerRating != StoryDanger.None)}";
            Find.WindowStack.Add(new Dialog_MessageBox(status));
        }

        private static string PhaseStatus(CycleSchedule schedule, int now)
        {
            if (!schedule.Initialized) return "周期尚未初始化";
            if (schedule.Battle)
                return $"保存的周期阶段 战斗期\n已发送波次 {schedule.WavesSent}/{schedule.WaveGoal}\n"
                    + "全部波次发出并解除威胁后进入平静期";
            float days = Math.Max(0, schedule.CalmEndTick - now) / (float)CycleTuning.TicksPerDay;
            string due = now >= schedule.CalmEndTick ? "\n平静期已到期 等待转换条件满足" : string.Empty;
            return $"保存的周期阶段 平静期\n平静期剩余天数 {days:0.00}{due}";
        }

        [DebugAction("CinderStory", "切换周期", displayPriority = 100)]
        private static void ToggleCycle()
        {
            if (!EmberCycleGame.Selected) return;
            EmberCycleGame.Current.ToggleCycle(Find.TickManager.TicksGame);
        }
    }
}
