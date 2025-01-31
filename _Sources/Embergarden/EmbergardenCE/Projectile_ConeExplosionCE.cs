using CombatExtended;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Embergarden;
using System.Reflection;

namespace EmbergardenCE
{
    public class Projectile_ConeExplosionCE : BulletCE
    {
        MethodInfo detonate => typeof(CompExplosive).GetMethod("Detonate", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void Impact(Thing hitThing)
        {
            DefModExtension_ConeExplosion extension = def.GetModExtension<DefModExtension_ConeExplosion>();
            if (extension != null)
            {
                float direction = origin.AngleTo(Destination);
                FloatRange angle = new FloatRange(direction - extension.angle, direction + extension.angle);
                ConeExplosionUtility.DoExplosion(base.Position, Map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, (int)DamageAmount, 1, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: direction, ignoredThings: null, affectedAngle: angle, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor, invert: extension.invert);
            }
            var comps = GetComps<CompExplosive>();
            foreach (var comp in comps)
            {
                detonate.Invoke(comp, new object[] { Map, false });
            }
            base.Impact(hitThing);
        }
    }
}
