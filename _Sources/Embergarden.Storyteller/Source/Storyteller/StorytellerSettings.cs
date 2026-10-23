using UnityEngine;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class StorytellerSettings : ModSettings
    {
        public int CalmMinDays = CycleTuning.CalmMinDays;
        public int CalmMaxDays = CycleTuning.CalmMaxDays;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref CalmMinDays, "calmMinDays", CycleTuning.CalmMinDays);
            Scribe_Values.Look(ref CalmMaxDays, "calmMaxDays", CycleTuning.CalmMaxDays);
            Normalize();
        }

        public void Normalize()
        {
            CalmMinDays = Mathf.Clamp(CalmMinDays, CycleTuning.SettingsMinDays, CycleTuning.SettingsMaxDays);
            CalmMaxDays = Mathf.Clamp(CalmMaxDays, CalmMinDays, CycleTuning.SettingsMaxDays);
        }

        public void Draw(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            listing.Label("Cinder_StorySettingsIntro".Translate());
            listing.GapLine();
            listing.Label("Cinder_CalmMinDays".Translate(CalmMinDays));
            CalmMinDays = Mathf.RoundToInt(listing.Slider(CalmMinDays,
                CycleTuning.SettingsMinDays, CycleTuning.SettingsMaxDays));
            Normalize();
            if (CalmMinDays < CycleTuning.ShortCalmPromptThreshold)
                listing.Label("Cinder_CalmVeryShortPrompt".Translate());
            listing.Label("Cinder_CalmMaxDays".Translate(CalmMaxDays));
            CalmMaxDays = Mathf.RoundToInt(listing.Slider(CalmMaxDays,
                CalmMinDays, CycleTuning.SettingsMaxDays));
            if (listing.ButtonText("Cinder_ResetStorySettings".Translate())) Reset();
            listing.End();
        }

        private void Reset()
        {
            CalmMinDays = CycleTuning.CalmMinDays;
            CalmMaxDays = CycleTuning.CalmMaxDays;
        }
    }

    public sealed class CinderStorytellerMod : Mod
    {
        public static StorytellerSettings Settings;

        public CinderStorytellerMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<StorytellerSettings>();
        }

        public override string SettingsCategory() => "Cinder_StorySettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect) => Settings.Draw(inRect);
    }
}
