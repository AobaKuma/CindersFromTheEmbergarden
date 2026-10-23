using System.Reflection;
using System.Runtime.CompilerServices;
using Embergarden.Storytelling;
using UnityEngine;
using UnityStubs.Gui;
using Verse;

internal static class NativeThemeTests
{
    private const string TitleText = "叙事者颜色回归";
    private const string BodyText = "原版正文颜色回归";
    private const string ConfirmationText = "确认颜色回归";
    private const int PortraitWidth = 2664;
    private const int PortraitHeight = 1500;
    private const int OpaqueColumns = 2014;
    private const float PortraitTextGap = 24f;
    private const float ContentPadding = 14f;
    private const float WindowBorder = 1f;
    private static readonly Rect WindowRect = new Rect(0f, 0f, 900f, 250f);

    public static void Run()
    {
        using RenderingScope scope = new RenderingScope();
        IGuiBackend backend = DispatchProxy.Create<IGuiBackend, NativeThemeBackend>();
        GuiBackend.Current = backend;
        Event.current = new Event { type = EventType.repaint, mousePosition = new Vector2(-100f, -100f) };
        CheckNativeWindow((NativeThemeBackend)backend);
        CheckLongText((NativeThemeBackend)backend);
        Console.WriteLine("[CinderStory] 原版文字 单确认布局与长文滚动渲染测试通过");
    }

    private static void PrepareLanguage()
    {
        if (LanguageDatabase.activeLanguage != null) return;
        LoadedLanguage language = (LoadedLanguage)RuntimeHelpers.GetUninitializedObject(typeof(LoadedLanguage));
        language.folderName = "English";
        language.info = new LanguageInfo();
        LanguageDatabase.activeLanguage = language;
    }

    private static void PrepareShaderSources()
    {
        string root = Path.GetDirectoryName(typeof(DiaNode).Assembly.Location);
        Type loader = Assembly.Load("Waylith").GetType("Waylith.Rendering.ShaderSourceLoader", true);
        loader.GetMethod("RegisterShaderSourceDirectory").Invoke(null,
            new object[] { Path.Combine(root, "Rendering", "Shaders") });
    }

    private static void CheckNativeWindow(NativeThemeBackend recording)
    {
        Dialog_EmberPhase window = CreateWindow();
        Color incomingColor = new Color(0.21f, 0.42f, 0.63f, 0.74f);
        Color incomingContent = new Color(0.62f, 0.51f, 0.4f, 0.83f);
        GUI.color = incomingColor;
        GUI.contentColor = incomingContent;
        window.DoWindowContents(WindowRect);
        CheckText(recording, TitleText);
        CheckText(recording, BodyText);
        CheckText(recording, ConfirmationText);
        CheckLayout(recording);
        CheckPortrait(recording);
        Program.Check(window.InitialSize == new Vector2(900f, 250f), "窗口尺寸未缩减");
        Program.Check((float)typeof(Dialog_EmberPhase).GetProperty("Margin",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window) == 0f, "窗口仍缩进立绘区域");
        Program.Check(GUI.color == incomingColor && GUI.contentColor == incomingContent,
            "原版对话绘制污染了全局颜色");
        Program.Check(recording.ScrollViews == 0, "短正文不应显示滚动区域");
        Program.Check(recording.DrawCalls == 3, "未保持标题 正文和单一确认");
    }

    private static Dialog_EmberPhase CreateWindow()
    {
        Dialog_EmberPhase window = (Dialog_EmberPhase)RuntimeHelpers.GetUninitializedObject(typeof(Dialog_EmberPhase));
        Texture2D portrait = new Texture2D(PortraitWidth, PortraitHeight);
        Color32[] pixels = new Color32[PortraitWidth * PortraitHeight];
        Color32 opaque = new Color32(255, 255, 255, 255);
        for (int y = 0; y < PortraitHeight; y++)
            Array.Fill(pixels, opaque, y * PortraitWidth, OpaqueColumns);
        portrait.SetPixels32(pixels);
        portrait.Apply(false);
        SetField(window, "portrait", portrait);
        window.title = TitleText;
        window.text = BodyText;
        window.buttonAText = ConfirmationText;
        Dictionary<string, string> resolved = (Dictionary<string, string>)typeof(ColoredText)
            .GetField("cache", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        resolved[BodyText] = BodyText;
        return window;
    }

    private static void CheckLayout(NativeThemeBackend recording)
    {
        NativeThemeDraw title = recording.Draws[TitleText];
        NativeThemeDraw body = recording.Draws[BodyText];
        NativeThemeDraw confirmation = recording.Draws[ConfirmationText];
        Rect content = WindowRect.ContractedBy(ContentPadding);
        Program.Check(title.Anchor == TextAnchor.UpperLeft && title.Rect.yMax < body.Rect.y,
            "叙事者名称未置于正文顶部");
        Program.Check(body.Anchor == TextAnchor.MiddleLeft
            && Mathf.Abs(body.Rect.center.y - WindowRect.center.y) < WindowRect.height * 0.1f,
            "正文未在中部垂直居中");
        Program.Check(confirmation.Anchor == TextAnchor.MiddleCenter
            && confirmation.Rect.xMin > body.Rect.xMin && confirmation.Rect.yMin > body.Rect.yMax
            && confirmation.Rect.xMax >= content.xMax - content.width * 0.05f
            && confirmation.Rect.yMax >= content.yMax - content.height * 0.05f,
            $"确认按钮未置于右下角 {confirmation.Rect} {confirmation.Anchor} 正文 {body.Rect}");
    }

    private static void CheckPortrait(NativeThemeBackend recording)
    {
        NativeThemeTexture portrait = recording.Textures.Single(draw =>
            draw.Texture.width == PortraitWidth && draw.Texture.height == PortraitHeight);
        Rect rect = portrait.Rect;
        const float previousHeight = 192f;
        const float tolerance = 0.1f;
        const float aspectTolerance = 0.001f;
        Program.Check(Mathf.Abs(rect.xMin - (WindowRect.xMin + WindowBorder)) < tolerance,
            $"立绘未贴合左边框内沿 {rect}");
        Program.Check(Mathf.Abs(rect.yMax - (WindowRect.yMax - WindowBorder)) < tolerance,
            $"立绘未贴合下边框内沿 {rect}");
        Program.Check(Mathf.Abs(rect.height - (WindowRect.height - ContentPadding)) < tolerance
            && rect.height > previousHeight, $"立绘未放大 {rect}");
        Program.Check(Mathf.Abs(rect.width / rect.height - (float)PortraitWidth / PortraitHeight) < aspectTolerance,
            "立绘未保持原图比例");
        float occupiedRight = rect.x + rect.width * OpaqueColumns / PortraitWidth;
        float textX = recording.Draws[BodyText].Rect.xMin;
        Program.Check(textX - occupiedRight >= PortraitTextGap - tolerance,
            $"正文进入透明区后遮挡立绘内容 {textX} {occupiedRight}");
        Program.Check(textX < rect.xMax - rect.width * 0.1f,
            $"正文未进入立绘右侧透明区 {textX} {rect.xMax}");
    }

    private static void CheckLongText(NativeThemeBackend recording)
    {
        recording.Reset();
        recording.LongText = BodyText;
        CreateWindow().DoWindowContents(WindowRect);
        CheckText(recording, BodyText);
        CheckText(recording, ConfirmationText);
        Program.Check(recording.ScrollViews == 1, "长正文未使用单个原版滚动区域");
        Program.Check(recording.Draws[BodyText].Anchor == TextAnchor.UpperLeft,
            "长正文未从滚动区顶部显示");
        Program.Check(recording.DrawCalls == 3, "长正文重复了确认按钮");
    }

    private static void CheckText(NativeThemeBackend recording, string text)
    {
        Program.Check(recording.Draws.TryGetValue(text, out NativeThemeDraw draw), "未经过原版文字渲染 " + text);
        Program.Check(draw.Color == new Color32(254, 216, 130, 255), "原版文字渲染颜色不是主题色 " + text);
    }

    private static void SetField(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private sealed class RenderingScope : IDisposable
    {
        private static readonly FieldInfo MainThread = typeof(UnityData)
            .GetField("mainThreadId", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly object previousThread = MainThread.GetValue(null);
        private readonly IGuiBackend previousBackend = GuiBackend.Current;
        private readonly Event previousEvent = Event.current;
        private readonly LoadedLanguage previousLanguage = LanguageDatabase.activeLanguage;
        private readonly Color previousColor = GUI.color;
        private readonly Color previousContentColor = GUI.contentColor;
        private readonly List<ModContentPack> previousMods = LoadedModManager.RunningModsListForReading.ToList();

        public RenderingScope()
        {
            MainThread.SetValue(null, Environment.CurrentManagedThreadId);
            LoadedModManager.RunningModsListForReading.Clear();
            PrepareLanguage();
            PrepareShaderSources();
        }

        public void Dispose()
        {
            MainThread.SetValue(null, previousThread);
            LoadedModManager.RunningModsListForReading.Clear();
            LoadedModManager.RunningModsListForReading.AddRange(previousMods);
            GuiBackend.Current = previousBackend;
            Event.current = previousEvent;
            LanguageDatabase.activeLanguage = previousLanguage;
            GUI.color = previousColor;
            GUI.contentColor = previousContentColor;
        }
    }
}

public class NativeThemeBackend : DispatchProxy
{
    public readonly Dictionary<string, NativeThemeDraw> Draws = new Dictionary<string, NativeThemeDraw>();
    public readonly List<NativeThemeTexture> Textures = new List<NativeThemeTexture>();
    public int ScrollViews;
    public int DrawCalls;
    public string LongText;

    public void Reset()
    {
        Draws.Clear();
        Textures.Clear();
        ScrollViews = 0;
        DrawCalls = 0;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.Name == "DrawText")
        {
            Draws[(string)args[1]] = new NativeThemeDraw
                { Rect = (Rect)args[0], Anchor = (TextAnchor)args[5], Color = (Color)args[7] };
            DrawCalls++;
        }
        if (targetMethod.Name == "PushScrollView") ScrollViews++;
        if (targetMethod.Name == "DrawTexture" || targetMethod.Name == "DrawTextureWithTexCoords")
            Textures.Add(new NativeThemeTexture { Rect = (Rect)args[0], Texture = (Texture)args[1] });
        if (targetMethod.Name == "MeasureText")
            return new Vector2(180f, (string)args[3] == LongText ? 400f : 22f);
        if (targetMethod.Name == "LineSpacing") return 22f;
        if (targetMethod.ReturnType == typeof(void)) return null;
        return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
    }
}

public sealed class NativeThemeDraw
{
    public Rect Rect;
    public TextAnchor Anchor;
    public Color Color;
}

public sealed class NativeThemeTexture
{
    public Rect Rect;
    public Texture Texture;
}
