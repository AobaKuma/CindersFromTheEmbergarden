using System.Xml;
using Embergarden.Storytelling;
using Verse;

internal static class LocalizationTests
{
    public static void Run()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../"));
        Program.Check(GenTypes.GetTypeInAnyAssembly(typeof(EmberDialogueDef).FullName)
            == typeof(EmberDialogueDef), "对话Def目录类型无法解析");
        XmlDocument baseDefs = Load(Path.Combine(root,
            "Defs/StorytellerDefs/EmberDialogues.xml"));
        int count = baseDefs.SelectNodes("/Defs/Embergarden.Storytelling.EmberDialogueDef")
            .Count;
        CheckDefaultFallback(root, baseDefs);
        CheckLanguage(root, "ChineseSimplified (简体中文)", count);
        CheckLanguage(root, "English", count);
        CheckStoryteller(root, "ChineseSimplified (简体中文)", true);
        CheckStoryteller(root, "English", true);
        CheckKeyed(root, "ChineseSimplified (简体中文)", false);
        CheckKeyed(root, "English", true);
        Console.WriteLine("[CinderStory] 对话中英文本地化完整性测试通过");
    }

    private static void CheckDefaultFallback(string root, XmlDocument baseDefs)
    {
        foreach (XmlNode node in baseDefs.SelectNodes("//text | //confirmationText"))
            Program.Check(!node.InnerText.Contains("占位符文本"), $"默认XML未使用英文 {node.Name}");
        XmlDocument storyteller = Load(Path.Combine(root,
            "Defs/StorytellerDefs/EmberWarden.xml"));
        Program.Check(!storyteller.InnerText.Contains("占位符文本"),
            "叙事者默认XML未使用英文");
    }

    private static void CheckLanguage(string root, string language, int expected)
    {
        XmlDocument translation = Load(Path.Combine(root, "Languages", language,
            "DefInjected/Embergarden.Storytelling.EmberDialogueDef/EmberDialogues.xml"));
        XmlNodeList nodes = translation.SelectNodes("/LanguageData/*");
        Program.Check(nodes.Count == expected * 2, $"{language} 对话字段数量错误");
        foreach (XmlNode node in nodes)
        {
            Program.Check(!string.IsNullOrWhiteSpace(node.InnerText)
                && !node.InnerText.Contains("占位符文本"), $"{node.Name} 未填写文案");
        }
    }

    private static void CheckStoryteller(string root, string language, bool delivered)
    {
        XmlDocument translation = Load(Path.Combine(root, "Languages", language,
            "DefInjected/StorytellerDef/EmberWarden.xml"));
        Program.Check(translation.SelectSingleNode("/LanguageData/Cinder_EmberWarden.label")
            != null, $"{language} 缺少叙事者名称");
        string label = translation.SelectSingleNode(
            "/LanguageData/Cinder_EmberWarden.label").InnerText;
        string description = translation.SelectSingleNode(
            "/LanguageData/Cinder_EmberWarden.description").InnerText;
        if (delivered)
            Program.Check(!label.Contains("占位符文本") && !description.Contains("占位符文本"),
                $"{language} 叙事者文案未填写");
        else
            Program.Check(label == "-[占位符文本-Cinder_EmberWarden]"
                && description == "-[占位符文本-Cinder_EmberWarden_Description]",
                $"{language} 叙事者占位符错误");
    }

    private static void CheckKeyed(string root, string language, bool deliveredContent)
    {
        string[] templateKeys = Array.Empty<string>();
        string[] diagnosticKeys =
            { "Cinder_BattleFallback", "Cinder_CalmFallback" };
        string[] interfaceKeys =
            { "Cinder_StorySettingsCategory", "Cinder_CalmMinDays",
                "Cinder_CalmMaxDays", "Cinder_ResetStorySettings",
                "Cinder_StorySettingsIntro", "Cinder_CalmVeryShortPrompt",
                "Cinder_DefaultReply" };
        XmlDocument translation = Load(Path.Combine(root, "Languages", language,
            "Keyed/EmberWarden.xml"));
        XmlNodeList nodes = translation.SelectNodes("/LanguageData/*");
        Program.Check(nodes.Count == templateKeys.Length + diagnosticKeys.Length
            + interfaceKeys.Length,
            $"{language} Keyed数量错误");
        foreach (string key in templateKeys)
        {
            string content = translation.SelectSingleNode($"/LanguageData/{key}")?.InnerText;
            if (deliveredContent)
                Program.Check(!string.IsNullOrWhiteSpace(content)
                    && !content.Contains("占位符文本"), $"{language} Keyed未填写 {key}");
            else
                Program.Check(content == $"-[占位符文本-{key}]",
                    $"{language} Keyed占位符错误 {key}");
        }
        foreach (string key in interfaceKeys)
        {
            string text = translation.SelectSingleNode($"/LanguageData/{key}")?.InnerText;
            Program.Check(!string.IsNullOrWhiteSpace(text) && !text.Contains("占位符文本"),
                $"{language} 界面文本被误设为占位符 {key}");
        }
        foreach (string key in diagnosticKeys)
        {
            string fallback = language == "English"
                ? "This is a default fallback text. If you see this, please report it."
                : "这是一个默认回退文本，如果你看到这个，请汇报";
            Program.Check(translation.SelectSingleNode($"/LanguageData/{key}").InnerText
                == fallback,
                $"{language} 默认回退文本错误 {key}");
        }
        Program.Check(translation.SelectSingleNode("/LanguageData/Cinder_CalmMinDays")
            .InnerText.Contains("{0}"), $"{language} 最短天数缺少参数");
        Program.Check(translation.SelectSingleNode("/LanguageData/Cinder_DefaultReply")
            .InnerText == "...", $"{language} 默认确认文本错误");
    }

    private static XmlDocument Load(string path)
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);
        return document;
    }
}
