using Verse;
using Verse.Noise;

namespace Embergarden
{
    public class Projectile_ConeExplosion : Projectile_Explosive
    {
        protected override void Explode()
        {
            DefModExtension_ConeExplosion extension = def.GetModExtension<DefModExtension_ConeExplosion>();
            if (extension == null) base.Explode();
            else
            {
                float direction = origin.AngleToFlat(destination);
                FloatRange angle = new FloatRange(direction - extension.angle, direction + extension.angle);
                ConeExplosionUtility.DoExplosion(base.Position, Map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: direction, ignoredThings: null, affectedAngle: angle, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor, invert: extension.invert);
            }
            Destroy();
        }
    }
}
