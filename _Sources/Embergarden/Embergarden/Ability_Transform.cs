using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

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
                    Log.Message("remake turret");
                    Building t = (Building)ThingMaker.MakeThing(Prop.buildingDef);
                    t.SetFaction(parent.pawn.Faction);
                    turretOwner.TryAdd(t);
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
                var pawn = parent.pawn;
                var turretCache = Turret;
                GenSpawn.Spawn(Turret, pawn.Position, pawn.Map);
                pawn.DeSpawn(DestroyMode.WillReplace);

                turretCache.TryGetComp<Comp_TurretTransformable>().pawnOwner.TryAdd(pawn);
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
