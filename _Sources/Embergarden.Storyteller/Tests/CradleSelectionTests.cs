using System.Reflection;
using System.Runtime.CompilerServices;
using Embergarden.Storytelling;
using HarmonyLib;
using RimWorld;
using Verse;

internal static class CradleSelectionTests
{
    private const int SampleCount = 4000;
    private const int SampleSeed = 171231;
    private const float SampleTolerance = 0.025f;
    private static readonly Faction Cradle =
        (Faction)RuntimeHelpers.GetUninitializedObject(typeof(Faction));
    private static readonly Faction Other =
        (Faction)RuntimeHelpers.GetUninitializedObject(typeof(Faction));
    private static bool cradleAvailable;
    private static bool otherAvailable;

    public static void Run()
    {
        Harmony harmony = new Harmony("CinderStory.Tests.Selection");
        MethodInfo target = AccessTools.Method(typeof(CradleRaidSelection), "TryFindFaction");
        harmony.Patch(target, prefix: new HarmonyMethod(typeof(CradleSelectionTests), nameof(FindFaction)));
        Rand.PushState(SampleSeed);
        try
        {
            CheckSeparatedPools();
            CheckCradleFrequency();
            CheckUnavailablePools();
        }
        finally
        {
            Rand.PopState();
            harmony.Unpatch(target, HarmonyPatchType.All, harmony.Id);
        }
        Console.WriteLine("[CinderStory] 摇篮概率池与不可用回退测试通过");
    }

    private static void CheckSeparatedPools()
    {
        FactionDef cradle = new FactionDef { defName = CycleTuning.CradleFactionName };
        FactionDef other = new FactionDef { defName = "OtherFaction" };
        Program.Check(!CradleRaidSelection.CanSelectPairFaction(cradle, null, false),
            "普通派系池仍可再次选中摇篮");
        Program.Check(!CradleRaidSelection.CanSelectPairFaction(other, null, true),
            "摇篮派系池混入普通派系");
    }

    private static void CheckCradleFrequency()
    {
        cradleAvailable = true;
        otherAvailable = true;
        int cradleCount = 0;
        for (int index = 0; index < SampleCount; index++)
        {
            IncidentParms parms = new IncidentParms();
            Program.Check(CradleRaidSelection.TryPrepare(null, parms, null), "合法派系未被选中");
            if (ReferenceEquals(parms.faction, Cradle)) cradleCount++;
            else Program.Check(ReferenceEquals(parms.faction, Other), "未明确选择合法派系");
        }
        float fraction = cradleCount / (float)SampleCount;
        Program.Check(Math.Abs(fraction - CycleTuning.CradleSelectionChance) < SampleTolerance,
            $"摇篮抽样比例偏离25% {fraction}");
    }

    private static void CheckUnavailablePools()
    {
        cradleAvailable = false;
        otherAvailable = true;
        CheckFallback(Other);
        cradleAvailable = true;
        otherAvailable = false;
        CheckFallback(Cradle);
        cradleAvailable = false;
        Program.Check(!CradleRaidSelection.TryPrepare(null, new IncidentParms(), null),
            "无合法派系时仍返回出击成功");
    }

    private static void CheckFallback(Faction expected)
    {
        for (int index = 0; index < SampleCount; index++)
        {
            IncidentParms parms = new IncidentParms();
            Program.Check(CradleRaidSelection.TryPrepare(null, parms, null)
                && ReferenceEquals(parms.faction, expected), "候选不可用时未使用另一合法派系池");
        }
    }

    private static bool FindFaction(bool cradleOnly, ref Faction faction, ref bool __result)
    {
        __result = cradleOnly ? cradleAvailable : otherAvailable;
        faction = __result ? (cradleOnly ? Cradle : Other) : null;
        return false;
    }
}
