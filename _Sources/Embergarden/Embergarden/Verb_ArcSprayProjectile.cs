using Verse;

namespace Embergarden
{
    public class Verb_ArcSprayProjectile : Verb_ArcSpray
    {
        protected override void HitCell(IntVec3 cell)
        {
            base.HitCell(cell);
            ((Projectile)GenSpawn.Spawn(verbProps.defaultProjectile, caster.Position, caster.Map, WipeMode.Vanish)).Launch(caster, caster.DrawPos, cell, cell, ProjectileHitFlags.All, false, null, null);
        }
    }
}
