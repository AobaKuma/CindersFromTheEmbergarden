using Verse;

namespace Embergarden.Storytelling
{
    public static class RaidGenerationLog
    {
        public static bool Enabled = false;
        private static string lastBlocked;
        private static string lastFailure;

        public static void Reset()
        {
            lastBlocked = null;
            lastFailure = null;
        }

        public static void Info(string text)
        {
            if (Enabled) Log.Message("[CinderStory][Raid] " + text);
        }

        public static void Blocked(string key, string text)
        {
            if (!Enabled || lastBlocked == key) return;
            lastBlocked = key;
            Log.Message("[CinderStory][Raid] 调度阻止 " + text);
        }

        public static void Failure(string key, string text)
        {
            if (!Enabled || lastFailure == key) return;
            lastFailure = key;
            Log.Message("[CinderStory][Raid] 生成失败 " + text);
        }

        public static void Success(string text)
        {
            if (!Enabled) return;
            lastBlocked = null;
            lastFailure = null;
            Log.Message("[CinderStory][Raid] 生成成功 " + text);
        }
    }
}
