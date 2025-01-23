using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", [typeof(PawnGenerationRequest)])]
    public static class PawnGeneratorPatch
    {
        public static void Postfix(ref Pawn __result)
        {
            if (__result.kindDef.GetModExtension<ModExtension_RandomOneAbility>() is ModExtension_RandomOneAbility ext)
            {
                __result.abilities.GainAbility(ext.abilities.RandomElement());
            }
        }
    }

    public class ModExtension_RandomOneAbility : DefModExtension
    {
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }
}
