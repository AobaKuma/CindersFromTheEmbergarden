using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class PawnGizmoPatch
    {
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
}
