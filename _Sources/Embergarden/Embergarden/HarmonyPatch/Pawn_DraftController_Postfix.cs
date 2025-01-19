using Verse;
using HarmonyLib;
using RimWorld;
using System.Reflection;

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

    public class ModExtension_Draftable : DefModExtension
    {

    }
}
