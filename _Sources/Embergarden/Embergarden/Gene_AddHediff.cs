using HarmonyLib;
using RimWorld;
using Verse;

namespace Embergarden
{
    [HarmonyPatch(typeof(CompStatue), "InitFakePawn")]
    public static class Patch_InitFakePawn
    {
        public static bool statue = false;
        public static void Prefix()
        {
            statue = true;
        }

        public static void Postfix()
        {
            statue = false;
        }
    }
    public class Gene_AddHediff : Gene
    {
        private ModExtension_GeneAddHediff ext => def.GetModExtension<ModExtension_GeneAddHediff>();

        public override void PostAdd()
        {
            if (!Patch_InitFakePawn.statue)
            {
                if (ext != null)
                {
                    pawn.health?.AddHediff(ext.HediffDef);
                }
            }
        }

        public override void PostRemove()
        {
            if (!Patch_InitFakePawn.statue)
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
    }

    public class ModExtension_GeneAddHediff : DefModExtension
    {
        public HediffDef HediffDef;
    }
}