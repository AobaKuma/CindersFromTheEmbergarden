using System.Diagnostics;
using UnityEngine;
using Verse;
using static RimWorld.Sketch;

namespace Embergarden
{
    public class Setting : ModSettings
    {
        public bool CommaEyes = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref CommaEyes, "CommaEyes");
        }
    }

    public class CinderMod : Mod
    {
        public static Setting settings;
        public static bool CommaEyes => settings.CommaEyes;

        public CinderMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 17f) / 2f;
            listing_Standard.Begin(inRect);
            Text.Font = GameFont.Small;
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("Cinder_CommaEyesLabel".Translate(), ref settings.CommaEyes, "Cinder_CommaEyesDesc".Translate());
            listing_Standard.End();
        }
    }
}
