using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Embergarden.Storytelling
{
    [StaticConstructorOnStartup]
    public static class StorytellerPortraits
    {
        private static string calmLabel;
        private static string calmDescription;
        private static readonly FieldInfo LabelCapCache = typeof(Def)
            .GetField("cachedLabelCap", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly Texture2D Calm = ContentFinder<Texture2D>.Get("UI/Storytellers/EmberWardenCalm");
        public static readonly Texture2D Battle = ContentFinder<Texture2D>.Get("UI/Storytellers/EmberWardenBattle");
        public static readonly Texture2D BattleDialog = ContentFinder<Texture2D>.Get("UI/Storytellers/EmberWardenBattleUncropped");
        public static readonly Texture2D Avatar = ContentFinder<Texture2D>.Get("UI/Storytellers/EmberWardenAvatar");
        public static readonly Texture2D BattleAvatar = ContentFinder<Texture2D>.Get("UI/Storytellers/EmberWardenBattleAvatar");

        public static void Refresh()
        {
            StorytellerDef def = DefDatabase<StorytellerDef>.GetNamedSilentFail(CycleTuning.StorytellerName);
            if (def == null) return;
            bool battle = EmberCycleGame.Selected && EmberCycleGame.Current?.Schedule.Battle == true;
            if (calmLabel == null)
            {
                calmLabel = def.label;
                calmDescription = def.description;
            }
            def.label = battle
                ? "Cinder_EmberWarden_BattleName".Translate().ToString() : calmLabel;
            def.description = battle
                ? "Cinder_EmberWarden_BattleDescription".Translate().ToString()
                : calmDescription;
            // 清空名称缓存
            LabelCapCache?.SetValue(def, null);
            def.portraitLargeTex = battle ? Battle : Calm;
            def.portraitTinyTex = battle ? BattleAvatar : Avatar;
        }
    }
}
