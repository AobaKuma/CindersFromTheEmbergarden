using System.Reflection;
using System.Runtime.CompilerServices;
using Embergarden.Storytelling;
using HarmonyLib;
using RimWorld;
using Verse;

internal enum BattleEntryStep
{
    Threat,
    Announcement
}

internal sealed class BattleEntryScope : IDisposable
{
    private const int RandomSeed = 6187;
    private const int MapArgument = 0;
    private const int PairArgument = 1;
    private const int ExcludedArgument = 2;
    private const int FiredFactionArgument = 3;
    private static BattleEntryScope active;
    private readonly Game previousGame = Current.Game;
    private readonly Harmony harmony = new Harmony("CinderStory.Tests.BattleEntry");
    private readonly bool success;
    private readonly StorytellerComp_EmberCycle source = new StorytellerComp_EmberCycle();
    private readonly Map map;
    public readonly EmberCycleGame Owner;
    public readonly List<BattleEntryStep> Steps = new List<BattleEntryStep>();
    public int ThreatCalls;
    public int WavesAtAnnouncement = -1;
    public bool CorrectThreatArguments;

    public bool HasEncounter => AccessTools.Field(typeof(EmberCycleGame), "encounter").GetValue(Owner) != null;

    public BattleEntryScope(bool success)
    {
        this.success = success;
        Game game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
        Owner = new EmberCycleGame(game);
        map = (Map)RuntimeHelpers.GetUninitializedObject(typeof(Map));
        map.listerThings = new ListerThings(ListerThingsUse.Global);
        PrepareGame(game);
        Current.Game = game;
        active = this;
        Rand.PushState(RandomSeed);
        PatchBoundaries();
    }

    private void PrepareGame(Game game)
    {
        AccessTools.Field(typeof(Game), "maps").SetValue(game, new List<Map> { map });
        game.components = new List<GameComponent> { Owner };
        game.storyteller = (Storyteller)RuntimeHelpers.GetUninitializedObject(typeof(Storyteller));
        game.storyteller.def = new StorytellerDef { defName = CycleTuning.StorytellerName };
        game.storyteller.difficulty = new Difficulty { allowBigThreats = true };
        game.storyteller.storytellerComps = new List<StorytellerComp> { source };
    }

    private void PatchBoundaries()
    {
        Patch(AccessTools.PropertyGetter(typeof(Map), nameof(Map.IsPlayerHome)), nameof(SelectHome));
        Patch(AccessTools.Method(typeof(StorytellerComp_EmberCycle), nameof(StorytellerComp_EmberCycle.TryFireThreat)),
            nameof(RecordThreat));
        Patch(AccessTools.Method(typeof(EmberCycleGame), "Announce"), nameof(RecordAnnouncement));
    }

    private void Patch(MethodBase target, string prefix)
        => harmony.Patch(target, prefix: new HarmonyMethod(typeof(BattleEntryScope), prefix));

    private static bool SelectHome(Map __instance, ref bool __result)
    {
        if (active == null || !ReferenceEquals(__instance, active.map)) return true;
        __result = true;
        return false;
    }

    private static bool RecordThreat(StorytellerComp_EmberCycle __instance, object[] __args, ref bool __result)
    {
        BattleEntryScope scope = active;
        scope.ThreatCalls++;
        scope.Steps.Add(BattleEntryStep.Threat);
        scope.CorrectThreatArguments = ReferenceEquals(__instance, scope.source)
            && ReferenceEquals(__args[MapArgument], scope.map) && !(bool)__args[PairArgument]
            && __args[ExcludedArgument] == null;
        __args[FiredFactionArgument] = null;
        __result = scope.success;
        return false;
    }

    private static bool RecordAnnouncement(EmberCycleGame __instance, bool battle, int waves)
    {
        BattleEntryScope scope = active;
        Program.Check(ReferenceEquals(__instance, scope.Owner) && battle
            && waves == scope.Owner.Schedule.WaveGoal, "真实入口未发送正确开场通知");
        scope.Steps.Add(BattleEntryStep.Announcement);
        scope.WavesAtAnnouncement = scope.Owner.Schedule.WavesSent;
        return false;
    }

    public void Dispose()
    {
        harmony.UnpatchAll(harmony.Id);
        Rand.PopState();
        active = null;
        Current.Game = previousGame;
    }
}
