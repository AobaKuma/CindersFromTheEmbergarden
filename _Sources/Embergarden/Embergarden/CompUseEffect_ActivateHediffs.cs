using RimWorld;
using Verse;

namespace Embergarden
{
    public class CompUseEffect_ActivateHediffs : CompUseEffect
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            bool flag = false;
            if (p.health.hediffSet.GetHediffComps<HediffComp_LimitedResurrect>().FirstOrFallback(null) is HediffComp_LimitedResurrect resurrect && !resurrect.CanResurrect) flag = true;
            if (p.health.hediffSet.GetHediffComps<HediffComp_Regen>().FirstOrFallback(null) is HediffComp_Regen regen && !regen.Unlocked) flag = true;
            return flag ? true : "Cinder_NeedResurrectOrRegen";
        }

        public override void DoEffect(Pawn usedBy)
        {
            if (usedBy.health.hediffSet.GetHediffComps<HediffComp_LimitedResurrect>().FirstOrFallback(null) is HediffComp_LimitedResurrect resurrect) resurrect.CanResurrect = true;
            if (usedBy.health.hediffSet.GetHediffComps<HediffComp_Regen>().FirstOrFallback(null) is HediffComp_Regen regen) regen.Unlocked = true;
        }
    }
}
