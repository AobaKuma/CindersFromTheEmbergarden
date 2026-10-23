using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class StorytellerCompProperties_EmberFavors : StorytellerCompProperties_CategoryMTB
    {
        public bool allFavorable;
        public List<IncidentDef> favorableIncidents = new List<IncidentDef>();

        public StorytellerCompProperties_EmberFavors()
        {
            compClass = typeof(StorytellerComp_EmberFavors);
        }
    }

    public sealed class StorytellerComp_EmberFavors : StorytellerComp
    {
        private StorytellerCompProperties_EmberFavors Props => (StorytellerCompProperties_EmberFavors)props;

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            if (!EmberCycleGame.IsCalm) yield break;
            if (!Rand.MTBEventOccurs(Props.mtbDays, CycleTuning.TicksPerDay, CycleTuning.CheckInterval))
                yield break;
            IncidentParms parms = GenerateParms(Props.category, target);
            if (!TrySelectRandomIncident(UsableIncidentsInCategory(Props.category, parms),
                out IncidentDef incident, target)) yield break;
            if (IsFavorable(incident)) yield return new FiringIncident(incident, this, parms);
        }

        private bool IsFavorable(IncidentDef incident)
        {
            return Props.allFavorable || incident.letterDef == LetterDefOf.PositiveEvent
                || Props.favorableIncidents.Contains(incident);
        }
    }
}
