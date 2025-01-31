using CombatExtended;
using System.Collections.Generic;
using Embergarden;
using RimWorld;
using Verse;

namespace EmbergardenCE
{
    public class CompSecondaryVerbCE : CompUnderBarrel, IAltFireMode
    {
        new CompProperties_SecondaryVerbCE Props => props as CompProperties_SecondaryVerbCE;

        public bool IsSecondaryVerbSelected => usingUnderBarrel;

        public List<StatModifier> statOffsets => Props.statOffsets;
    }

    public class CompProperties_SecondaryVerbCE : CompProperties_UnderBarrel
    {

        public List<StatModifier> statOffsets = new List<StatModifier>();

        public CompProperties_SecondaryVerbCE()
        {
            this.compClass = typeof(CompSecondaryVerbCE);
        }
    }

    public class CompAmmoUserS : CompAmmoUser { }
}
