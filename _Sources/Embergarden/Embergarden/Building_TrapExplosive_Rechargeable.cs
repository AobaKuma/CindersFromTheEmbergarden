using RimWorld;
using Verse;

namespace Embergarden
{
    public class Building_TrapExplosive_Rechargeable : Building_TrapExplosive
    {
        int countdown = 0;

        protected Effecter progressBarEffecter;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref countdown, "countdown");
            base.ExposeData();
        }

        public override void Tick()
        {
            if (countdown > 0)
            {
                countdown--;

                progressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();
                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                mote.progress = countdown / def.building.turretBurstCooldownTime;
                mote.offsetZ = -0.8f;

                Log.Message(mote.progress);
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
                if (progressBarEffecter != null)
                {
                    progressBarEffecter.Cleanup();
                    progressBarEffecter = null;
                }
                countdown = def.building.turretBurstCooldownTime.SecondsToTicks();

                base.SpringSub(p);
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
