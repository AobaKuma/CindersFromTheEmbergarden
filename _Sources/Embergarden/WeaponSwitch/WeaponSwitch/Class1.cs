using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WeaponSwitch
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

    public class CompSwitch : ThingComp
    {
        public CompProperties_Switch Props => (CompProperties_Switch)props;

        public HediffWithComps hediff;
        public HediffComp_Disappears Disappears => hediff.GetComp<HediffComp_Disappears>();
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (Props.abilityDef != null)
            {
                pawn.abilities.GainAbility(Props.abilityDef);
            }
            if (hediff != null)
            {
                pawn.health.AddHediff(hediff);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (Props.abilityDef != null)
            {
                pawn.abilities.RemoveAbility(Props.abilityDef);
            }
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = "";
            if (hediff != null)
            {
                text += hediff.LabelBase + ": " + Disappears.ticksToDisappear.ToStringSecondsFromTicks("F0");
            }
            return text;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (hediff != null)
            {
                float severityAdjustment = 0f;
                Disappears.CompPostTick(ref severityAdjustment);
                if (Disappears.ticksToDisappear <= 0)
                {
                    hediff = null;
                    Extension.ChangeOldThing(parent, Props.changeTo);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref hediff, "hediff", true);
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
            if (comp.hediff != null)
            {
                comp.Disappears.ticksToDisappear = 0;
                Pawn.health.RemoveHediff(comp.hediff);
                comp.hediff = null;
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

        public static void CopyComps(ThingWithComps baseForm, ThingWithComps switchForm)
        {
            List<ThingComp> comps = switchForm.AllComps.ListFullCopy();
            CompSwitch compSwitch = switchForm.GetComp<CompSwitch>();
            comps.Remove(compSwitch);
            comps.Remove(comps.Find(x => x is CompEquippable));
            comps.Remove(comps.Find(x => x is CompStyleable));
            for (int i = 0; i < comps.Count; i++)
            {
                ThingComp comp = comps[i];
                CompProperties Index = comp.props;
                ThingComp baseComp = baseForm.GetCompByDefType(Index);
                if (baseComp != null)
                {
                    baseComp.parent = switchForm;
                    switchForm.AllComps[switchForm.AllComps.IndexOf(comp)] = baseComp;
                }
            }
            compSwitch.hediff = baseForm.GetComp<CompSwitch>().hediff;
        }

        public static void ChangeOldThing(ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            IntVec3 intVec3 = baseForm.Position;
            Map map = baseForm.Map;
            baseForm.Destroy();
            CopyComps(baseForm, newThing);
            GenSpawn.Spawn(newThing, intVec3, map);
        }

        public static void ChangeEquipThing(this Pawn pawn, ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            pawn.equipment.DestroyEquipment(baseForm);
            baseForm.Notify_Unequipped(pawn);
            CopyComps(baseForm, newThing);
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
            compSwitch.hediff = parent;
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
                    compSwitch.hediff = null;
                    if (compSwitch != null)
                    {
                        Pawn.ChangeEquipThing(thing, compSwitch.Props.changeTo);
                        compSwitch = Pawn.equipment.Primary.GetComp<CompSwitch>();
                        Ability ability = Pawn.abilities.GetAbility(compSwitch.Props.abilityDef);
                        ability.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
                    }
                }
            }
        }
    }
}