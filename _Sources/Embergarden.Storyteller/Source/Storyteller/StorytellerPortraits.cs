using RimWorld;
using UnityEngine;
using Verse;

namespace Embergarden.Storytelling
{
    [StaticConstructorOnStartup]
    public static class StorytellerPortraits
    {
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
            def.portraitLargeTex = battle ? Battle : Calm;
            def.portraitTinyTex = battle ? BattleAvatar : Avatar;
        }
    }
}
