using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                return turretOwner.Any ? turretOwner.InnerListForReading[0] : null;
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

        public static bool TryFindCellNearWith(IntVec3 near, Predicate<IntVec3> validator, Map map, out IntVec3 result)
        {
            result = near;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(near, 56, true))
            {
                if (validator(cell))
                {
                    result = cell;
                    return true;
                }
            }
            return false;
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Prop.buildingDef != null)
            {
                if (Turret == null)
                {
                    Building t = (Building)ThingMaker.MakeThing(Prop.buildingDef);
                    t.SetFaction(parent.pawn.Faction);
                    turretOwner.TryAdd(t);
                }

                var pawn = parent.pawn;
                var turretCache = Turret;
                Map map = pawn.Map;

                if (TryFindCellNearWith(pawn.Position, c => GenSpawn.CanSpawnAt(Turret.def, c, map), map, out var result))
                    {
                        GenPlace.TryPlaceThing(Turret, result, map, ThingPlaceMode.Direct);
                    }

                pawn.DeSpawn(DestroyMode.WillReplace);

                turretCache.TryGetComp<Comp_TurretTransformableAbstract>().pawnOwner.TryAdd(pawn);
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

    public class CompTransformWhenDowned : ThingComp
    {
        Ability ability;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if ((!parent.Faction?.IsPlayer ?? true) || parent is not Pawn p) yield break;

            if (ability == null)
            {
                var abilitys = p.abilities?.abilities.Where(a => a.comps.ContainsAny(c => c is AbilityCompEffect_Transform));
                if (abilitys.Any())
                {
                    ability = abilitys.First();
                }
            }
            if (p.Downed && ability != null)
            {
                Command_Action command = new()
                {
                    defaultLabel = ability.def.label,
                    defaultDesc = ability.def.description,
                    icon = ability.def.uiIcon,
                    action = delegate { ability.Activate(parent, parent); },
                };
                yield return command;
            }
        }
    }
}
