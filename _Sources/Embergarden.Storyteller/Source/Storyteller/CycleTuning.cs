namespace Embergarden.Storytelling
{
    public static class CycleTuning
    {
        public const string StorytellerName = "Cinder_EmberWarden";
        public const string CradleFactionName = "Cinder_ESDSP";
        public const int TicksPerDay = 60000;
        public const int CheckInterval = 1000;
        public const int TicksPerHour = 2500;
        public const int BattleIntervalMinHours = 3;
        public const int BattleIntervalMaxHours = 12;
        public const int CalmMinDays = 15;
        public const int CalmMaxDays = 30;
        public const int SettingsMinDays = 1;
        public const int SettingsMaxDays = 120;
        public const int ShortCalmPromptThreshold = 10;
        public const int WaveMin = 6;
        public const int WaveMax = 10;
        public const float DoubleWaveChance = 0.33f;
        public const float CalmThreatChance = 0.5f;
        public const float CalmThreatEarliest = 0.3f;
        public const float CalmThreatLatest = 0.8f;
        public const float MentalBreakChanceFactor = 0.5f;
        public const float CradleSelectionChance = 0.25f;
        public const int BattleIntervalMinTicks = BattleIntervalMinHours * TicksPerHour;
        public const int BattleIntervalMaxTicks = BattleIntervalMaxHours * TicksPerHour;
    }
}
