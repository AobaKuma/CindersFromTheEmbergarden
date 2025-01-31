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
}
