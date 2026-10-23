using HarmonyLib;
using UnityEngine;
using Verse;

namespace Embergarden.Storytelling
{
    public static class EmberDialogueTheme
    {
        public static readonly Color TextColor = new Color32(254, 216, 130, 255);
        public static readonly Color BorderColor = new Color32(135, 130, 114, 255);
        public static readonly Color BackgroundColor = new Color32(76, 67, 50, 255);

        public static bool Active => Verse.Current.Game != null
            && Find.WindowStack?.currentlyDrawnWindow is Dialog_EmberPhase;

        public static void DrawBackground(Rect rect)
        {
            Color previous = GUI.color;
            try
            {
                GUI.color = BackgroundColor;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
                GUI.color = BorderColor;
                Widgets.DrawBox(rect);
            }
            finally
            {
                GUI.color = previous;
            }
        }
    }

    // 原版底板无配色入口
    [HarmonyPatch(typeof(Widgets), nameof(Widgets.DrawWindowBackground), typeof(Rect))]
    public static class EmberDialogueBackgroundPatch
    {
        public static bool Prefix(Rect rect)
        {
            if (!EmberDialogueTheme.Active) return true;
            EmberDialogueTheme.DrawBackground(rect);
            return false;
        }
    }

}
