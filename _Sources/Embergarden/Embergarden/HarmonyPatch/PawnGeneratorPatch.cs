using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using System.Security.Cryptography;
using Verse.AI;
using static Verse.DamageWorker;

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
            if (__result.kindDef.GetModExtension<ModExtension_HeadOverride>() is ModExtension_HeadOverride ext2 && ext2.heads.Any())
            {
                __result.story.headType = ext2.heads.RandomElement();
                if (ext2.noBeards)
                {
                    __result.style.beardDef = BeardDefOf.NoBeard;
                }
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

    public class ModExtension_HeadOverride : DefModExtension
    {
        public List<HeadTypeDef> heads = new List<HeadTypeDef>();

        public bool noBeards = true;
    }
}
