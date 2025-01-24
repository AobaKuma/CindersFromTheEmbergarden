using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Embergarden
{
    public class Building_TrapTentacle : Building
    {

        ModExtension_TentacleTrap ext => def.GetModExtension<ModExtension_TentacleTrap>();

        private List<Pawn> touchingPawns = new List<Pawn>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref touchingPawns, "testees", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && touchingPawns.RemoveAll((Pawn x) => x == null) != 0)
            {
                Log.Error("Removed null pawns from touchingPawns.");
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                map.terrainGrid.SetTerrain(Position, ext.terrainDef);
            }
        }

        public override void Tick()
        {
            if (base.Spawned)
            {
                List<Thing> thingList = base.Position.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn pawn && !touchingPawns.Contains(pawn))
                    {
                        touchingPawns.Add(pawn);
                        CheckSpring(pawn);
                    }
                }
                for (int j = 0; j < touchingPawns.Count; j++)
                {
                    Pawn pawn2 = touchingPawns[j];
                    if (pawn2 == null || !pawn2.Spawned || pawn2.Position != base.Position)
                    {
                        touchingPawns.Remove(pawn2);
                    }
                }
            }
            base.Tick();
        }

        private void CheckSpring(Pawn p)
        {
            if (Rand.Chance(SpringChance(p)))
            {
                Spring(p);
            }
        }

        protected virtual float SpringChance(Pawn p)
        {
            float num = 1f;
            if (p.kindDef.immuneToTraps)
            {
                return 0f;
            }
            if (KnowsOfTrap(p))
            {
                if (p.Faction != null)
                {
                    num = ((p.Faction != base.Faction) ? 0f : 0.005f);
                }
                else if (p.IsNonMutantAnimal)
                {
                    num = 0.2f;
                    num *= def.building.trapPeacefulWildAnimalsSpringChanceFactor;
                }
                else
                {
                    num = 0.3f;
                }
            }
            num *= this.GetStatValue(StatDefOf.TrapSpringChance) * p.GetStatValue(StatDefOf.PawnTrapSpringChance);
            return Mathf.Clamp01(num);
        }

        public bool KnowsOfTrap(Pawn p)
        {
            if (p.Faction != null && !p.Faction.HostileTo(base.Faction))
            {
                return true;
            }
            if (p.Faction == null && p.IsNonMutantAnimal && !p.InAggroMentalState)
            {
                return true;
            }
            if (p.guest != null && p.guest.Released)
            {
                return true;
            }
            if (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction)
            {
                return true;
            }
            if (p.RaceProps.Humanlike && p.IsFormingCaravan())
            {
                return true;
            }
            if (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction)
            {
                return true;
            }
            if (p.Faction == null && p.RaceProps.Humanlike)
            {
                return true;
            }
            return false;
        }

        public override ushort PathFindCostFor(Pawn p)
        {
            if (!KnowsOfTrap(p))
            {
                return 0;
            }
            return 800;
        }

        public override ushort PathWalkCostFor(Pawn p)
        {
            if (!KnowsOfTrap(p))
            {
                return 0;
            }
            return 40;
        }

        public override bool IsDangerousFor(Pawn p)
        {
            return KnowsOfTrap(p);
        }

        public void Spring(Pawn p)
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

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Map.terrainGrid.TerrainAt(Position) == ext.terrainDef) Map.terrainGrid.Notify_TerrainDestroyed(Position);
            base.DeSpawn(mode);
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
