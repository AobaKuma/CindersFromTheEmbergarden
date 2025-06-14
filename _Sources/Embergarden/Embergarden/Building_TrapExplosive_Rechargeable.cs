using RimWorld;
using System.Reflection;
using Verse;

namespace Embergarden
{
    public class Building_TrapExplosive_Rechargeable : Building_TrapExplosive
    {
        int countdown = 0;

        protected Effecter progressBarEffecter;
        MethodInfo detonate => typeof(CompExplosive).GetMethod("Detonate", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void ExposeData()
        {
            Scribe_Values.Look(ref countdown, "countdown");
            base.ExposeData();
        }

        protected override void Tick()
        {
            if (countdown > 0)
            {
                countdown--;

                progressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                mote.progress = (float)countdown / def.building.turretBurstCooldownTime.SecondsToTicks();
                mote.offsetZ = -0.8f;
            }
            else if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }

            base.Tick();
        }

        protected override void SpringSub(Pawn p)
        {
            if (countdown <= 0)
            {
                var comp = GetComp<CompExplosive>();
                comp.AddThingsIgnoredByExplosion([this]);
                detonate.Invoke(comp, [Map, false]);
                if (progressBarEffecter != null)
                {
                    progressBarEffecter.Cleanup();
                    progressBarEffecter = null;
                }
                countdown = def.building.turretBurstCooldownTime.SecondsToTicks();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
            base.Destroy(mode);
        }
    }
}
