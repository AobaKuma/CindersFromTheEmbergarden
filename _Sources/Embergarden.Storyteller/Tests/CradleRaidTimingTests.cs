using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Embergarden.Storytelling;
using HarmonyLib;
using RimWorld;
using Verse;

internal static class CradleRaidTimingTests
{
    private const float OriginalRaidDelay = 60f;

    public static void Run()
    {
        CheckStorytellerSelection();
        CheckDifferentFactionPair();
        CheckOriginalInstructions();
        Console.WriteLine("[CinderStory] 摇篮初期袭击限制与原版筛选保留测试通过");
    }

    private static void CheckDifferentFactionPair()
    {
        FactionDef first = new FactionDef { defName = "First" };
        FactionDef second = new FactionDef { defName = "Second" };
        FactionDef cradle = new FactionDef { defName = CycleTuning.CradleFactionName };
        Program.Check(!CradleRaidSelection.CanSelectPairFaction(first, first, false),
            "双波允许重复派系");
        Program.Check(CradleRaidSelection.CanSelectPairFaction(second, first, false),
            "双波拒绝不同派系");
        Program.Check(CradleRaidSelection.CanSelectPairFaction(cradle, first, true),
            "双波摇篮候选未通过");
        Program.Check(!CradleRaidSelection.CanSelectPairFaction(second, first, true),
            "双波摇篮候选包含其他派系");
        Program.Check(!CradleRaidSelection.CanSelectPairFaction(cradle, cradle, true),
            "双波重复摇篮派系未排除");
    }

    private static void CheckStorytellerSelection()
    {
        Game previous = Current.Game;
        FactionDef cradle = new FactionDef
            { defName = CycleTuning.CradleFactionName, earliestRaidDays = OriginalRaidDelay };
        FactionDef other = new FactionDef
            { defName = "OtherFaction", earliestRaidDays = OriginalRaidDelay };
        try
        {
            Current.Game = null;
            CheckDelay(cradle, OriginalRaidDelay, "无游戏时改变了派系限制");
            SetStoryteller(CycleTuning.StorytellerName);
            CheckDelay(cradle, 0f, "本叙事者未解除摇篮初期限制");
            CheckDelay(other, OriginalRaidDelay, "本叙事者改变了其他派系限制");
            SetStoryteller("OtherStoryteller");
            CheckDelay(cradle, OriginalRaidDelay, "切换叙事者后未恢复摇篮限制");
            Program.Check(cradle.earliestRaidDays == OriginalRaidDelay, "派系全局定义被修改");
        }
        finally
        {
            Current.Game = previous;
        }
    }

    private static void SetStoryteller(string defName)
    {
        Game game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
        Storyteller storyteller = (Storyteller)RuntimeHelpers.GetUninitializedObject(typeof(Storyteller));
        storyteller.def = new StorytellerDef { defName = defName };
        game.storyteller = storyteller;
        Current.Game = game;
    }

    private static void CheckDelay(FactionDef faction, float expected, string message)
    {
        Program.Check(CradleRaidTimingPatch.EarliestRaidDays(faction) == expected, message);
    }

    private static void CheckOriginalInstructions()
    {
        MethodInfo target = AccessTools.Method(typeof(IncidentWorker_RaidEnemy), "FactionCanBeGroupSource",
            new[] { typeof(Faction), typeof(IncidentParms), typeof(bool) });
        FieldInfo field = AccessTools.Field(typeof(FactionDef), nameof(FactionDef.earliestRaidDays));
        List<CodeInstruction> original = PatchProcessor.GetOriginalInstructions(target);
        Program.Check(original.Count(instruction => instruction.LoadsField(field)) == 1,
            "原版袭击天数判定数量发生变化");
        List<CodeInstruction> copy = original.Select(instruction => new CodeInstruction(instruction)).ToList();
        List<CodeInstruction> patched = CradleRaidTimingPatch.Transpiler(copy).ToList();
        Program.Check(original.Count == patched.Count, "补丁改变了原版指令数量");
        for (int index = 0; index < original.Count; index++)
            CheckInstruction(original[index], patched[index], field);
    }

    private static void CheckInstruction(CodeInstruction original, CodeInstruction patched, FieldInfo field)
    {
        if (original.LoadsField(field))
        {
            MethodInfo helper = AccessTools.Method(typeof(CradleRaidTimingPatch),
                nameof(CradleRaidTimingPatch.EarliestRaidDays));
            Program.Check(patched.opcode == OpCodes.Call && Equals(patched.operand, helper),
                "袭击天数判定未替换为阶段条件");
        }
        else
            Program.Check(original.opcode == patched.opcode && Equals(original.operand, patched.operand),
                "补丁修改了天数之外的原版筛选");
        Program.Check(original.labels.SequenceEqual(patched.labels), "补丁丢失原版跳转标签");
        Program.Check(original.blocks.SequenceEqual(patched.blocks), "补丁丢失原版异常块");
    }
}
