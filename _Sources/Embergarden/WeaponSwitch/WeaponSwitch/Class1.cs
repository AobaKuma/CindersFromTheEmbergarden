using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WeaponSwitch
{
    public static class Extensions
    {
        public static void SwitchFormTo(this Pawn pawn,ThingWithComps thing,CompSwitch comp)
        {
            pawn.ChangeEquipThing(thing, comp.Props.changeTo);
            comp = pawn.equipment.Primary.GetComp<CompSwitch>();
            Ability ability = pawn.abilities.GetAbility(comp.Props.abilityDef);
            ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
        }
    }

    public class CompProperties_Switch : CompProperties
    {
        public ThingDef changeTo;

        public AbilityDef abilityDef;

        public CompProperties_Switch()
        {
            compClass = typeof(CompSwitch);
        }
    }

    public class CompSwitch : ThingComp
    {
        public CompProperties_Switch Props => (CompProperties_Switch)props;

        public HediffDef hediffDef;
        public int ticks;
        public Pawn Holder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

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
                        pawn.SwitchFormTo(parent, this);
                    }
                    else
                    {
                        if (parent.Spawned)
                        {
                            Extension.ChangeOldThing(parent, Props.changeTo);
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
            Pawn.ChangeEquipThing(BaseForm, ChangeTo);
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
                    Pawn.SwitchFormTo(BaseForm, BaseForm.GetComp<CompSwitch>());
                }
            }
            else
            {
                Pawn.SwitchFormTo(BaseForm,BaseForm.GetComp<CompSwitch>());
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }

    public static class Extension
    {
        public static ThingWithComps ChangeThing(ThingWithComps baseForm, ThingDef changeTo)
        {
            int hitPoints = baseForm.HitPoints;
            ThingDef stuff = null;
            if (baseForm.Stuff != null)
            {
                stuff = baseForm.Stuff;
            }
            ThingWithComps newThing = (ThingWithComps)ThingMaker.MakeThing(changeTo, stuff);
            newThing.HitPoints = hitPoints;
            ThingStyleDef styleDef = baseForm.StyleDef;
            if (baseForm.def.randomStyle != null && newThing.def.randomStyle != null)
            {
                ThingStyleChance chance = baseForm.def.randomStyle.Find(x => x.StyleDef == styleDef);
                int index = baseForm.def.randomStyle.IndexOf(chance);
                newThing.StyleDef = newThing.def.randomStyle[index].StyleDef;
            }
            return newThing;
        }

        public static void CopyQuality(ThingWithComps baseForm, ThingWithComps switchForm)
        {
            CompQuality quality = switchForm.GetComp<CompQuality>();
            if (baseForm.TryGetQuality(out QualityCategory q))
            {
                quality.SetQuality(q, null);
            }
        }

        public static void ChangeOldThing(ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            IntVec3 intVec3 = baseForm.Position;
            Map map = baseForm.Map;
            baseForm.Destroy();
            CopyQuality(baseForm, newThing);
            GenSpawn.Spawn(newThing, intVec3, map);
        }

        public static void ChangeEquipThing(this Pawn pawn, ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            pawn.equipment.DestroyEquipment(baseForm);
            baseForm.Notify_Unequipped(pawn);
            CopyQuality(baseForm, newThing);
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
                    CompSwitch compSwitch = thing.GetComp<CompSwitch>();
                    if (compSwitch != null)
                    {
                        Pawn.SwitchFormTo(thing, compSwitch);
                    }
                }
            }
        }
    }
}