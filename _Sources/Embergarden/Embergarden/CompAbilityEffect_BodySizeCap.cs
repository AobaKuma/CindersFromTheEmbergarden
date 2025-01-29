using RimWorld;
using Unity.Mathematics;
using Verse;

namespace Embergarden
{
    public class CompAbilityEffect_BodySizeCap : CompAbilityEffect
    {
        public CompProperties_BodySizeCap Props => props as CompProperties_BodySizeCap;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (target.Thing == parent.pawn) return false;
            return CanApplyOn(target, target);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return target.Pawn != null && target.Pawn.BodySize < Props.bodySizeCap;
        }
    }
    public class CompProperties_BodySizeCap : CompProperties_AbilityEffect
    {
        public CompProperties_BodySizeCap()
        {
            compClass = typeof(CompAbilityEffect_BodySizeCap);
        }
        public float bodySizeCap = 1.5f;
    }
}
