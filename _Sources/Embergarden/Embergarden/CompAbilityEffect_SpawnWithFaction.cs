using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Embergarden
{
    public class CompAbilityEffect_SpawnWithFaction : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnWithFaction Props => (CompProperties_AbilitySpawnWithFaction)props;
        protected Thing spawnedThing;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            spawnedThing = ThingMaker.MakeThing(Props.thingDef);
            GenPlace.TryPlaceThing(spawnedThing, target.Cell, parent.pawn.Map, ThingPlaceMode.Near);
            spawnedThing.SetFactionDirect(parent.ConstantCaster.Faction);
            if (Props.sendSkipSignal)
            {
                CompAbilityEffect_Teleport.SendSkipUsedSignal(target, parent.pawn);
            }
            Props.effecter?.Spawn(target.Cell, parent.ConstantCaster.Map);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Cell.Filled(parent.pawn.Map) || (!Props.allowOnBuildings && target.Cell.GetEdifice(parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }
    }

    public class CompProperties_AbilitySpawnWithFaction : CompProperties_AbilitySpawn
    {
        public EffecterDef effecter;

        public CompProperties_AbilitySpawnWithFaction()
        {
            compClass = typeof(CompAbilityEffect_SpawnWithFaction);
        }
    }
}
