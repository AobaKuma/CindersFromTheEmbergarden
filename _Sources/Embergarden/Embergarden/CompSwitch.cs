using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Embergarden
{
    public class CompProperties_Switch : CompProperties
    {
        public ThingDef changeTo;

        public AbilityDef abilityDef;

        public CompProperties_Switch()
        {
            compClass = typeof(CompSwitch);
        }
    }

    public class CompSwitch : ThingComp, IThingHolder
    {
        public ThingOwner<Thing> innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public CompProperties_Switch Props => (CompProperties_Switch)props;

        public HediffDef hediffDef;

        public int ticks;
        public Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

        public CompSwitch()
        {
            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep, removeContentsIfDestroyed: false);
        }

        public ThingWithComps BaseForm
        {
            get
            {
                if (innerContainer.NullOrEmpty())
                {
                    return null;
                }
                return (ThingWithComps)innerContainer[0];
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (Props.abilityDef != null)
            {
                pawn.abilities.GainAbility(Props.abilityDef);
            }
            if (ticks > 0)
            {
                if (hediffDef != null)
                {
                    HediffWithComps hediffWithComps = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, pawn);
                    hediffWithComps.GetComp<HediffComp_Disappears>().ticksToDisappear = ticks;
                    pawn.health.AddHediff(hediffWithComps);
                }
                ticks = 0;
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (Props.abilityDef != null)
            {
                pawn.abilities.RemoveAbility(Props.abilityDef);
            }
            if (hediffDef != null)
            {
                HediffWithComps hediffWithComps = (HediffWithComps)pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediffWithComps != null)
                {
                    ticks = hediffWithComps.GetComp<HediffComp_Disappears>().ticksToDisappear;
                    pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef));
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            if (ticks > 0)
            {
                text += hediffDef.label + ": " + ticks.ToStringSecondsFromTicks("F0");
            }
            return text;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ticks > 0)
            {
                ticks--;
                if (ticks <= 0)
                {
                    hediffDef = null;
                    if (Holder != null)
                    {
                        Pawn pawn = Holder;
                        pawn.SwitchFormTo(parent);
                    }
                    else
                    {
                        if (parent.Spawned)
                        {
                            Extension.ChangeOldThing(parent);
                        }
                        else
                        {
                            ticks = 1;
                        }
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Defs.Look(ref hediffDef, "hediffDef");
            Scribe_Values.Look(ref ticks, "ticks");
        }
    }

    public class CompAbilityEffect_Switch : CompAbilityEffect
    {
        public Pawn Pawn => parent.pawn;
        public ThingWithComps BaseForm => Pawn.equipment.Primary;
        public ThingDef ChangeTo => BaseForm.GetComp<CompSwitch>().Props.changeTo;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn.ChangeEquipThing(BaseForm);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }

    public class CompAbilityEffect_RemoveHediff : CompAbilityEffect
    {
        public Pawn Pawn => parent.pawn;
        public ThingWithComps BaseForm => Pawn.equipment.Primary;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            CompSwitch comp = BaseForm.GetComp<CompSwitch>();
            if (comp.hediffDef != null)
            {
                HediffWithComps hediffWithComps = (HediffWithComps)Pawn.health.hediffSet.GetFirstHediffOfDef(comp.hediffDef);
                if (hediffWithComps != null)
                {
                    hediffWithComps.GetComp<HediffComp_Disappears>().ticksToDisappear = 0;
                    Pawn.health.RemoveHediff(hediffWithComps);
                }
                else
                {
                    Pawn.SwitchFormTo(BaseForm);
                }
            }
            else
            {
                Pawn.SwitchFormTo(BaseForm);
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }

    public static class Extension
    {
        public static void SwitchFormTo(this Pawn pawn, ThingWithComps thing)
        {
            pawn.ChangeEquipThing(thing);
            CompSwitch comp = pawn.equipment.Primary.GetComp<CompSwitch>();
            Ability ability = pawn.abilities.GetAbility(comp.Props.abilityDef);
            ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
        }

        public static ThingWithComps ChangeThing(ThingDef changeTo)
        {
            ThingDef stuff = null;
            if (changeTo.MadeFromStuff)
            {
                stuff = GenStuff.DefaultStuffFor(changeTo);
            }
            ThingWithComps newThing = (ThingWithComps)ThingMaker.MakeThing(changeTo, stuff);
            return newThing;
        }

        public static void CopyStats(this ThingWithComps newThing, ThingWithComps baseForm)
        {
            if (baseForm.Stuff != null)
            {
                newThing.SetStuffDirect(baseForm.Stuff);
            }
            newThing.HitPoints = baseForm.HitPoints;
            CompQuality quality = newThing.compQuality;
            if (baseForm.TryGetQuality(out QualityCategory q))
            {
                quality.SetQuality(q, null);
            }
            ThingStyleDef styleDef = baseForm.StyleDef;
            if (baseForm.def.randomStyle != null && newThing.def.randomStyle != null)
            {
                ThingStyleChance chance = baseForm.def.randomStyle.Find(x => x.StyleDef == styleDef);
                int index = baseForm.def.randomStyle.IndexOf(chance);
                newThing.StyleDef = newThing.def.randomStyle[index].StyleDef;
            }
        }

        public static void ChangeOldThing(ThingWithComps baseForm)
        {
            CompSwitch compSwitch = baseForm.GetComp<CompSwitch>();
            ThingWithComps newThing;
            if (compSwitch.BaseForm == null)
            {
                newThing = ChangeThing(compSwitch.Props.changeTo);
            }
            else
            {
                newThing = compSwitch.BaseForm;
            }
            compSwitch.innerContainer.Clear();
            compSwitch = newThing.GetComp<CompSwitch>();
            Map map = baseForm.Map;
            baseForm.DeSpawn(DestroyMode.WillReplace);
            compSwitch.innerContainer.TryAdd(baseForm);
            newThing.CopyStats(baseForm);

            IntVec3 intVec3 = baseForm.Position;
            GenSpawn.Spawn(newThing, intVec3, map);
        }

        public static void ChangeEquipThing(this Pawn pawn, ThingWithComps baseForm)
        {
            CompSwitch compSwitch = baseForm.GetComp<CompSwitch>();
            ThingWithComps newThing;
            if (compSwitch.BaseForm == null)
            {
                newThing = ChangeThing(compSwitch.Props.changeTo);
            }
            else
            {
                newThing = compSwitch.BaseForm;
            }
            compSwitch.innerContainer.Clear();
            compSwitch = newThing.GetComp<CompSwitch>();
            compSwitch.innerContainer.TryAddOrTransfer(baseForm);
            newThing.CopyStats(baseForm);

            baseForm.Notify_Unequipped(pawn);
            pawn.equipment.MakeRoomFor(newThing);
            pawn.equipment.AddEquipment(newThing);
        }
    }

    public class HediffCompPropertiesSwitch : HediffCompProperties
    {
        public HediffCompPropertiesSwitch()
        {
            compClass = typeof(SwitchWhenDisappear);
        }
    }

    public class SwitchWhenDisappear : HediffComp
    {
        public HediffCompPropertiesSwitch Props => (HediffCompPropertiesSwitch)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            CompSwitch compSwitch = Pawn.equipment.Primary.GetComp<CompSwitch>();
            compSwitch.hediffDef = parent.def;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (parent.ShouldRemove)
            {
                ThingWithComps thing = Pawn.equipment.Primary;
                if (thing != null)
                {
                    Pawn.SwitchFormTo(thing);
                }
            }
        }
    }
}