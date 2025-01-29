using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Embergarden
{
    public class HediffComp_PreApplyDamage: HediffComp
    {
        public override void CompPostMake()
        {
            base.CompPostMake();
            AddPawnComp();
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AddPawnComp();
            }
        }
        public void AddPawnComp()
        {
            if (!parent.pawn.TryGetComp<Comp_PreApplyDamage>(out var _))
            {
                parent.pawn.AllComps.Add(new Comp_PreApplyDamage() { parent = parent.pawn });
            }
        }
        public virtual void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
        }
    }
    public class HediffComp_ProtectiveShield : HediffComp_PreApplyDamage
    {
        public float DurablePercent => Hitpoints / MaxHitpoints;
        public float MaxHitpoints => maxHitpoints == 0 ? maxHitpoints = (int)(Props.hitpoints * parent.pawn.BodySize) : maxHitpoints;
        public float Hitpoints
        {
            get { return hitpoints; }
            set {
                if(value>MaxHitpoints) value = MaxHitpoints;
                parent.Severity = DurablePercent;
                hitpoints = value;
            }
        }
        private int maxHitpoints;
        private float hitpoints;
        public HediffCompProperties_ProtectiveShield Props
        {
            get
            {
                return (HediffCompProperties_ProtectiveShield)props;
            }
        }
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);

            if (!dinfo.Def.harmsHealth)
            {
                absorbed = true;
                return;
            }

            if (Hitpoints > 0)
            {
                var dmg = dinfo.Amount;
                var dmgReduced = dmg - Hitpoints;
                if (dmgReduced <= 0)
                {
                    absorbed = true;
                    dmgReduced = 0;
                }
                dinfo.SetAmount(dmgReduced);
                Hitpoints -= dmg;
                Props.effectOnDamaged?.SpawnMaintained(parent.pawn.Position, parent.pawn.MapHeld, 0.2f);
                FilthMaker.TryMakeFilth(GenAdjFast.AdjacentCells8Way(parent.pawn.Position).RandomElement().ClampInsideMap(parent.pawn.MapHeld), parent.pawn.MapHeld, Props.filthOnDamaged);

            }
            if (Hitpoints <=0)
            {
                Hitpoints = 0;
                //Messages.Message("Embergarden_AddonBroken".Translate(), new LookTargets(parent.pawn.PositionHeld, parent.pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                parent.pawn.health.RemoveHediff(parent);
            }

        }
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (base.CompGetGizmos()!=null)
            {
                foreach (Gizmo item in base.CompGetGizmos())
                {
                    yield return item;
                }
            }
            
            foreach (Gizmo gizmo in GetGizmos())
            {
                yield return gizmo;
            }
        }
        private IEnumerable<Gizmo> GetGizmos()
        {
            if ((parent.pawn.Faction == Faction.OfPlayer || (parent.pawn.RaceProps.IsMechanoid)) && Find.Selector.SingleSelectedThing == parent.pawn)
            {
                Gizmo_AttachmentShieldStatus gizmo_Shield = new Gizmo_AttachmentShieldStatus
                {
                    shield = this
                };
                yield return gizmo_Shield;
            }
        }        
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref hitpoints, "hitpoints");
        }
        public override void CompPostMake()
        {
            base.CompPostMake();
            hitpoints = Props.hitpoints;
        }

        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            Hitpoints += other.TryGetComp<HediffComp_ProtectiveShield>().Hitpoints;
        }
    }
    public class HediffCompProperties_ProtectiveShield : HediffCompProperties
    {
        public ThingDef filthOnDamaged;
        public EffecterDef effectOnDamaged;
        public int hitpoints;
        public HediffCompProperties_ProtectiveShield()
        {
            compClass = typeof(HediffComp_ProtectiveShield);
        }
    }

    public class Comp_PreApplyDamage : ThingComp
    {
        public IEnumerable<HediffComp_PreApplyDamage> HediffsPreApplyDamage=>((Pawn) parent)?.health.hediffSet.GetHediffComps<HediffComp_PreApplyDamage>();
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            foreach(var h in HediffsPreApplyDamage)
            {
                h.PreApplyDamage(ref dinfo, out absorbed);
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            //Scribe_Collections.Look(ref hediffs, "hediffs", LookMode.Reference);
            
        }
        //public List<HediffComp_PreApplyDamage> hediffs;
    }
}
