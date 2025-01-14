using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Embergarden
{
    public class AbilityCompEffect_Transform : CompAbilityEffect
    {
        public CompProperties_Transform Prop => (CompProperties_Transform)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Prop.buildingDef != null)
            {
                Pawn pawn = parent.pawn;
                var map = pawn.Map;
                var p = pawn.Position;
                Building building = (Building)ThingMaker.MakeThing(Prop.buildingDef);
                pawn.DeSpawn(DestroyMode.WillReplace);
                building.TryGetComp<Comp_TurretTransformable>()?.GetDirectlyHeldThings()?.TryAdd(pawn);
                building.SetFaction(pawn.Faction);
                GenSpawn.Spawn(building, p, map);
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
