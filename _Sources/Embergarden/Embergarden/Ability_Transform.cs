using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Embergarden
{
    public class AbilityCompEffect_Transform : CompAbilityEffect, IThingHolder
    {
        public CompProperties_Transform Prop => (CompProperties_Transform)props;
        public AbilityCompEffect_Transform()
        {
            turretOwner = new ThingOwner<Building>(this, true);
        }
        ThingOwner<Building> turretOwner;

        public Building Turret
        {
            get
            {
                if (!turretOwner.Any)
                {
                    turretOwner.TryAdd((Building)ThingMaker.MakeThing(Prop.buildingDef));
                }
                return turretOwner.InnerListForReading[0];
            }
            set
            {
                if (value.Spawned) value.DeSpawn();
                turretOwner.TryAdd(value);
            }
        }

        public IThingHolder ParentHolder => parent.pawn;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            return;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return turretOwner;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref turretOwner, "turret", [this]);
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Prop.buildingDef != null)
            {
                Turret.SetFaction(parent.pawn.Faction);
                var turretCache = Turret;
                GenSpawn.Spawn(Turret, parent.pawn.Position, parent.pawn.Map);
                parent.pawn.DeSpawn(DestroyMode.WillReplace);
                turretCache.TryGetComp<Comp_TurretTransformable>().innerPawn.TryAdd(parent.pawn);
                Log.Message(Turret.TryGetComp<Comp_TurretTransformable>().innerPawn.Any);
            }
        }
    }
    public class CompProperties_Transform : CompProperties_AbilityEffect
    {
        public CompProperties_Transform()
        {
            compClass = typeof(AbilityCompEffect_Transform);
        }
        public ThingDef buildingDef;
    }
}
