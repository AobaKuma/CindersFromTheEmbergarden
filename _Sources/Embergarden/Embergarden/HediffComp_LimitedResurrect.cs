using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI.Group;

namespace Embergarden
{
    public class HediffComp_LimitedResurrect : HediffComp
    {
        public bool CanResurrect = true;

        DamageInfo? dinfo;
        Hediff culprit = null;


        HediffCompProperties_Resurrect Props => base.props as HediffCompProperties_Resurrect;

        FieldInfo corpseTrackersInfo => typeof(GameComponent_Anomaly).GetField("corpseTrackers", BindingFlags.NonPublic | BindingFlags.Instance);

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (CanResurrect) return null;

                return Props.disabledString;
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            this.culprit = culprit;
            this.dinfo = dinfo;
            Current.Game.GetComponent<GameComponent_ResurrectAdaptor>().hediffs.Add(this);
        }

        public void Resurrect()
        {
            if (!CanResurrect) return;
            CanResurrect = false;

            if (ModLister.AnomalyInstalled)
            {
                var corpseTrackers = corpseTrackersInfo.GetValue(Find.Anomaly) as Dictionary<Pawn, UnnaturalCorpseTracker>;
                foreach (var c in corpseTrackers)
                {
                    if (c.Value.AwokenPawn != null && c.Value.AwokenPawn == Pawn) return;
                }
            }

            ResurrectionUtility.TryResurrect(Pawn, null);
            HediffPurge(Pawn);
            Props.effecter?.Spawn(Pawn.PositionHeld, Pawn.MapHeld);
            if (Props.afterEffectHediff != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.afterEffectHediff, Pawn);
                if (hediff.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears comp)
                {
                    comp.disappearsAfterTicks = Props.afterEffectTick;
                    comp.ticksToDisappear = Props.afterEffectTick;
                }
                Pawn.health.forceDowned = true;
                Pawn.health.AddHediff(hediff);
                Pawn.health.forceDowned = false;
            }
            foreach (var hediffInfo in Props.regenHediffs)
            {
                if (Pawn.health.hediffSet.GetFirstHediffOfDef(hediffInfo.hediff) != null) continue;
                if (hediffInfo.bodyPart == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffInfo.hediff, Pawn, null);
                    Pawn.health.AddHediff(hediff);
                }
                else
                {
                    foreach (BodyPartRecord notMissingPart in Pawn.health.hediffSet.GetNotMissingParts())
                    {
                        if ((hediffInfo.partCustomLabel != null && notMissingPart.untranslatedCustomLabel == hediffInfo.partCustomLabel)
                            || notMissingPart.def == hediffInfo.bodyPart)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(hediffInfo.hediff, Pawn, notMissingPart);
                            Pawn.health.forceDowned = true;
                            Pawn.health.AddHediff(hediff);
                            Pawn.health.forceDowned = false;
                            break;
                        }
                    }
                }
            }

            if (Pawn.Downed)
            {
                Pawn.GetLord()?.RemovePawn(Pawn);
            }
            if (Pawn.IsColonist)
            {
                if (Props.notificationByWeapon != null && dinfo.HasValue && dinfo.Value.Weapon != null)
                {
                    Messages.Message(Props.notificationByWeapon.Formatted(Pawn.Named("PAWN"), dinfo.Value.Weapon.label.Named("WEAPON")), Pawn, MessageTypeDefOf.NegativeHealthEvent);
                }
                else if (Props.notificationByHediff != null && culprit != null)
                {
                    Messages.Message(Props.notificationByHediff.Formatted(Pawn.Named("PAWN"), culprit.LabelBase.Named("HEDIFF")), Pawn, MessageTypeDefOf.NegativeHealthEvent);
                }
                else if (Props.notificationGeneral != null && culprit != null)
                {
                    Messages.Message(Props.notificationGeneral.Formatted(Pawn.Named("PAWN")), Pawn, MessageTypeDefOf.NegativeHealthEvent);
                }
            }
            dinfo = null;
            culprit = null;
        }

        void HediffPurge(Pawn pawn)
        {
            for (int i = pawn.health.hediffSet.hediffs.Count; i > 0; i--)
            {
                var hediff = pawn.health.hediffSet.hediffs[i - 1];
                if (hediff.def.isBad)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref CanResurrect, "canResurrect");
        }
    }

    public class HediffCompProperties_Resurrect : HediffCompProperties
    {
        public int afterEffectTick = 120000;

        public HediffDef afterEffectHediff;

        public EffecterDef effecter;

        [MustTranslate]
        public string notificationGeneral, notificationByWeapon, notificationByHediff, disabledString;

        public List<HediffParm> regenHediffs = new List<HediffParm>();
        public HediffCompProperties_Resurrect()
        {
            compClass = typeof(HediffComp_LimitedResurrect);
        }
    }

    public class HediffComp_PreventRemoval : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            Pawn.health.AddHediff(parent);
        }
    }

    public class HediffParm
    {
        public HediffDef hediff;

        public BodyPartDef bodyPart;

        public string partCustomLabel;
    }
}
