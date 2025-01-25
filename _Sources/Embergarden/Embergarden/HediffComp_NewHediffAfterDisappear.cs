using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class HediffComp_NewHediffAfterDisappear : HediffComp
    {
        HediffCompProperties_NewHediffAfterDisappear Props => props as HediffCompProperties_NewHediffAfterDisappear;

        public override void CompPostPostRemoved()
        {
            if (parent.Severity == 0)
            {
                Pawn.health.AddHediff(Props.hediffDef);
            }
        }
    }

    public class HediffCompProperties_NewHediffAfterDisappear : HediffCompProperties
    {
        public HediffDef hediffDef;
        public HediffCompProperties_NewHediffAfterDisappear()
        {
            compClass = typeof(HediffComp_NewHediffAfterDisappear);
        }
    }
}
