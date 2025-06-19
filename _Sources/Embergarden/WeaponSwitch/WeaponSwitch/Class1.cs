using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WeaponSwitch
{
    public class CompProperties_Switch : CompProperties_EquippableAbility
    {
        public ThingDef changeTo;
        public CompProperties_Switch()
        {
            compClass = typeof(CompSwitch);
        }
    }

    public class CompSwitch : CompEquippableAbility
    {
        public CompProperties_Switch Props => (CompProperties_Switch)props;

        public HediffWithComps hediff;
        public HediffComp_Disappears Disappears => hediff.GetComp<HediffComp_Disappears>();
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (hediff != null)
            {
                pawn.health.AddHediff(hediff);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
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
            for (int i = 0; i < newThing.AllComps.Count; i++)
            {
                CompProperties Index = newThing.AllComps[i].props;
                ThingComp baseComp = baseForm.GetCompByDefType(Index);
                if (baseComp != null)
                {
                    baseComp.parent = newThing;
                    newThing.AllComps[i] = baseComp;
                }
            }
            CompSwitch compSwitch = newThing.GetComp<CompSwitch>();
            compSwitch.Initialize(newThing.def.comps.Find(x => x.compClass == typeof(CompSwitch)));
            ThingStyleDef styleDef = baseForm.StyleDef;
            if (baseForm.def.randomStyle != null && newThing.def.randomStyle != null)
            {
                ThingStyleChance chance = baseForm.def.randomStyle.Find(x => x.StyleDef == styleDef);
                int index = baseForm.def.randomStyle.IndexOf(chance);
                newThing.StyleDef = newThing.def.randomStyle[index].StyleDef;
            }
            return newThing;
        }

        public static void ChangeOldThing(ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            IntVec3 intVec3 = baseForm.Position;
            Map map = baseForm.Map;
            baseForm.Destroy();
            GenSpawn.Spawn(newThing, intVec3, map);
        }

        public static void ChangeEquipThing(this Pawn pawn, ThingWithComps baseForm, ThingDef changeTo)
        {
            ThingWithComps newThing = ChangeThing(baseForm, changeTo);
            pawn.equipment.DestroyEquipment(baseForm);
            baseForm.Notify_Unequipped(pawn);
            pawn.equipment.MakeRoomFor(newThing);
            pawn.equipment.AddEquipment(newThing);
        }
    }

    public class HediffCompPropertiesSwitch : HediffCompProperties_GiveAbility
    {
        public HediffCompPropertiesSwitch()
        {
            compClass = typeof(SwitchWhenDisappear);
        }
    }
    public class SwitchWhenDisappear : HediffComp_GiveAbility
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
                    }
                }
            }
        }
    }
}