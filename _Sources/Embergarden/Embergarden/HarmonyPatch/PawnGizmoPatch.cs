using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse.AI.Group;

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
            /**
            if (__instance.def.HasModExtension<ModExtension_Draftable>() && __instance.drafter != null)
            {
                AcceptanceReport allowsDrafting = __instance.GetLord()?.AllowsDrafting(__instance) ?? ((AcceptanceReport)true);
                foreach (Gizmo gizmo2 in drafterGizmo.Invoke(__instance.drafter, []) as IEnumerable<Gizmo>)
                {
                    if (!allowsDrafting && !gizmo2.Disabled)
                    {
                        gizmo2.Disabled = true;
                        gizmo2.disabledReason = allowsDrafting.Reason;
                    }
                    yield return gizmo2;
                }
            }
            **/
        }
    }
}
