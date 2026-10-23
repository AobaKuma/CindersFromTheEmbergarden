using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    public sealed class BattleEncounter : IExposable
    {
        private Map map;
        private HashSet<int> existingThings = new HashSet<int>();

        public BattleEncounter() { }

        public BattleEncounter(Map target)
        {
            map = target;
            existingThings = new HashSet<int>(map.listerThings.AllThings.Select(t => t.thingIDNumber));
        }

        public bool IsClear(int now)
        {
            if (map == null || !Find.Maps.Contains(map)) return true;
            if (map.dangerWatcher.DangerRating != StoryDanger.None) return false;
            return !map.listerThings.AllThings.Any(t => !existingThings.Contains(t.thingIDNumber) && IsBlocking(t));
        }

        private static bool IsBlocking(Thing thing)
        {
            if (thing.Destroyed || !thing.Spawned) return false;
            if (thing is Skyfaller || thing is TunnelHiveSpawner || IsOpeningPod(thing)) return true;
            if (thing is Pawn pawn) return !pawn.Dead && !pawn.Downed && pawn.HostileTo(Faction.OfPlayer);
            if (thing is Hive) return true;
            return thing.HostileTo(Faction.OfPlayer)
                && (thing is Building_Turret || thing.def.isMechClusterThreat);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Collections.Look(ref existingThings, "existingThings", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && existingThings == null)
                existingThings = new HashSet<int>();
        }

        private static bool IsOpeningPod(Thing thing)
        {
#if RW15
            return thing is ActiveDropPod;
#else
            return thing is ActiveTransporter;
#endif
        }
    }
}
