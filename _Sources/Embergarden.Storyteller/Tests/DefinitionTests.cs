using System.Linq;
using System.Reflection;
using System.Xml;
using Embergarden.Storytelling;
using RimWorld;
using Verse;
using Verse.AI;

internal static class DefinitionTests
{
    public static void Run()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        XmlDocument document = new XmlDocument();
        document.Load(Path.Combine(root, "Defs/StorytellerDefs/EmberDialogues.xml"));
        foreach (XmlNode node in document.DocumentElement.ChildNodes)
        {
            EmberDialogueDef def = DirectXmlToObject.ObjectFromXml<EmberDialogueDef>(node, false);
            DefDatabase<EmberDialogueDef>.Add(def);
        }
        DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
        CheckPhaseSelection();
        CheckBattleStartPool();
        CheckReferences();
        CheckPrivateHooks();
        CheckStorytellerComponents(root);
        Console.WriteLine("[CinderStory] 原版 XML 解析 条件选词与确认文本测试通过");
    }

    private static void CheckPhaseSelection()
    {
        DialogueContext context = new DialogueContext { Waves = 8, CalmDays = 20 };
        EmberDialogueDef safe = DialogueSelection.SelectBattleEnd(context);
        Program.Check(safe.defName == "Cinder_Warden_EndSurvived", "无伤亡分支错误");
        Program.Check(safe.tag == EmberDialogueTag.Survived, "无伤亡标签错误");
        context.Downed = 2;
        EmberDialogueDef wounded = DialogueSelection.SelectBattleEnd(context);
        Program.Check(wounded.defName == "Cinder_Warden_EndWounded", "倒地分支错误");
        Program.Check(wounded.tag == EmberDialogueTag.Wounded, "倒地标签错误");
        context.Deaths = 1;
        EmberDialogueDef grief = DialogueSelection.SelectBattleEnd(context);
        Program.Check(grief.defName == "Cinder_Warden_EndLoss", "死亡优先级错误");
        Program.Check(grief.tag == EmberDialogueTag.Death, "死亡标签错误");
    }

    private static void CheckReferences()
    {
        EmberDialogueDef start = DefDatabase<EmberDialogueDef>.GetNamed("Cinder_Warden_StartApproaching");
        Program.Check(start.replies.Count == 0 && start.defaultNextDialogue == null, "残留开战回复分支");
        Program.Check(!string.IsNullOrWhiteSpace(start.confirmationText), "默认确认文本为空");
        DialoguePlayback playback = DialoguePlayback.FromDefinition(start,
            new DialogueContext { Waves = 9 });
        Program.Check(playback.ConfirmationText == start.confirmationText, "实际 XML 确认文本未展开");
    }

    private static void CheckBattleStartPool()
    {
        DialogueContext context = new DialogueContext { Waves = 8 };
        int count = DefDatabase<EmberDialogueDef>.AllDefsListForReading.Count(d =>
            d.phase == EmberDialoguePhase.BattleStart && d.weight > 0f);
        Program.Check(count >= 3, "开战文本池数量不足");
        EmberDialogueDef selected = DialogueSelection.Select(EmberDialoguePhase.BattleStart,
            context);
        Program.Check(selected?.phase == EmberDialoguePhase.BattleStart,
            "开战文本未从概率池选取");
    }

    private static void CheckPrivateHooks()
    {
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        MethodInfo method = typeof(MentalBreaker).GetMethod("TestMoodMentalBreak", flags);
        Program.Check(method?.ReturnType == typeof(bool) && method.GetParameters().Length == 0,
            "心情崩溃补丁签名不匹配");
        Program.Check(typeof(MentalBreaker).GetField("pawn", flags)?.FieldType == typeof(Pawn),
            "心情崩溃补丁字段不匹配");
    }

    private static void CheckStorytellerComponents(string root)
    {
        XmlDocument document = new XmlDocument();
        document.Load(Path.Combine(root, "Defs/StorytellerDefs/EmberWarden.xml"));
        foreach (XmlNode node in document.SelectNodes("/Defs/StorytellerDef/comps/li"))
        {
            StorytellerCompProperties properties = DirectXmlToObject.ObjectFromXml<StorytellerCompProperties>(node, false);
            Program.Check(properties?.compClass != null, "叙事者组件无法实例化 " + node.Attributes["Class"]?.Value);
            foreach (XmlNode field in node.ChildNodes)
                Program.Check(HasField(properties.GetType(), field.Name),
                    "叙事者组件 XML 存在未知字段 " + properties.GetType().Name + "." + field.Name);
        }
        DirectXmlCrossRefLoader.Clear();
    }

    private static bool HasField(Type type, string name)
    {
        while (type != null)
        {
            if (type.GetField(name, BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null) return true;
            type = type.BaseType;
        }
        return false;
    }
}
