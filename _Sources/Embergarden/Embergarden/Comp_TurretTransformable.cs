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
        public CompProperties_Transformable Prop => (CompProperties_Transformable)props;
        public Building_TurretGun Turret => parent as Building_TurretGun;

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
            if (!Prop.autoAI && parent.Faction == Faction.OfPlayer)
            {
                return;
            }

            if (Turret == null)
            {
                return;
            }
            D.Message($"Ticked, Target is {Turret.CurrentTarget}, next Transformation Tick is {lastHaveTargetTick + Prop.idleSeconds.SecondsToTicks()}");
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
                else if (GenTicks.TicksGame >= lastHaveTargetTick + Prop.idleSeconds.SecondsToTicks())
                {
                    TryTransform();
                }
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastHaveTargetTick, "lastHaveTargetTick");
            Scribe_Deep.Look(ref innerPawn, "innerPawn", [this]);
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
            return innerPawn;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (needUpdateHP)
            {
                UpdateHP();
            }
        }
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (absorbed || InnerPawn == null)
            {
                Log.Error("InnerPawn is null");
                return;
            }
            Log.Message(InnerPawn.def);
            InnerPawn.TakeDamage(dinfo);
            if (InnerPawn.Dead)
            {
                Log.Message(InnerPawn.def);
                Log.Warning("GetDamaged");
                var p = InnerPawn;
                innerPawn.Remove(p);
                Corpse corpse = (Corpse)ThingMaker.MakeThing(p.RaceProps.corpseDef);
                corpse.InnerPawn = p;
                GenSpawn.Spawn(corpse, parent.Position, parent.Map, WipeMode.Vanish);
                parent.Destroy(DestroyMode.KillFinalize);
            }
            parent.HitPoints = (int)InnerPawn.health.summaryHealth.SummaryHealthPercent * parent.MaxHitPoints;
            absorbed = true;
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
                    defaultLabel = Prop.defaultLabel.Translate(),
                    defaultDesc = Prop.defaultDesc.Translate(),
                    icon = Prop.Icon2D,
                    action = TryTransform,
                };
                yield return command;
            }
        }
        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"/n{InnerPawn}";
        }
        public override string GetDescriptionPart()
        {
            return base.GetDescriptionPart() + $"/n{InnerPawn}";
        }
        private void UpdateHP()
        {
            if (InnerPawn == null) return;
            parent.HitPoints =
                (int)InnerPawn.health.summaryHealth.SummaryHealthPercent
                * parent.MaxHitPoints + 1;
            if (needUpdateHP) needUpdateHP = false;
        }
        public void TryTransform()
        {
            innerPawn.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Direct);
            parent.Destroy(DestroyMode.WillReplace);
        }


        public Pawn InnerPawn => innerPawn.Any ? innerPawn[0] : null;

        public ThingOwner<Pawn> innerPawn;
        public Comp_TurretTransformable()
        {
            innerPawn = new(this);
        }
    }
}
