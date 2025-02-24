using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using static RimWorld.MechClusterSketch;
using System.Collections.Generic;
using System.Linq;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class Pawn_DraftController_Postfix
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn_DraftController).PropertyGetter("ShowDraftGizmo");
        }

        [HarmonyPostfix]
        static void Postfix(ref bool __result, Pawn_DraftController __instance)
        {
            if (!__result && __instance.pawn.def.HasModExtension<ModExtension_Draftable>())
            {
                __result = true;
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class Pawn_IsColonyMech_Postfix
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn).PropertyGetter("IsColonyMech");
        }

        [HarmonyPostfix]
        static void Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result && __instance.def.HasModExtension<ModExtension_Draftable>())
            {
                __result = __instance.Faction == Faction.OfPlayer;
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class Pawn_IsColonyMechPlayerControlled_Postfix
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn).PropertyGetter("IsColonyMechPlayerControlled");
        }

        [HarmonyPostfix]
        static void Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result)
            {
                __result = ModExtension_Draftable.CanDraft(__instance);
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(MechanitorUtility), "CanDraftMech")]
    static class MechanitorUtility_Postfix
    {
        [HarmonyPostfix]
        static void Postfix(ref AcceptanceReport __result, Pawn mech)
        {
            if (!__result)
            {
                __result = ModExtension_Draftable.CanDraft(mech);
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
    static class InMechanitorCommandRange_Postfix
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, Pawn mech)
        {
            if (!__result)
            {
                __result = ModExtension_Draftable.CanDraft(mech);
            }
        }
    }

    public class ModExtension_Draftable : DefModExtension
    {
        public static bool CanDraft(Pawn p)
        {
            return p.def.HasModExtension<ModExtension_Draftable>() && p.Faction == Faction.OfPlayer && p.MentalStateDef == null;
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class PlayerPawnsForStoryteller_Postfix
    {
        static MethodBase TargetMethod()
        {
            return typeof(Map).PropertyGetter("PlayerPawnsForStoryteller");
        }

        [HarmonyPostfix]
        static void Postfix(ref IEnumerable<Pawn> __result)
        {
            var cache = __result.ToList();
            foreach (var p in __result)
            {
                if (p.def.HasModExtension<ModExtension_Draftable>())
                {
                    cache.Remove(p);
                }
            }
            __result = cache;
        }
    }


    [StaticConstructorOnStartup]
    [HarmonyPatch]
    static class Alert_StarvationAnimals_Postfix
    {
        static MethodBase TargetMethod()
        {
            return typeof(Alert_StarvationAnimals).PropertyGetter("StarvingAnimals");
        }

        [HarmonyPostfix]
        static void Postfix(ref List<Pawn> __result)
        {
            var cache = __result.ToList();
            foreach (var p in __result)
            {
                if (p.def.HasModExtension<ModExtension_Draftable>() && !p.Spawned)
                {
                    cache.Remove(p);
                }
            }
            __result = cache;
        }
    }
}
