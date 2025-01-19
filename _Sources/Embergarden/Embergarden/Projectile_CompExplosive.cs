using RimWorld;
using System.Reflection;
using Verse;

namespace Embergarden
{
    public class Projectile_CompExplosive : Bullet
    {
        MethodInfo detonate => typeof(CompExplosive).GetMethod("Detonate", BindingFlags.Instance | BindingFlags.NonPublic);
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != null)
            {
                this.Position = hitThing.TrueCenter().ToIntVec3();
            }
            var comps = this.GetComps<CompExplosive>();
            foreach (var comp in comps)
            {
                detonate.Invoke(comp, [Map]);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }
}
