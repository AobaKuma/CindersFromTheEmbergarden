using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace Embergarden
{
    /// <summary>
    /// 移动炮塔 建筑形态Comp
    /// </summary>
    public abstract partial class Comp_TurretTransformableAbstract : ThingComp
    {
        public CompProperties_Transformable Props => (CompProperties_Transformable)props;
        public Building Turret => parent as Building;

        public abstract LocalTargetInfo CurrentTarget { get; }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            needUpdateHP = true;
        }

        public void UpdateRepairCache()
        {
            parent.Map.listerBuildingsRepairable.Notify_BuildingTookDamage(Turret);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (needUpdateHP && InnerPawn == null)
            {
                NewPawn();
            }
            pawnOwner.ThingOwnerTick();

            if (needCheckRepair && parent.HitPoints >= parent.MaxHitPoints)
            {
                WoundPurge(InnerPawn);
            }
            if (needUpdateHP)
            {
                UpdateHP();
            }
            if (Turret.IsHashIntervalTick(250))
            {
                TransformTick();
            }
        }
        public void TransformTick()
        {
            if (!Props.autoAI && parent.Faction == Faction.OfPlayer)
            {
                return;
            }

            if (Turret == null)
            {
                return;
            }
            if (CurrentTarget.IsValid)
            {
                if (lastHaveTargetTick > 0)
                    lastHaveTargetTick = -1;
            }
            else
            {
                if (lastHaveTargetTick == -1)
                {
                    lastHaveTargetTick = GenTicks.TicksGame;
                }
                else if (GenTicks.TicksGame >= lastHaveTargetTick + Props.idleSeconds.SecondsToTicks())
                {
                    TryTransform();
                }
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastHaveTargetTick, "lastHaveTargetTick");
            Scribe_Deep.Look(ref pawnOwner, "innerPawn", [this]);
        }
        int lastHaveTargetTick = -1;
        bool needUpdateHP = true;
        bool needCheckRepair = true;
    }

    public abstract partial class Comp_TurretTransformableAbstract : IThingHolder
    {
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            return;
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return pawnOwner;
        }
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (absorbed || InnerPawn == null)
            {
                return;
            }
            InnerPawn.TakeDamage(dinfo);
            UpdateHP();
            absorbed = true;
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (mode == DestroyMode.KillFinalize)
            {
                if (InnerPawn != null && !InnerPawn.Dead)
                {
                    Pawn p = InnerPawn;
                    pawnOwner.TryDropAll(parent.Position, previousMap, ThingPlaceMode.Near);
                    p.Kill(null);
                }
                var comp = parent.GetComp<CompExplosive>();
                if (comp != null)
                {
                    Log.Message("detonate");
                    typeof(CompExplosive).GetMethod("Detonate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(comp, [previousMap, false]);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent.Faction == Faction.OfPlayer && InnerPawn != null)
            {
                Command_Action command = new()
                {
                    defaultLabel = Props.defaultLabel,
                    defaultDesc = Props.defaultDesc,
                    icon = Props.Icon2D,
                    action = TryTransform,
                };
                yield return command;
            }
        }
        private void UpdateHP()
        {
            if (InnerPawn == null || InnerPawn.Dead)
            {
                pawnOwner.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
                parent.Kill(null);
                return;
            }
            parent.HitPoints = (int)Mathf.Clamp(InnerPawn.health.summaryHealth.SummaryHealthPercent * parent.MaxHitPoints, 1, parent.MaxHitPoints);
            needUpdateHP = false;
            needCheckRepair = true;
            UpdateRepairCache();
        }
        public void TryTransform()
        {
            if (InnerPawn == null) NewPawn();
            Pawn p = InnerPawn;
            p.SetFactionDirect(parent.Faction);
            pawnOwner.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Direct);
            bool flag = Find.Selector.SelectedObjectsListForReading.Contains(parent);
            parent.DeSpawn(DestroyMode.WillReplace);
            foreach (var ab in p.abilities.abilities)
            {
                if (ab.CompOfType<AbilityCompEffect_Transform>() is AbilityCompEffect_Transform comp)
                {
                    comp.Turret = Turret;
                }
            }

            if (p.IsPlayerControlled) p.drafter.Drafted = true;
            if (flag)
            {
                Find.Selector.Select(p);
            }
        }

        public Pawn NewPawn()
        {
            if (pawnOwner.Any)
            {
                while (pawnOwner.Count > 1)
                {
                    pawnOwner.RemoveAt(1);
                }
                return null;
            }
            Pawn p = PawnGenerator.GeneratePawn(Props.pawnKind, parent.Faction);
            pawnOwner.TryAdd(p);
            return p;
        }
        public void WoundPurge(Pawn pawn)
        {
            for (int i = pawn.health.hediffSet.hediffs.Count; i > 0; i--)
            {
                var h = pawn.health.hediffSet.hediffs[i - 1];
                if (h is Hediff_Injury || h is Hediff_MissingPart)
                {
                    pawn.health.RemoveHediff(h);
                }
            }
            needCheckRepair = false;
        }

        public Pawn InnerPawn => pawnOwner.Any ? pawnOwner[0] : null;

        public ThingOwner<Pawn> pawnOwner;
        public Comp_TurretTransformableAbstract()
        {
            pawnOwner = new(this, true);
        }
    }

    public class Comp_TurretTransformable : Comp_TurretTransformableAbstract
    {
        public override LocalTargetInfo CurrentTarget => (Turret as Building_TurretGun).CurrentTarget;
    }
}
