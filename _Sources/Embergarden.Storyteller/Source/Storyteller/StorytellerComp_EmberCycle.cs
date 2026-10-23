using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class StorytellerCompProperties_EmberCycle : StorytellerCompProperties
    {
        public List<IncidentDef> battleIncidents = new List<IncidentDef>();

        public StorytellerCompProperties_EmberCycle()
        {
            compClass = typeof(StorytellerComp_EmberCycle);
        }
    }

    public sealed class StorytellerComp_EmberCycle : StorytellerComp
    {
        private StorytellerCompProperties_EmberCycle Props => (StorytellerCompProperties_EmberCycle)props;

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            EmberCycleGame.Current?.Advance(this);
            yield break;
        }

        public bool TryFireThreat(Map map, bool requireFactionRaid, Faction excludedFaction,
            out Faction firedFaction)
        {
            firedFaction = null;
            IncidentParms parms = GenerateParms(IncidentCategoryDefOf.ThreatBig, map);
            List<IncidentDef> candidates = FindCandidates(parms, requireFactionRaid);
            if (candidates.Count == 0)
            {
                RaidGenerationLog.Failure("no-candidates", $"没有可执行事件 点数 {parms.points:0.##}");
                return false;
            }
            if (!TrySelectRandomIncident(candidates, out IncidentDef incident, map)) return false;
            if (incident.Worker is IncidentWorker_RaidEnemy raid
                && !CradleRaidSelection.TryPrepare(raid, parms, excludedFaction))
            {
                RaidGenerationLog.Failure($"faction-{incident.defName}",
                    $"事件 {incident.defName} 没有合法袭击派系");
                return false;
            }
            bool success = Find.Storyteller.TryFire(new FiringIncident(incident, this, parms));
            if (success) firedFaction = parms.faction;
            if (success)
                RaidGenerationLog.Success($"事件 {incident.defName} 派系 {parms.faction?.def?.defName ?? "无"}");
            else
                RaidGenerationLog.Failure($"execute-{incident.defName}",
                    $"事件 {incident.defName} 通过预检但执行失败");
            return success;
        }

        private List<IncidentDef> FindCandidates(IncidentParms parms, bool requireFactionRaid)
        {
            List<IncidentDef> candidates = new List<IncidentDef>();
            if (Props.battleIncidents.NullOrEmpty())
            {
                RaidGenerationLog.Failure("empty-config", "战斗事件列表为空");
                return candidates;
            }
            foreach (IncidentDef incident in Props.battleIncidents)
            {
                if (incident == null) continue;
                if (requireFactionRaid && incident.Worker is not IncidentWorker_RaidEnemy)
                {
                    RaidGenerationLog.Failure($"non-raid-{incident.defName}",
                        $"双波跳过非敌对袭击事件 {incident.defName}");
                    continue;
                }
                if (incident.Worker.CanFireNow(parms)) candidates.Add(incident);
                else
                    RaidGenerationLog.Failure($"can-fire-{incident.defName}",
                        $"原版 CanFireNow 拒绝 {incident.defName} 早期天数 {incident.earliestDay:0.##} "
                        + $"威胁点范围 {incident.minThreatPoints:0.##}-{incident.maxThreatPoints:0.##}");
            }
            return candidates;
        }
    }
}
