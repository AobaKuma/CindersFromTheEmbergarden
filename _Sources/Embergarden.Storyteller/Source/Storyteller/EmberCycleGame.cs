using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class EmberCycleGame : GameComponent
    {
        public readonly CycleSchedule Schedule = new CycleSchedule();
        private BattleEncounter encounter;
        private int pendingDialog;
        private int lastProcessedTick = -1;
        private HashSet<int> battleDeaths = new HashSet<int>();
        private DialoguePlayback dialogue;

        public int BattleDeaths => battleDeaths.Count;
        public DialoguePlayback Dialogue => dialogue;

        public static EmberCycleGame Current => Verse.Current.Game?.GetComponent<EmberCycleGame>();
        public static bool Selected => Verse.Current.Game != null
            && Find.Storyteller?.def?.defName == CycleTuning.StorytellerName;
        public static bool IsCalm => Selected && Current != null && !Current.Schedule.Battle;

        public EmberCycleGame(Game game) { }

        public void EnsureInitialized()
        {
            if (Selected && !Schedule.Initialized && Find.Maps.Any(m => m.IsPlayerHome))
            {
                StartCalm(Find.TickManager.TicksGame, false);
                RaidGenerationLog.Info("初始化平静期");
            }
        }

        public void Advance(StorytellerComp_EmberCycle source)
        {
            int now = Find.TickManager.TicksGame;
            if (!Selected)
            {
                RaidGenerationLog.Blocked("not-selected", "当前未选择余烬花园叙事者");
                return;
            }
            if (lastProcessedTick == now) return;
            if (!Find.Maps.Any(m => m.IsPlayerHome))
            {
                RaidGenerationLog.Blocked("no-home", "没有玩家基地地图");
                return;
            }
            lastProcessedTick = now;
            if (!Schedule.Initialized) StartCalm(now, false);
            if (pendingDialog != 0)
            {
                RaidGenerationLog.Blocked("dialog", "阶段对话尚未关闭");
                return;
            }
            if (!Find.Storyteller.difficulty.allowBigThreats)
            {
                RaidGenerationLog.Blocked("big-threats-disabled", "当前难度禁止大型威胁");
                return;
            }
            if (encounter != null)
            {
                if (!encounter.IsClear(now))
                {
                    RaidGenerationLog.Blocked("encounter-active", "上一批威胁尚未清除");
                    return;
                }
                encounter = null;
                int interval = Rand.RangeInclusive(CycleTuning.BattleIntervalMinTicks,
                    CycleTuning.BattleIntervalMaxTicks);
                Schedule.WaitAfterBattle(now, interval);
                RaidGenerationLog.Info($"上一批已清除 下一次检查Tick {Schedule.NextThreatTick}");
            }
            if (Find.Maps.Any(m => m.IsPlayerHome && m.dangerWatcher.DangerRating != StoryDanger.None))
            {
                RaidGenerationLog.Blocked("danger-active", "基地仍处于原版活动威胁状态");
                return;
            }
            if (Schedule.Battle) AdvanceBattle(source, now);
            else AdvanceCalm(source, now);
        }

        private void AdvanceCalm(StorytellerComp_EmberCycle source, int now)
        {
            if (now >= Schedule.CalmEndTick)
            {
                BeginBattle(now);
                return;
            }
            if (Schedule.CanSendCalmThreat(now)) FireBatch(source, 1, now);
        }

        private void AdvanceBattle(StorytellerComp_EmberCycle source, int now)
        {
            if (Schedule.WavesSent >= Schedule.WaveGoal)
            {
                RaidGenerationLog.Info($"战斗期波次完成 {Schedule.WavesSent}/{Schedule.WaveGoal}");
                StartCalm(now, true);
                return;
            }
            if (now < Schedule.NextThreatTick)
            {
                RaidGenerationLog.Blocked("interval", $"等待下一次检查Tick {Schedule.NextThreatTick}");
                return;
            }
            int count = Schedule.BatchSize(Rand.Chance(CycleTuning.DoubleWaveChance));
            FireBatch(source, count, now);
        }

        private void FireBatch(StorytellerComp_EmberCycle source, int count, int now)
        {
            Map map = Find.Maps.Where(m => m.IsPlayerHome).RandomElement();
            RaidGenerationLog.Info($"开始尝试生成 {count} 波 当前Tick {now} 已发送 {Schedule.WavesSent}/{Schedule.WaveGoal}");
            BattleEncounter candidate = new BattleEncounter(map);
            bool fired = false;
            Faction previousFaction = null;
            for (int i = 0; i < count; i++)
            {
                if (!source.TryFireThreat(map, count == 2, previousFaction, out Faction firedFaction))
                {
                    RaidGenerationLog.Failure($"batch-{count}-wave-{i + 1}",
                        $"第 {i + 1} 波未执行 当前已发送 {Schedule.WavesSent}/{Schedule.WaveGoal}");
                    break;
                }
                Schedule.RecordSuccess();
                fired = true;
                previousFaction = firedFaction;
            }
            if (fired) encounter = candidate;
            Schedule.RetryLater(now);
            RaidGenerationLog.Info($"本批结束 已发送 {Schedule.WavesSent}/{Schedule.WaveGoal} 下次检查Tick {Schedule.NextThreatTick}");
        }

        private void StartCalm(int now, bool announce)
        {
            StorytellerSettings settings = CinderStorytellerMod.Settings ?? new StorytellerSettings();
            settings.Normalize();
            int days = Rand.RangeInclusive(settings.CalmMinDays, settings.CalmMaxDays);
            int duration = days * CycleTuning.TicksPerDay;
            int offset = Rand.Chance(CycleTuning.CalmThreatChance)
                ? (int)(duration * Rand.Range(CycleTuning.CalmThreatEarliest, CycleTuning.CalmThreatLatest)) : -1;
            int completedWaves = Schedule.WavesSent;
            Schedule.StartCalm(now, duration, offset);
            RaidGenerationLog.Reset();
            RaidGenerationLog.Info($"进入平静期 截止Tick {Schedule.CalmEndTick} 计划威胁Tick {Schedule.CalmThreatTick}");
            if (announce) Announce(false, completedWaves);
        }

        private void Announce(bool battle, int waves)
        {
            pendingDialog = battle ? 1 : 2;
            dialogue = DialoguePlayback.Create(battle, DialogueContext.Capture(this, waves));
            StorytellerPortraits.Refresh();
        }

        public void BeginBattle(int now)
        {
            Schedule.StartBattle(now, Rand.RangeInclusive(CycleTuning.WaveMin, CycleTuning.WaveMax));
            RaidGenerationLog.Reset();
            RaidGenerationLog.Info($"进入战斗期 目标波次 {Schedule.WaveGoal} 当前Tick {now}");
            battleDeaths.Clear();
            if (Find.Storyteller.difficulty.allowBigThreats
                && CycleSource() is StorytellerComp_EmberCycle source)
                FireBatch(source, 1, now);
            else
                RaidGenerationLog.Failure("begin-battle-source", "首波未尝试 叙事者组件或大型威胁开关不可用");
            Announce(true, Schedule.WaveGoal);
        }

        private static StorytellerComp_EmberCycle CycleSource()
            => Find.Storyteller?.storytellerComps?.OfType<StorytellerComp_EmberCycle>().FirstOrDefault();

        public void ToggleCycle(int now)
        {
            if (!Selected) return;
            EnsureInitialized();
            encounter = null;
            if (Schedule.Battle)
            {
                StartCalm(now, true);
                return;
            }
            BeginBattle(now);
        }

        private void CheckExpiredCalm()
        {
            EnsureInitialized();
            if (!Schedule.Initialized || Schedule.Battle || pendingDialog != 0) return;
            if (Find.TickManager.TicksGame < Schedule.CalmEndTick)
                return;
            if (!DebugSettings.enableStoryteller)
            {
                RaidGenerationLog.Blocked("debug-storyteller-disabled", "叙事者调度被开发设置关闭");
                return;
            }
            if (!Find.Storyteller.difficulty.allowBigThreats)
            {
                RaidGenerationLog.Blocked("expired-big-threats-disabled", "到期时当前难度禁止大型威胁");
                return;
            }
            if (encounter != null && !encounter.IsClear(Find.TickManager.TicksGame))
            {
                RaidGenerationLog.Blocked("expired-encounter-active", "到期时仍有未清除威胁");
                return;
            }
            if (Find.Maps.Any(m => m.IsPlayerHome && m.dangerWatcher.DangerRating != StoryDanger.None))
            {
                RaidGenerationLog.Blocked("expired-danger-active", "到期时基地仍处于原版活动威胁状态");
                return;
            }
            encounter = null;
            BeginBattle(Find.TickManager.TicksGame);
        }

        public override void GameComponentUpdate()
        {
            if (!Selected || Verse.Current.ProgramState != ProgramState.Playing) return;
            CheckExpiredCalm();
            if (pendingDialog == 0) return;
            if (Find.WindowStack.IsOpen<Dialog_EmberPhase>()) return;
            if (dialogue == null)
                dialogue = DialoguePlayback.Create(pendingDialog == 1,
                    DialogueContext.Capture(this, Schedule.WaveGoal));
            Find.WindowStack.Add(new Dialog_EmberPhase(this, pendingDialog == 1));
        }

        public void AcknowledgeDialog(DialoguePlayback expected)
        {
            if (!ReferenceEquals(dialogue, expected)) return;
            pendingDialog = 0;
            dialogue = null;
        }

        public void RecordColonistDeath(Pawn pawn)
        {
            if (Selected && Schedule.Battle) battleDeaths.Add(pawn.thingIDNumber);
        }

        public override void LoadedGame()
        {
            EnsureInitialized();
            StorytellerPortraits.Refresh();
        }

        public override void StartedNewGame() => LoadedGame();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Schedule.Initialized, "initialized");
            Scribe_Values.Look(ref Schedule.Battle, "battle");
            Scribe_Values.Look(ref Schedule.CalmEndTick, "calmEndTick");
            Scribe_Values.Look(ref Schedule.CalmThreatTick, "calmThreatTick", -1);
            Scribe_Values.Look(ref Schedule.CalmThreatSpent, "calmThreatSpent");
            Scribe_Values.Look(ref Schedule.WaveGoal, "waveGoal");
            Scribe_Values.Look(ref Schedule.WavesSent, "wavesSent");
            Scribe_Values.Look(ref Schedule.NextThreatTick, "nextThreatTick");
            Scribe_Values.Look(ref pendingDialog, "pendingDialog");
            Scribe_Deep.Look(ref encounter, "encounter");
            Scribe_Collections.Look(ref battleDeaths, "battleDeaths", LookMode.Value);
            Scribe_Deep.Look(ref dialogue, "dialogue");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && battleDeaths == null)
                battleDeaths = new HashSet<int>();
        }
    }
}
