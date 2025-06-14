using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace Embergarden
{
    public class Projectile_Tentacle : Projectile
    {
        ProjectileProperties_Tentacle Props => def.projectile as ProjectileProperties_Tentacle;
        public List<Vector3> tentacles = new List<Vector3>();
        float? angle = null;
        int index;
        int flightTicks => (int)StartingTicksToImpact - lifetime;
        private Color? EmissionColor;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref index, "index");
            Scribe_Collections.Look(ref tentacles, "tentacles", LookMode.Value);
        }

        protected override void Tick()
        {
            if (AllComps != null)
            {
                int i = 0;
                for (int count = AllComps.Count; i < count; i++)
                {
                    AllComps[i].CompTick();
                }
            }
            lifetime--;
            if (flightTicks % Props.tickPerTentacle == 0)
            {
                if (EmissionColor == null) EmissionColor = Color.Lerp(Props.colorA, Props.colorB, Rand.Value);

                index = flightTicks / Props.tickPerTentacle;
                if (index < tentacles.Count)
                {
                    if (angle != null && Props.tentacleFleck != null)
                    {
                        var dataStatic = FleckMaker.GetDataStatic(origin + tentacles[index - 1], MapHeld, Props.tentacleFleck);
                        dataStatic.rotation = angle.Value;
                        dataStatic.instanceColor = EmissionColor;
                        MapHeld.flecks.CreateFleck(dataStatic);
                    }
                    if (Props.tentacleHeadFleck != null)
                    {
                        var A = tentacles[Mathf.Max(0, index - 1)];
                        var B = tentacles[Mathf.Min(tentacles.Count - 1, index + 1)];
                        angle = (A - B).ToAngleFlat() - 90;

                        var dataStatic = FleckMaker.GetDataStatic(origin + tentacles[index], MapHeld, Props.tentacleHeadFleck);
                        dataStatic.rotation = angle.Value;
                        dataStatic.instanceColor = EmissionColor;
                        MapHeld.flecks.CreateFleck(dataStatic);
                    }
                    Props.effecterDef.Spawn(this, Map, tentacles[index]);
                }

            }
            if (landed)
            {
                HitEffect();
                return;
            }
            ticksToImpact--;
            if (ticksToImpact <= 0)
            {
                HitEffect();
                if (DestinationCell.InBounds(base.Map))
                {
                    base.Position = DestinationCell;
                }
                Impact(usedTarget.Thing);
            }
        }

        protected void HitEffect()
        {
            if (index >= tentacles.Count) index = tentacles.Count - 1;
            if (angle != null && Props.tentacleHitFleck != null)
            {
                var dataStatic = FleckMaker.GetDataStatic(origin + tentacles[Mathf.Min(index, tentacles.Count - 1)], MapHeld, Props.tentacleHitFleck);
                dataStatic.rotation = angle.Value;
                MapHeld.flecks.CreateFleck(dataStatic);
                Props.effecterDef.Spawn(this, Map, tentacles[index]);
            }
            Props.hitEffecterDef?.Spawn(this, Map, tentacles[index]);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                bool instigatorGuilty = launcher is not Pawn pawn || !pawn.Drafted;
                DamageInfo dinfo = new(def.projectile.damageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                dinfo.SetWeaponQuality(equipmentQuality);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn2 = hitThing as Pawn;
                if (def.projectile.extraDamages != null)
                {
                    foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
                    {
                        if (Rand.Chance(extraDamage.chance))
                        {
                            DamageInfo dinfo2 = new(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                }
                if (Rand.Chance(def.projectile.explosionChanceToStartFire) && (pawn2 == null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(pawn2))))
                {
                    hitThing.TryAttachFire(Rand.Range(0, def.projectile.explosionChanceToStartFire), launcher);
                }
            }
            base.Impact(hitThing, blockedByShield);
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            var delta = usedTarget.Cell.ToVector3Shifted() - origin;
            var angle = delta.ToAngleFlat();
            float magnitude = delta.Yto0().magnitude;
            float swing = Props.swingRange.RandomInRange * Rand.Sign;
            float freq = Props.frequencyRange.RandomInRange;
            for (int i = Props.tickPerTentacle; i < ticksToImpact + Props.tickPerTentacle; i += Props.tickPerTentacle)
            {
                float pct = (float)i / ticksToImpact;
                var offset = Mathf.Sin(pct * magnitude * freq) * (pct > 0.5f ? 1 - pct : pct) * swing;
                tentacles.Add(delta * pct + new Vector3(0, 0, offset).RotatedBy(angle));
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        private float GetDodgeChance(LocalTargetInfo target)
        {
            if (target.Thing is not Pawn pawn)
            {
                return 0f;
            }
            if (pawn.Downed || pawn.Crawling) return 0f;
            if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.verb != null && !stance_Busy.verb.verbProps.IsMeleeAttack)
            {
                return 0f;
            }
            float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
            if (ModsConfig.IdeologyActive)
            {
                if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset);
                }
                else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                {
                    num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsLitOffset);
                }
            }
            return num;
        }
    }

    public class ProjectileProperties_Tentacle : ProjectileProperties
    {
        public int tickPerTentacle = 1;
        public EffecterDef effecterDef, hitEffecterDef;
        public FleckDef tentacleFleck, tentacleHeadFleck, tentacleHitFleck;
        public FloatRange swingRange = new FloatRange(1, 2), frequencyRange = new FloatRange(0.8f, 1.2f);
        public Color colorA = Color.white, colorB = Color.white;
    }
}
