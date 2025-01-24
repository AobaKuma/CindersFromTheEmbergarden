using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Embergarden
{
    /// <summary>
    /// 移动炮塔 建筑形态Comp
    /// </summary>
    public partial class Comp_TurretTransformable : ThingComp
    {
        public CompProperties_Transformable Props => (CompProperties_Transformable)props;
        public Building_TurretGun Turret => parent as Building_TurretGun;

        public override void PostPostMake()
        {
            base.PostPostMake();
            NewPawn();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (InnerPawn == null)
            {
                parent.Destroy();
                return;
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
            D.Message($"Ticked, Target is {Turret.CurrentTarget}, next Transformation Tick is {lastHaveTargetTick + Props.idleSeconds.SecondsToTicks()}");
            if (Turret.CurrentTarget.IsValid)
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
    }
    public partial class Comp_TurretTransformable : IThingHolder
    {
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            return;
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return pawnOwner;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateHP();
        }
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (absorbed || InnerPawn == null)
            {
                return;
            }
            InnerPawn.TakeDamage(dinfo);
            if (InnerPawn.Dead)
            {
                var p = InnerPawn;
                pawnOwner.Remove(p);
                Corpse corpse = (Corpse)ThingMaker.MakeThing(p.RaceProps.corpseDef);
                corpse.InnerPawn = p;
                GenSpawn.Spawn(corpse, parent.Position, parent.Map, WipeMode.Vanish);
                parent.Destroy(DestroyMode.KillFinalize);
            }
            UpdateHP();
            absorbed = true;
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (mode == DestroyMode.KillFinalize && !InnerPawn.Dead)
            {
                Pawn p = InnerPawn;
                pawnOwner.TryDropAll(parent.Position, previousMap, ThingPlaceMode.Near);
                p.Kill(null);
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
                    defaultLabel = Props.defaultLabel.Translate(),
                    defaultDesc = Props.defaultDesc.Translate(),
                    icon = Props.Icon2D,
                    action = TryTransform,
                };
                yield return command;
            }
        }
        private void UpdateHP()
        {
            if (InnerPawn == null) return;
            parent.HitPoints = (int)Mathf.Clamp(InnerPawn.health.summaryHealth.SummaryHealthPercent * parent.MaxHitPoints, 1, parent.MaxHitPoints);
            if (needUpdateHP) needUpdateHP = false;
        }
        public void TryTransform()
        {
            if (InnerPawn == null) NewPawn();
            Pawn p = InnerPawn;
            p.SetFactionDirect(parent.Faction);
            pawnOwner.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Direct);
            parent.Destroy(DestroyMode.WillReplace);
            if (p.IsPlayerControlled) p.drafter.Drafted = true;
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

        public Pawn InnerPawn => pawnOwner.Any ? pawnOwner[0] : null;

        public ThingOwner<Pawn> pawnOwner;
        public Comp_TurretTransformable()
        {
            pawnOwner = new(this, true);
        }
    }
}
