﻿using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Embergarden
{
    public class CompSecondaryVerb : ThingComp, IEquippedGizmo, IAltFireMode
    {
        private Verb verbInt;
        private CompEquippable compEquippableInt;
        private bool isSecondaryVerbSelected;
        private FieldInfo cachedBurstShotCountInfo;

        public CompProperties_SecondaryVerb Props => (CompProperties_SecondaryVerb)props;

        public bool IsSecondaryVerbSelected => isSecondaryVerbSelected;

        public Verb secondaryVerb;

        private CompEquippable EquipmentSource
        {
            get
            {
                if (compEquippableInt != null)
                {
                    return compEquippableInt;
                }
                compEquippableInt = parent.TryGetComp<CompEquippable>();
                if (compEquippableInt == null)
                {
                    Log.ErrorOnce(parent.LabelCap + " has CompSecondaryVerb but no CompEquippable", 50020);
                }
                return compEquippableInt;
            }
        }

        public Pawn CasterPawn => Verb.caster as Pawn;

        private Verb Verb
        {
            get
            {
                if (verbInt == null)
                {
                    verbInt = EquipmentSource.PrimaryVerb;
                }
                return verbInt;
            }
        }

        public List<StatModifier> statOffsets => Props.statOffsets;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (CasterPawn == null || CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                string text = (IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon);
                if (text == "")
                {
                    text = "UI/Buttons/Reload";
                }
                yield return new Command_Action
                {
                    action = SwitchVerb,
                    defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
                    defaultDesc = Props.description,
                    icon = ContentFinder<Texture2D>.Get(text, reportFailure: false)
                };
            }
        }

        public IEnumerable<Gizmo> CompGetGizmosEquipped()
        {
            if (CasterPawn == null || CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                string text = (IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon);
                if (text == "")
                {
                    text = "UI/Buttons/Reload";
                }
                yield return new Command_Action
                {
                    action = SwitchVerb,
                    defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel),
                    defaultDesc = Props.description,
                    icon = ContentFinder<Texture2D>.Get(text, reportFailure: false)
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isSecondaryVerbSelected, "PLA_useSecondaryVerb", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostAmmoDataLoaded();
            }
        }

        public void SwitchToSecondary()
        {
            EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
            isSecondaryVerbSelected = true;
        }

        private void SwitchVerb()
        {
            if (EquipmentSource.PrimaryVerb.Bursting) return;
            /*
            Verb verb = (Verb)Activator.CreateInstance(Props.verbProps.verbClass);
            string text = Verb.CalculateUniqueLoadID(EquipmentSource, 114514);
            verb.loadID = text;
            verb.verbProps = Props.verbProps;
            verb.verbTracker = EquipmentSource.verbTracker;
            verb.caster = Verb.caster;
            verb.tool = null;
            verb.maneuver = null;
            Log.Message(verb.ToString());
            secondaryVerb = verb;
            */
            if (!IsSecondaryVerbSelected)
            {
                EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
                isSecondaryVerbSelected = true;
            }
            else
            {
                EquipmentSource.PrimaryVerb.verbProps = parent.def.Verbs[0];
                isSecondaryVerbSelected = false;
            }
            if (cachedBurstShotCountInfo == null) cachedBurstShotCountInfo = typeof(Verb).GetField("cachedBurstShotCount", BindingFlags.NonPublic | BindingFlags.Instance);
            cachedBurstShotCountInfo.SetValue(EquipmentSource.PrimaryVerb, null);
            Props.sound?.PlayOneShot(new TargetInfo(CasterPawn.Position, CasterPawn.Map));
        }

        private void PostAmmoDataLoaded()
        {
            if (isSecondaryVerbSelected)
            {
                EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
            }
        }
    }

    public class CompProperties_SecondaryVerb : CompProperties
    {
        public VerbProperties verbProps = new VerbProperties();

        [NoTranslate]
        public string mainCommandIcon;

        [MustTranslate]
        public string mainWeaponLabel;

        [NoTranslate]
        public string secondaryCommandIcon;

        [MustTranslate]
        public string secondaryWeaponLabel;

        [MustTranslate]
        public string description;

        public SoundDef sound;

        public List<StatModifier> statOffsets = new List<StatModifier>();

        public CompProperties_SecondaryVerb()
        {
            compClass = typeof(CompSecondaryVerb);
        }
    }

    public interface IEquippedGizmo
    {
        IEnumerable<Gizmo> CompGetGizmosEquipped();
    }

    public interface IAltFireMode
    {
        bool IsSecondaryVerbSelected { get; }

        List<StatModifier> statOffsets { get; }
    }
}
