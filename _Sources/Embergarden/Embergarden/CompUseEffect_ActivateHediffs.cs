using RimWorld;
using Verse;

namespace Embergarden
{
    public class CompUseEffect_ActivateHediffs : CompUseEffect
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            bool flag = false;
            if (p.health.hediffSet.GetHediffComps<HediffComp_LimitedResurrect>() is HediffComp_LimitedResurrect resurrect && !resurrect.CanResurrect) flag = true;
            if (!flag && p.health.hediffSet.GetHediffComps<HediffComp_Regen>() is HediffComp_Regen regen && !regen.Unlocked) flag = true;
            return flag;
        }

        public override void DoEffect(Pawn usedBy)
        {
            if (usedBy.health.hediffSet.GetHediffComps<HediffComp_LimitedResurrect>() is HediffComp_LimitedResurrect resurrect) resurrect.CanResurrect = true;
            if (usedBy.health.hediffSet.GetHediffComps<HediffComp_Regen>() is HediffComp_Regen regen) regen.Unlocked = true;
        }
    }
}
