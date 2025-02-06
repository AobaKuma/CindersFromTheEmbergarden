using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class CompAddHediff : ThingComp
    {
        CompProperties_AddHediff Props => props as CompProperties_AddHediff;

        public override void PostPostMake()
        {
            base.PostPostMake();
            TryRegenHediff();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            TryRegenHediff();
        }

        protected void TryRegenHediff()
        {
            if (parent is Pawn p)
            {
                if (Props.skipPawnKindDefs.Any() && Props.skipPawnKindDefs.Contains(p.kindDef)) return;
                foreach (var def in Props.hediffDefs)
                {
                    if (!p.health?.hediffSet.HasHediff(def) ?? false)
                    {
                        p.health.AddHediff(def);
                    }
                }
            }
        }
    }

    public class CompProperties_AddHediff : CompProperties
    {
        public CompProperties_AddHediff()
        {
            compClass = typeof(CompAddHediff);
        }

        public List<HediffDef> hediffDefs = new List<HediffDef>();

        public List<PawnKindDef> skipPawnKindDefs = new List<PawnKindDef>();
    }
}
