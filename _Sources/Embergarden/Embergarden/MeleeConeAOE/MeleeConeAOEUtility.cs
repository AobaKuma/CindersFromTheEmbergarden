using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Embergarden
{
    internal static class MeleeConeAOEUtility
    {
        internal static void Explode(Pawn attacker, Map map, IntVec3 origin, IntVec3 direction,
            Thing intendedTarget, ThingDef weapon, DefModExtension_MeleeConeAOE settings)
        {
            List<IntVec3> cells = new List<IntVec3>();
            int reach = (int)Math.Ceiling(Math.Min(settings.radius, Math.Max(map.Size.x, map.Size.z)));
            for (int x = Math.Max(0, origin.x - reach); x <= Math.Min(map.Size.x - 1, origin.x + reach); x++)
            {
                for (int z = Math.Max(0, origin.z - reach); z <= Math.Min(map.Size.z - 1, origin.z + reach); z++)
                {
                    if (MeleeConeAOEGeometry.ContainsCell(x - origin.x, z - origin.z,
                        direction.x, direction.z, settings.radius, settings.HalfAngel))
                    {
                        cells.Add(new IntVec3(x, 0, z));
                    }
                }
            }

            if (cells.Count == 0) return;

            List<Thing> ignoredThings = new List<Thing> { attacker };
            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (thing != attacker && !thing.HostileTo(attacker)) ignoredThings.Add(thing);
            }

            GenExplosion.DoExplosion(origin, map, settings.radius, settings.damageDef, attacker,
                damAmount: settings.damageAmount, armorPenetration: settings.armorPenetration,
                weapon: weapon, intendedTarget: intendedTarget, damageFalloff: false,
                ignoredThings: ignoredThings, doSoundEffects: settings.doSoundEffects,
                screenShakeFactor: settings.screenShakeFactor, overrideCells: cells);
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    internal static class MeleeConeAOE_MeleeAttackPatch
    {
        internal sealed class AttackState
        {
            internal Pawn attacker;
            internal Map map;
            internal IntVec3 origin;
            internal IntVec3 direction;
            internal Thing target;
            internal ThingDef weapon;
            internal DefModExtension_MeleeConeAOE settings;
        }

        private static bool Prepare()
        {
            return !ModsConfig.IsActive("ceteam.combatextended")
                && !ModsConfig.IsActive("ceteam.combatextended_steam");
        }

        private static void Prefix(Verb_MeleeAttack __instance, out AttackState __state)
        {
            __state = null;
            Pawn attacker = __instance.CasterPawn;
            if (attacker == null || !attacker.Spawned || attacker.stances.FullBodyBusy) return;

            Thing target = __instance.CurrentTarget.Thing;
            if (target == null || !target.Spawned || target.Map != attacker.Map) return;
            DefModExtension_MeleeConeAOE settings = attacker.def.GetModExtension<DefModExtension_MeleeConeAOE>();
            if (settings == null || !settings.IsValid) return;

            IntVec3 direction = target.Position - attacker.Position;
            if (direction.x == 0 && direction.z == 0) direction = attacker.Rotation.FacingCell;
            __state = new AttackState
            {
                attacker = attacker,
                map = attacker.Map,
                origin = attacker.Position,
                direction = direction,
                target = target,
                weapon = __instance.EquipmentSource?.def ?? attacker.def,
                settings = settings
            };
        }

        private static void Postfix(AttackState __state)
        {
            if (__state == null) return;
            MeleeConeAOEUtility.Explode(__state.attacker, __state.map, __state.origin, __state.direction,
                __state.target, __state.weapon, __state.settings);
        }
    }
}
