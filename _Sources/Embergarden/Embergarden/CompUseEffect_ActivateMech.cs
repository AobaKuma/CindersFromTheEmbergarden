using RimWorld;
using Verse;
using static MechanitorUtility;

namespace Embergarden
{
    public class CompUseEffect_ActivateMech : CompUseEffect
    {
        CompProperties_UseEffect_ActivateMech Props => props as CompProperties_UseEffect_ActivateMech;
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (Props.requireMechanitor)
            {
                if (IsMechanitor(p))
                {
                    int num = p.mechanitor.TotalBandwidth - p.mechanitor.UsedBandwidth;
                    float statValue = Props.pawnKindDef.race.GetStatValueAbstract(StatDefOf.BandwidthCost);
                    if (num < statValue)
                    {
                        return "CannotControlMechNotEnoughBandwidth".Translate();
                    }
                }
                else return "RequiresMechanitor".Translate();
            }
            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            var mech = PawnGenerator.GeneratePawn(Props.pawnKindDef, usedBy.Faction);
            if (EverControllable(mech) && IsMechanitor(usedBy))
            {
                mech.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, mech);
                usedBy.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
            }
            GenPlace.TryPlaceThing(mech, parent.PositionHeld, parent.MapHeld, ThingPlaceMode.Near);
            parent.DeSpawn();
            parent.Destroy();
        }
    }

    public class CompProperties_UseEffect_ActivateMech : CompProperties_UseEffect
    {
        public CompProperties_UseEffect_ActivateMech()
        {
            compClass = typeof(CompUseEffect_ActivateMech);
        }

        public PawnKindDef pawnKindDef;

        public bool requireMechanitor = true;
    }
}
