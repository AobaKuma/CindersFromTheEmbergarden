using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Embergarden
{
    public static class ConeExplosionUtility
    {
        public static void DoExplosion(IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, float armorPenetration = -1f, SoundDef explosionSound = null, ThingDef weapon = null, ThingDef projectile = null, Thing intendedTarget = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, GasType? postExplosionGasType = null, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool damageFalloff = false, float? direction = null, List<Thing> ignoredThings = null, FloatRange? affectedAngle = null, bool doVisualEffects = true, float propagationSpeed = 1f, float excludeRadius = 0f, bool doSoundEffects = true, ThingDef postExplosionSpawnThingDefWater = null, float screenShakeFactor = 1f, bool invert = false)
        {
            if (affectedAngle == null)
            {
                return;
            }
            FloatRange affectedAngles = (FloatRange)affectedAngle;

            if (invert)
            {
                affectedAngles = new FloatRange(affectedAngles.max - 180, affectedAngles.min + 180);
            }

            GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, armorPenetration, explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings, affectedAngles, doVisualEffects, propagationSpeed, excludeRadius, doSoundEffects, postExplosionSpawnThingDefWater, screenShakeFactor);
            if (affectedAngles.max > 180)
            {
                affectedAngles.max = -360 + affectedAngles.max;
                affectedAngles.min = -180;
                GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, armorPenetration, explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings, affectedAngles, doVisualEffects, propagationSpeed, excludeRadius, doSoundEffects, postExplosionSpawnThingDefWater, screenShakeFactor);
            }
            if (affectedAngles.min < -180)
            {
                affectedAngles.min = 360 + affectedAngles.min;
                affectedAngles.max = 180;
                GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, armorPenetration, explosionSound, weapon, projectile, intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, direction, ignoredThings, affectedAngles, doVisualEffects, propagationSpeed, excludeRadius, doSoundEffects, postExplosionSpawnThingDefWater, screenShakeFactor);
            }
        }
        public static void RenderPredictedAreaOfEffect(IntVec3 loc, float radius, float angle, Vector3 direction)
        {
            int num = GenRadial.NumCellsInRadius(radius);
            List<IntVec3> affectedCells = new List<IntVec3>();
            for (int i = 1; i < num; i++)
            {
                IntVec3 intVec = loc + GenRadial.RadialPattern[i];
                Vector3 vec3 = (loc - intVec).ToVector3();
                vec3.y = 0;
                if (Math.Abs(Vector3.Angle(direction, vec3)) < angle)
                {
                    affectedCells.Add(intVec);
                }
            }
            GenDraw.DrawFieldEdges(affectedCells);
        }

        public static float RWangleToCEangle(float angle)
        {
            return -angle - 90;
        }

        public static float CEangleToRWangle(float angle)
        {
            return -(angle + 90);
        }
    }

    public class DefModExtension_ConeExplosion : DefModExtension
    {
        public float angle = 30;
        public bool invert = false;
    }
}
