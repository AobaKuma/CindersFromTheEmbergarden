﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace Embergarden
{
    public class CompTentacleSpreader : ThingComp
    {
        public CompProperties_TentacleSpreader Props => props as CompProperties_TentacleSpreader;

        public Effecter effecter;

        public float pct = 0;
        protected Thing spawnedThing;

        public override void CompTick()
        {
            base.CompTick();
            IncrementTime(1);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SpawnThing(parent.Position);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            IncrementTime(GenTicks.TickRareInterval);
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            IncrementTime(GenTicks.TickLongInterval);
        }

        public void IncrementTime(int delta)
        {
            int oldRad = GenRadial.NumCellsInRadius(Props.maxRadius * pct);
            pct += (float)delta / Props.durationTicks;
            if (pct > 1) pct = 1;
            int newRad = GenRadial.NumCellsInRadius(Props.maxRadius * pct);

            if (parent.Spawned && parent.MapHeld == Find.CurrentMap)
            {
                effecter ??= Props.effecterDef.SpawnAttached(parent, parent.MapHeld);
                effecter?.EffectTick(parent, parent);
            }
            else
            {
                effecter?.Cleanup();
                effecter = null;
            }
            for (int i = oldRad; i < newRad; i++)
            {
                IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
                if (!intVec.InBounds(parent.Map)) continue;
                if (parent.Map.thingGrid.ThingsListAtFast(intVec).Select(t => t.def).Contains(Props.spreadedThingDef)) continue;
                if (intVec.Filled(parent.Map)) continue;
                SpawnThing(intVec);
            }

            if (pct >= 1)
            {
                parent.Destroy();
                effecter?.Cleanup();
                effecter = null;
            }
        }

        void SpawnThing(IntVec3 cell)
        {
            spawnedThing ??= ThingMaker.MakeThing(Props.spreadedThingDef);
            GenPlace.TryPlaceThing(spawnedThing, cell, parent.Map, ThingPlaceMode.Direct);
            spawnedThing.SetFactionDirect(parent.Faction);
            spawnedThing = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref pct, "pct");
        }
    }

    public class CompProperties_TentacleSpreader : CompProperties
    {
        public CompProperties_TentacleSpreader()
        {
            compClass = typeof(CompTentacleSpreader);
        }

        public ThingDef spreadedThingDef;

        public EffecterDef effecterDef;

        public float maxRadius = 5;

        public int durationTicks = 60;
    }
}
