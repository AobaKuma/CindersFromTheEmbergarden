using RimWorld;
using Verse;
#if RW15
using HarmonyLib;
using System.Reflection;
#endif

namespace Embergarden.Storytelling
{
    public static class CradleRaidSelection
    {
#if RW15
        // 旧版私有筛选无公开入口
        private static readonly MethodInfo FactionFilter = AccessTools.Method(typeof(IncidentWorker_RaidEnemy),
            "FactionCanBeGroupSource", new[] { typeof(Faction), typeof(Map), typeof(bool) });
#endif

        public static bool TryPrepare(IncidentWorker_RaidEnemy raid, IncidentParms parms,
            Faction excludedFaction)
        {
            bool cradleOnly = Rand.Chance(CycleTuning.CradleSelectionChance);
            if (!TryFindFaction(raid, parms, excludedFaction, cradleOnly, out Faction faction)
                && !TryFindFaction(raid, parms, excludedFaction, !cradleOnly, out faction)) return false;
            parms.faction = faction;
            return true;
        }

        private static bool TryFindFaction(IncidentWorker_RaidEnemy raid, IncidentParms parms,
            Faction excludedFaction, bool cradleOnly, out Faction faction)
        {
            faction = null;
            return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted(parms,
                out faction, f => CanSelectPairFaction(f.def,
                    excludedFaction?.def, cradleOnly)
                    && CanSelect(raid, f, parms), allowHidden: true);
        }

        public static bool CanSelectPairFaction(FactionDef candidate, FactionDef excluded, bool cradleOnly)
        {
            if (candidate == null || (excluded != null && candidate == excluded)) return false;
            return (candidate.defName == CycleTuning.CradleFactionName) == cradleOnly;
        }

        private static bool CanSelect(IncidentWorker_RaidEnemy raid, Faction faction, IncidentParms parms)
        {
#if RW15
            return (bool)FactionFilter.Invoke(raid, new object[] { faction, (Map)parms.target, false });
#else
            return raid.FactionCanBeGroupSource(faction, parms);
#endif
        }
    }
}
