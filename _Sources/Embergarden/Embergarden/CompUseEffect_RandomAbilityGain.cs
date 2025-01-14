using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Embergarden
{
    public class CompUseEffect_RandomAbilityGain : CompUseEffect
    {
        CompProperties_UseEffect_RandomAbilityGain Props => props as CompProperties_UseEffect_RandomAbilityGain;

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            foreach (var a in Props.abilityDefs)
            {
                if (p.abilities.GetAbility(a) == null)
                {
                    return base.CanBeUsedBy(p);
                }
            }

            return "Cinder_AllAbilityGained".Translate();
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            usedBy.abilities.GainAbility(Props.abilityDefs.Where(a => usedBy.abilities.GetAbility(a) == null).RandomElement());
        }
    }

    public class CompProperties_UseEffect_RandomAbilityGain : CompProperties_UseEffect
    {
        public CompProperties_UseEffect_RandomAbilityGain()
        {
            compClass = typeof(CompUseEffect_RandomAbilityGain);
        }

        public List<AbilityDef> abilityDefs = new List<AbilityDef>();
    }
}
