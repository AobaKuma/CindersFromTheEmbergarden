using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using System.Security.Cryptography;
using Verse.AI;

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
            var w = __result.equipment?.Primary;
            if (w != null)
            {
                var def = w.def;
                if (!def.randomStyle.NullOrEmpty() && Rand.Chance(def.randomStyleChance))
                {
                    w.StyleDef = def.randomStyle.RandomElementByWeight((ThingStyleChance x) => x.Chance).StyleDef;
                }
            }
        }
    }

    public class ModExtension_RandomOneAbility : DefModExtension
    {
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Recipe_RemoveBodyPart), "DamagePart")]
    public static class TestA
    {
        public static void Prefix()
        {
            Log.Message("DamagePart");
        }
    }
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class TestE
    {
        public static void Prefix()
        {
            Log.Message("PawnPreApplyDamage");
        }
    }
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "PostApplyDamage")]
    public static class TestF
    {
        public static void Prefix()
        {
            Log.Message("PawnPostApplyDamage");
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class TestB
    {
        public static void Prefix(Thing __instance, DamageInfo dinfo)
        {
            Log.Message($"{__instance} TakeDamage {dinfo}");
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class TestC
    {
        public static void Prefix()
        {
            Log.Message("PreApplyDamage");
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn_HealthTracker), "PostApplyDamage")]
    public static class TestD
    {
        public static void Prefix()
        {
            Log.Message("PostApplyDamage");
        }
    }
}
