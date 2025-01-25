using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class CompAbilityEffect_ScorchWound : CompAbilityEffect
    {
        CompProperties_ScorchWound Props => props as CompProperties_ScorchWound;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn != null)
            {
                base.Apply(target, dest);
                Pawn Pawn = target.Pawn;
                List<Hediff> hediffs = new List<Hediff>();
                foreach (Hediff hediff in Pawn.health.hediffSet.hediffs)
                {
                    if (hediff.Bleeding)
                    {
                        hediffs.Add(hediff);
                    }
                }

                foreach (var hediff in hediffs)
                {
                    float severity = hediff.Severity;
                    var part = hediff.Part;

                    Hediff_Injury replacing = HediffMaker.MakeHediff(Props?.replacingHediff ?? Cider_DefOf.Burn, Pawn, part) as Hediff_Injury;
                    Pawn.health.RemoveHediff(hediff);
                    Pawn.health.AddHediff(replacing, part);
                    replacing.Severity = severity;
                    replacing.Tended(Props?.tendQuality ?? 0.5f, Props?.tendQuality ?? 0.5f);
                }
            }
        }
    }


    public class CompProperties_ScorchWound : CompProperties_AbilityEffect
    {
        public CompProperties_ScorchWound()
        {
            compClass = typeof(CompAbilityEffect_ScorchWound);
        }
        public HediffDef replacingHediff;

        public float tendQuality = 0.5f;
    }

    public class CompAbilityEffect_AIGiveMentalState : CompAbilityEffect_GiveMentalState
    {
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return Valid(target);
        }
    }

    public class CompAbilityEffect_ConnectingFleckLineAI : CompAbilityEffect_ConnectingFleckLine
    {
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }
}
