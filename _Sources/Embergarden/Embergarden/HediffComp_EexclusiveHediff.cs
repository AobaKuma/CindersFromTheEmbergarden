using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class HediffComp_EexclusiveHediff : HediffComp
    {
        private HediffCompProperties_EexclusiveHediff Props => (HediffCompProperties_EexclusiveHediff)this.props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            foreach (var item in Props.exclusiveHediff)
            {
                if (parent.pawn.health.hediffSet.GetFirstHediffOfDef(item) != null)
                {
                    parent.pawn.health.hediffSet.hediffs.Remove(parent.pawn.health.hediffSet.GetFirstHediffOfDef(item));
                }
            }
        }
    }
    public class HediffCompProperties_EexclusiveHediff : HediffCompProperties
    {
        public List<HediffDef> exclusiveHediff;

        public HediffCompProperties_EexclusiveHediff()
        {
            compClass = typeof(HediffComp_EexclusiveHediff);
        }
    }
}
