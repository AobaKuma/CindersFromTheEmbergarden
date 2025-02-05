using Verse;
using HarmonyLib;
using RimWorld;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(CallBossgroupUtility), "BossgroupEverCallable")]
    public static class FuckMechanitorPatch
    {
        public static bool Prefix(BossgroupDef def, ref AcceptanceReport __result)
        {
            if (def.HasModExtension<FuckMechanitor>())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    public class FuckMechanitor : DefModExtension { }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnGenerator), "GenerateOrRedressPawnInternal")]
    static class FuckIdeoApparel
    {
        [HarmonyPrefix]
        static void FuckingIdeoApparel(ref PawnGenerationRequest request)
        {
            if (request.KindDef.HasModExtension<ModExtension_NoIdeoApparel>())
            {
                request.ForceNoIdeoGear = true;
            }
        }
    }

    public class ModExtension_NoIdeoApparel : DefModExtension { };
}
