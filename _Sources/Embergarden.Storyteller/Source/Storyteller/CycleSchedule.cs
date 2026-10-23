using System;

namespace Embergarden.Storytelling
{
    public sealed class CycleSchedule
    {
        public bool Initialized;
        public bool Battle;
        public int CalmEndTick;
        public int CalmThreatTick = -1;
        public bool CalmThreatSpent;
        public int WaveGoal;
        public int WavesSent;
        public int NextThreatTick;

        public void StartCalm(int now, int durationTicks, int threatOffset)
        {
            Initialized = true;
            Battle = false;
            CalmEndTick = now + durationTicks;
            CalmThreatTick = threatOffset < 0 ? -1 : now + threatOffset;
            CalmThreatSpent = false;
            WaveGoal = 0;
            WavesSent = 0;
            NextThreatTick = now;
        }

        public void StartBattle(int now, int waves)
        {
            Battle = true;
            WaveGoal = Math.Max(CycleTuning.WaveMin, Math.Min(CycleTuning.WaveMax, waves));
            WavesSent = 0;
            NextThreatTick = now;
        }

        public bool CanSendCalmThreat(int now)
        {
            return Initialized && !Battle && !CalmThreatSpent && CalmThreatTick >= 0
                && now >= CalmThreatTick && now < CalmEndTick && now >= NextThreatTick;
        }

        public int BatchSize(bool doubleWave)
        {
            return Math.Min(doubleWave ? 2 : 1, Math.Max(0, WaveGoal - WavesSent));
        }

        public void RecordSuccess()
        {
            if (Battle) WavesSent++;
            else CalmThreatSpent = true;
        }

        public void RetryLater(int now)
        {
            NextThreatTick = now + CycleTuning.CheckInterval;
        }

        public void WaitAfterBattle(int now, int intervalTicks)
        {
            NextThreatTick = now + intervalTicks;
        }
    }
}
