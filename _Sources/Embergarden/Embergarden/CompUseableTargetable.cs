using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Embergarden
{
    public class CompUseableTargetable : CompUsable
    {
        public override ITargetingSource DestinationSelector => this;

        public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
        {
            return true;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            foreach (var fm in base.CompFloatMenuOptions(myPawn))
            {
                yield return fm;
            }

            FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Cinder_SerumTargetOthers", delegate
            {
                TargetingParameters parm = new TargetingParameters()
                {
                    canTargetSelf = true,
                    canTargetMechs = false,
                    canTargetAnimals = false
                };
                Find.Targeter.BeginTargeting(parm, action: delegate (LocalTargetInfo target)
                {
                    LocalTargetInfo tgt = LocalTargetInfo.Invalid;
                    if (target.Thing is Pawn p)
                    {
                        tgt = p;
                        var acrp = base.CanBeUsedBy(p, true);
                        if (acrp)
                        {
                            TryStartUseJob(myPawn, tgt, Props.ignoreOtherReservations);
                        }
                        else
                        {
                            Messages.Message(acrp.Reason, MessageTypeDefOf.RejectInput, false);
                        }
                    }
                });
            }, priority: Props.floatMenuOptionPriority), myPawn, parent);
            yield return floatMenuOption;
        }
    }

    public class JobDriver_UseItemTargetable : JobDriver_UseItem
    {
        private int useDuration = -1;

        private Mote warmupMote;

        private const TargetIndex Item = TargetIndex.A;

        private const TargetIndex Target = TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (base.TryMakePreToilReservations(errorOnFailed))
            {
                return pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useDuration, "useDuration", 0);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            this.FailOnDespawnedNullOrForbidden(Target);
            this.FailOnBurningImmobile(Target);
            yield return Toils_Goto.GotoThing(Item, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(Item).FailOnSomeonePhysicallyInteracting(Item);
            yield return Toils_Haul.StartCarryThing(Item).FailOnDestroyedNullOrForbidden(Item);
            yield return Toils_Goto.GotoThing(Target, PathEndMode.ClosestTouch);
            yield return PrepareToUse();
            yield return UseAtTarget();
        }

        protected new Toil PrepareToUse()
        {
            Toil toil = Toils_General.Wait(useDuration, TargetIndex.A);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.handlingFacing = true;
            toil.tickAction = delegate
            {
                if (job.GetTarget(TargetIndex.A).Thing is ThingWithComps thingWithComps)
                {
                    foreach (CompUseEffect comp in thingWithComps.GetComps<CompUseEffect>())
                    {
                        comp?.PrepareTick();
                    }
                }
                else
                {
                    job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUseEffect>()?.PrepareTick();
                }
                CompUsable compUsable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>();
                if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
                {
                    warmupMote = MoteMaker.MakeAttachedOverlay(job.GetTarget(TargetIndex.B).Thing, compUsable.Props.warmupMote, Vector3.zero);
                }
                warmupMote?.Maintain();
                pawn.rotationTracker.FaceTarget(base.TargetA);
            };
            if (job.targetB.IsValid)
            {
                toil.FailOnDespawnedOrNull(TargetIndex.B);
                CompTargetable compTargetable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTargetable>();
                if (compTargetable != null && compTargetable.Props.nonDownedPawnOnly)
                {
                    toil.FailOnDestroyedOrNull(TargetIndex.B);
                    toil.FailOnDowned(TargetIndex.B);
                }
            }
            return toil;
        }

        protected Toil UseAtTarget()
        {
            Toil use = ToilMaker.MakeToil("Use");
            use.initAction = delegate
            {
                Pawn actor = use.actor;
                CompUsable compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable>();
                compUsable.UsedBy(actor.CurJob.targetB.Thing as Pawn);
                if (compUsable.Props.finalizeMote != null)
                {
                    MoteMaker.MakeAttachedOverlay(actor.CurJob.GetTarget(TargetIndex.B).Thing, compUsable.Props.finalizeMote, Vector3.zero);
                }
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            return use;
        }
    }
}
