using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class DialogueContext : IExposable
    {
        public int Deaths;
        public int Downed;
        public int Colonists;
        public int Waves;
        public int CalmDays;
        public int Day;
        public float Mood;
        public float ThreatScale;

        public static DialogueContext Capture(EmberCycleGame owner, int waves)
        {
            List<Pawn> colonists = LivingColonists().ToList();
            List<Pawn> withMood = colonists.Where(p => p.needs?.mood != null).ToList();
            return new DialogueContext
            {
                Deaths = owner.BattleDeaths,
                Downed = colonists.Count(p => p.Downed),
                Colonists = colonists.Count,
                Waves = waves,
                CalmDays = owner.Schedule.Battle ? 0 :
                    (owner.Schedule.CalmEndTick - Find.TickManager.TicksGame) / CycleTuning.TicksPerDay,
                Day = GenDate.DaysPassedSinceSettle,
                Mood = withMood.Count == 0 ? 0f : withMood.Average(p => p.needs.mood.CurLevelPercentage) * 100f,
                ThreatScale = Find.Storyteller.difficulty.threatScale
            };
        }

        private static IEnumerable<Pawn> LivingColonists()
        {
#if RW15
            return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
#else
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists;
#endif
        }

        public float Value(DialogueMetric metric)
        {
            switch (metric)
            {
                case DialogueMetric.Deaths: return Deaths;
                case DialogueMetric.Downed: return Downed;
                case DialogueMetric.Colonists: return Colonists;
                case DialogueMetric.Waves: return Waves;
                case DialogueMetric.CalmDays: return CalmDays;
                case DialogueMetric.Day: return Day;
                case DialogueMetric.Mood: return Mood;
                case DialogueMetric.ThreatScale: return ThreatScale;
                default: return 0f;
            }
        }

        public string Format(string text)
        {
            return text.Replace("{DEATHS}", Deaths.ToString()).Replace("{DOWNED}", Downed.ToString())
                .Replace("{COLONISTS}", Colonists.ToString()).Replace("{WAVES}", Waves.ToString())
                .Replace("{CALM_DAYS}", CalmDays.ToString()).Replace("{DAY}", Day.ToString())
                .Replace("{MOOD}", Mood.ToString("0", CultureInfo.InvariantCulture))
                .Replace("{THREAT_SCALE}", ThreatScale.ToString("0.##", CultureInfo.InvariantCulture));
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Deaths, "deaths");
            Scribe_Values.Look(ref Downed, "downed");
            Scribe_Values.Look(ref Colonists, "colonists");
            Scribe_Values.Look(ref Waves, "waves");
            Scribe_Values.Look(ref CalmDays, "calmDays");
            Scribe_Values.Look(ref Day, "day");
            Scribe_Values.Look(ref Mood, "mood");
            Scribe_Values.Look(ref ThreatScale, "threatScale");
        }
    }
}
