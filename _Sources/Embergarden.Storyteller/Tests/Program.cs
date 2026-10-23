using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Verse;
using Embergarden.Storytelling;

internal static class Program
{
    private static int Main(string[] args)
    {
        string gameRoot = Path.GetFullPath(args[0]);
        AssemblyLoadContext.Default.Resolving += (_, name) => Resolve(gameRoot, name);
        return Run();
    }

    private static Assembly Resolve(string root, AssemblyName name)
    {
        string path = Path.Combine(root, name.Name + ".dll");
        return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Run()
    {
        try
        {
            InitializePreferences();
            InitializeModTypes();
            ScheduleTests.Run();
            BattleEntryTests.Run();
            DialogueTests.Run();
            NativeDialogueTests.Run();
            NativeThemeTests.Run();
            DefinitionTests.Run();
            LocalizationTests.Run();
            CradleRaidTimingTests.Run();
            CradleSelectionTests.Run();
            ScribeTests.Run();
            PatchBindingTests.Run();
            Console.WriteLine("[CinderStory] 所有测试通过");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    public static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void InitializePreferences()
    {
        // 隔离原版私有偏好数据
        typeof(Prefs).GetField("data", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, new PrefsData());
    }

    private static void InitializeModTypes()
    {
        // 隔离图形与用户配置加载
        ModContentPack pack = (ModContentPack)RuntimeHelpers.GetUninitializedObject(typeof(ModContentPack));
        pack.assemblies = new ModAssemblyHandler(pack);
        pack.assemblies.loadedAssemblies.Add(typeof(CycleTuning).Assembly);
        LoadedModManager.RunningModsListForReading.Add(pack);
    }
}
