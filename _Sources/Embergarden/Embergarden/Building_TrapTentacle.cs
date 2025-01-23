using RimWorld;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace Embergarden
{
    public class Building_TrapTentacle : Building_Trap
    {
        ModExtension_TentacleTrap ext => def.GetModExtension<ModExtension_TentacleTrap>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                map.terrainGrid.SetTerrain(Position, ext.terrainDef);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Map.terrainGrid.TerrainAt(Position) == ext.terrainDef) Map.terrainGrid.Notify_TerrainDestroyed(Position);
            base.DeSpawn(mode);
        }

        protected override void SpringSub(Pawn p)
        {
            if (p == null)
            {
                return;
            }
            if (p.health.hediffSet.HasHediff(ext.hediffDef)) return;
            ext.soundDef?.PlayOneShot(p);
            Hediff hediff = HediffMaker.MakeHediff(ext.hediffDef, p, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.ticksToDisappear = ext.durationSec.SecondsToTicks();
            }
            p.health.AddHediff(hediff);
        }
    }

    public class ModExtension_TentacleTrap : DefModExtension
    {
        public TerrainDef terrainDef;

        public HediffDef hediffDef;

        public SoundDef soundDef;

        public float durationSec;
    }
}
