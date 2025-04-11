using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse.AI.Group;
using System.Linq;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class PawnGizmoPatch
    {
        static MethodInfo drafterGizmo => typeof(Pawn_DraftController).GetMethod("GetGizmos", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (Gizmo command in __result) yield return command;
            if (__instance.Spawned && Find.Selector.SingleSelectedThing == __instance && __instance.equipment != null)
            {
                ThingWithComps thingWithComps = __instance.equipment.Primary;
                if (thingWithComps != null && thingWithComps.AllComps.Any())
                {
                    IEnumerable<IEquippedGizmo> comps = thingWithComps.GetComps<IEquippedGizmo>();
                    foreach (IEquippedGizmo allComp in comps)
                    {
                        foreach (Gizmo item in allComp.CompGetGizmosEquipped())
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(JobDriver_Blind), "Blind")]
    public static class JobDriver_BlindPatch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetHediffComps<HediffComp_Regen>().Any()) pawn.health.GetOrAddHediff(Cider_DefOf.Cinder_EyeRegenInhibited);
        }
    }
}
