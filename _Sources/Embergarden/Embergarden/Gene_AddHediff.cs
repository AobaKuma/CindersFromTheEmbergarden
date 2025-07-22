using Verse;

namespace Embergarden
{
    public class Gene_AddHediff : Gene
    {
        ModExtension_GeneAddHediff ext => def.GetModExtension<ModExtension_GeneAddHediff>();
        public override void PostAdd()
        {
            if (ext != null)
            {
                pawn.health.AddHediff(ext.HediffDef);
            }
        }

        public override void PostRemove()
        {
            if (ext != null)
            {
                for (int i = pawn.health.hediffSet.hediffs.Count; i > 0; i--)
                {
                    var h = pawn.health.hediffSet.hediffs[i - 1];
                    if (h.def == ext.HediffDef)
                    {
                        pawn.health.RemoveHediff(h);
                    }
                }
            }
        }
    }

    public class ModExtension_GeneAddHediff : DefModExtension
    {
        public HediffDef HediffDef;
    }
}
