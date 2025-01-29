using RimWorld;
using Verse;

namespace Embergarden
{
    public class CompUseEffect_SummonRaid : CompUseEffect
    {
        CompPropertiesUseable_SummonRaid Props => props as CompPropertiesUseable_SummonRaid;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
            if (component != null)
            {
                Props.bossgroupDef.Worker.Resolve(parent.Map, component.NumTimesCalledBossgroup(Props.bossgroupDef));
            }
        }
    }

    public class CompPropertiesUseable_SummonRaid : CompProperties_UseEffect
    {
        public CompPropertiesUseable_SummonRaid()
        {
            compClass = typeof(CompUseEffect_SummonRaid);
        }

        public BossgroupDef bossgroupDef;
    }
}
