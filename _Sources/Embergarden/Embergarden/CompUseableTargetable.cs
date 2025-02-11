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

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
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
                    var acrp = CanBeUsedBy(p, true);
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
            yield break;
        }
    }

    public class JobDriver_UseItemTargetable : JobDriver_UseItem
    {
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
