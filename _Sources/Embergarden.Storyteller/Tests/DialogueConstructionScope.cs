using System.Reflection;
using System.Runtime.CompilerServices;
using Embergarden.Storytelling;
using UnityEngine;
using Verse;

internal sealed class DialogueConstructionScope : IDisposable
{
    private static readonly FieldInfo MainThread = typeof(UnityData)
        .GetField("mainThreadId", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo Reloaded = typeof(ModContentHolder<Texture2D>)
        .GetField("anyReloaded", BindingFlags.NonPublic | BindingFlags.Static);
    private readonly object previousThread = MainThread.GetValue(null);
    private readonly object previousReloaded = Reloaded.GetValue(null);
    private readonly LoadedLanguage previousLanguage = LanguageDatabase.activeLanguage;
    private readonly List<ModContentPack> previousMods = LoadedModManager.RunningModsListForReading.ToList();
    private readonly List<RimWorld.StorytellerDef> previousDefinitions =
        DefDatabase<RimWorld.StorytellerDef>.AllDefsListForReading.ToList();

    public DialogueConstructionScope()
    {
        MainThread.SetValue(null, Environment.CurrentManagedThreadId);
        Reloaded.SetValue(null, true);
        PrepareLanguage();
        PrepareTextures();
        DefDatabase<RimWorld.StorytellerDef>.Clear();
        DefDatabase<RimWorld.StorytellerDef>.Add(new RimWorld.StorytellerDef
            { defName = CycleTuning.StorytellerName, label = "测试叙事者" });
    }

    private static void PrepareLanguage()
    {
        if (LanguageDatabase.activeLanguage != null) return;
        LoadedLanguage language = (LoadedLanguage)RuntimeHelpers.GetUninitializedObject(typeof(LoadedLanguage));
        language.folderName = "English";
        language.info = new LanguageInfo();
        LanguageDatabase.activeLanguage = language;
    }

    private static void PrepareTextures()
    {
        ModContentPack pack = (ModContentPack)RuntimeHelpers.GetUninitializedObject(typeof(ModContentPack));
        ModContentHolder<Texture2D> holder = new ModContentHolder<Texture2D>(pack);
        foreach (string suffix in new[] { "Calm", "Battle", "BattleUncropped", "Avatar", "BattleAvatar" })
            holder.contentList.Add("UI/Storytellers/EmberWarden" + suffix, new Texture2D(1, 1));
        typeof(ModContentPack).GetField("textures", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(pack, holder);
        LoadedModManager.RunningModsListForReading.Add(pack);
    }

    public void Dispose()
    {
        DefDatabase<RimWorld.StorytellerDef>.Clear();
        DefDatabase<RimWorld.StorytellerDef>.Add(previousDefinitions);
        LoadedModManager.RunningModsListForReading.Clear();
        LoadedModManager.RunningModsListForReading.AddRange(previousMods);
        LanguageDatabase.activeLanguage = previousLanguage;
        Reloaded.SetValue(null, previousReloaded);
        MainThread.SetValue(null, previousThread);
    }
}
