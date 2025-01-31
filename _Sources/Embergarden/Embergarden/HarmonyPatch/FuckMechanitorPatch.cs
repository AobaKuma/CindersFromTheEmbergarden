using Verse;
using HarmonyLib;
using RimWorld;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(CallBossgroupUtility), "BossgroupEverCallable")]
    public static class FuckMechanitorPatch
    {
        public static bool Prefix(BossgroupDef def)
        {
            return !def.HasModExtension<FuckMechanitor>();
        }
    }

    public class FuckMechanitor : DefModExtension { }
}
