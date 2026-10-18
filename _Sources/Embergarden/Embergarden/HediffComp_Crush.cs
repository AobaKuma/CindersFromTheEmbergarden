using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Embergarden
{
    public class HediffComp_Crush : HediffComp
    {
        private HediffCompProperties_Crush Props => (HediffCompProperties_Crush)props;

        private bool Active
        {
            get
            {
                if (Pawn.Dead || !Pawn.Spawned || Pawn.Downed || Pawn.stances.stunner.Stunned)
                {
                    return false;
                }
                CompCanBeDormant dormant = Pawn.TryGetComp<CompCanBeDormant>();
                if (dormant != null && !dormant.Awake) return false;
                if (!Pawn.Awake()) return false;
                return Pawn.Faction == null || !Pawn.Faction.IsPlayer || Pawn.Drafted;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(Math.Max(1, Props.intervalTicks)) || !Active)
            {
                return;
            }

            Map map = Pawn.Map;
            SpawnFlecks(map);

            List<Thing> targets = new List<Thing>();
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(Pawn.Position, map, Props.radius, true))
            {
                if (thing != Pawn && thing.HostileTo(Pawn)
                    && ((thing is Pawn victim && !victim.Dead) || thing is Building))
                {
                    targets.Add(thing);
                }
            }

            foreach (Thing target in targets)
            {
                if (target.Destroyed || !target.Spawned || target.Map != map) continue;
                if (target is Pawn deadPawn && deadPawn.Dead) continue;

                if (Props.damageAmount > 0f)
                {
                    target.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount,
                        Props.armorPenetration, instigator: Pawn, intendedTarget: target));
                }

                if (target is Pawn victim && !victim.Dead && !victim.Destroyed)
                {
                    ApplyHediff(victim);
                    if (Props.stunTicks > 0 && !victim.Downed)
                    {
                        victim.TakeDamage(new DamageInfo(DamageDefOf.Stun, Props.stunTicks / 30f,
                            instigator: Pawn, intendedTarget: victim));
                    }
                }
            }
        }

        private void ApplyHediff(Pawn victim)
        {
            if (Props.appliedHediff == null) return;

            Hediff hediff = victim.health.hediffSet.GetFirstHediffOfDef(Props.appliedHediff);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(Props.appliedHediff, victim);
                hediff.Severity = Props.appliedSeverity;
                victim.health.AddHediff(hediff);
            }

            HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.SetDuration(Math.Max(disappears.ticksToDisappear, Props.appliedDurationTicks));
            }
        }

        private void SpawnFlecks(Map map)
        {
            if (Props.fleckDef == null || Props.fleckCount <= 0) return;

            for (int i = 0; i < Props.fleckCount; i++)
            {
                float angle = (i + Rand.Value) * 360f / Props.fleckCount;
                Vector3 offset = new Vector3(0f, 0f, Props.fleckRadius * Rand.Range(0.35f, 1f)).RotatedBy(angle);
                Vector3 position = Pawn.DrawPos + offset;
                if (!position.ToIntVec3().InBounds(map) || !position.ShouldSpawnMotesAt(map)) continue;

                FleckCreationData fleck = FleckMaker.GetDataStatic(position, map, Props.fleckDef, Props.fleckScale);
                fleck.rotationRate = Rand.Range(-60f, 60f);
                fleck.velocityAngle = angle;
                fleck.velocitySpeed = Rand.Range(0.6f, 0.75f);
                map.flecks.CreateFleck(fleck);
            }
        }
    }

    public class HediffCompProperties_Crush : HediffCompProperties
    {
        public float radius = 1.9f;
        public int intervalTicks = 60;
        public DamageDef damageDef;
        public float damageAmount = 15f;
        public float armorPenetration = 0.5f;
        public HediffDef appliedHediff;
        public float appliedSeverity = 0.1f;
        public int appliedDurationTicks = 6000;
        public int stunTicks = 30;
        public FleckDef fleckDef;
        public int fleckCount = 8;
        public float fleckScale = 1.2f;
        public float fleckRadius = 1.9f;

        public HediffCompProperties_Crush()
        {
            compClass = typeof(HediffComp_Crush);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef)) yield return error;
            if (radius <= 0f) yield return "radius must be positive";
            if (intervalTicks <= 0) yield return "intervalTicks must be positive";
            if (damageDef == null) yield return "damageDef must be specified";
            if (damageAmount < 0f) yield return "damageAmount must not be negative";
            if (armorPenetration < 0f) yield return "armorPenetration must not be negative";
            if (appliedHediff != null && appliedSeverity <= 0f) yield return "appliedSeverity must be positive";
            if (appliedDurationTicks <= 0) yield return "appliedDurationTicks must be positive";
            if (stunTicks < 0) yield return "stunTicks must not be negative";
            if (fleckCount < 0) yield return "fleckCount must not be negative";
            if (fleckScale <= 0f) yield return "fleckScale must be positive";
            if (fleckRadius < 0f) yield return "fleckRadius must not be negative";
        }
    }

}
