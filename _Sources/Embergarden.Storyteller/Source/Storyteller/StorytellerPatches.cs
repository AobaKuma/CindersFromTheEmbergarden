using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Embergarden.Storytelling
{
    [StaticConstructorOnStartup]
    public static class StorytellerPatchEntry
    {
        static StorytellerPatchEntry()
        {
            new Harmony("BreadMo.Cinders.Storyteller").PatchAll();
        }
    }

    // 原版私有判定无扩展入口
    [HarmonyPatch(typeof(MentalBreaker), "TestMoodMentalBreak")]
    public static class CalmMentalBreakPatch
    {
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            if (__result && ___pawn.IsColonist && EmberCycleGame.IsCalm)
                __result = Rand.Chance(CycleTuning.MentalBreakChanceFactor);
        }
    }

    // 原版绘制无阶段回调
    [HarmonyPatch(typeof(StorytellerUI), nameof(StorytellerUI.DrawStorytellerSelectionInterface))]
    public static class StorytellerPortraitPatch
    {
        public static void Prefix() => StorytellerPortraits.Refresh();
    }

    // 原版事件入口无筛选回调
    [HarmonyPatch(typeof(Storyteller), nameof(Storyteller.TryFire))]
    public static class EmberThreatBudgetPatch
    {
        public static bool Prefix(FiringIncident fi)
        {
            if (!EmberCycleGame.Selected || fi.source is StorytellerComp_EmberCycle) return true;
            if (fi.source == null || fi.sourceQuestPart != null || fi.parms.quest != null || fi.parms.forced) return true;
            return fi.def.category != IncidentCategoryDefOf.ThreatBig;
        }
    }

    // 原版死亡通知无模组事件
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class EmberColonistDeathPatch
    {
        public static void Prefix(Pawn __instance, out bool __state)
        {
            __state = !__instance.Dead && __instance.IsColonist;
        }

        public static void Postfix(Pawn __instance, bool __state)
        {
            if (__state && __instance.Dead) EmberCycleGame.Current?.RecordColonistDeath(__instance);
        }
    }

    // 原版切换无初始化事件
    [HarmonyPatch(typeof(Storyteller), nameof(Storyteller.Notify_DefChanged))]
    public static class EmberStorytellerChangedPatch
    {
        public static void Postfix() => EmberCycleGame.Current?.EnsureInitialized();
    }
}
