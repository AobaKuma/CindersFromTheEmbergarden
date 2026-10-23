using Embergarden.Storytelling;
using HarmonyLib;

internal static class PatchBindingTests
{
    private const int ExpectedPatchCount = 8;

    public static void Run()
    {
        Harmony harmony = new Harmony("CinderStory.Tests.Bindings");
        try
        {
            harmony.PatchAll(typeof(CycleTuning).Assembly);
            int count = Harmony.GetAllPatchedMethods().Count(method =>
                Harmony.GetPatchInfo(method).Owners.Contains(harmony.Id));
            Program.Check(count == ExpectedPatchCount, $"补丁注册数量错误 {count}");
            Console.WriteLine("[CinderStory] 本机 1.7 全部 Harmony 补丁绑定通过");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }
}
