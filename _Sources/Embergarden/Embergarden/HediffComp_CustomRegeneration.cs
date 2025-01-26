using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Embergarden
{
    public class HediffComp_Regen : HediffComp
    {
        HediffCompProperties_Regen Props => base.props as HediffCompProperties_Regen;

        float healPerDay => Unlocked ? Props.healPerDayUnlocked : Props.healPerDay;

        float healPerCycle => healPerDay * Props.checkInterval / 60000;

        public bool Unlocked = false;

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref Unlocked, "Unlocked");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(Props.checkInterval))
            {
                List<Hediff> hediffsToHeal = [];
                foreach (Hediff hediff in Pawn.health.hediffSet.hediffs.InRandomOrder())
                {
                    if (hediff is Hediff_Injury)
                    {
                        hediffsToHeal.Add(hediff);
                        continue;
                    }
                    if (hediff is Hediff_MissingPart missingPart && Pawn.health.hediffSet.GetMissingPartFor(missingPart.Part.parent) == null && !Pawn.health.hediffSet.GetInjuredParts().Contains(missingPart.Part.parent))
                    {
                        hediffsToHeal.Add(hediff);
                    }
                }
                if (hediffsToHeal.Any())
                {
                    float heals = healPerCycle;
                    int count = hediffsToHeal.Count;
                    foreach (var hediffToHeal in hediffsToHeal)
                    {
                        if (heals <= 0) break;
                        if (hediffToHeal is Hediff_Injury)
                        {
                            float actualHeal = Math.Min(heals / count, hediffToHeal.Severity);
                            hediffToHeal.Severity -= actualHeal;
                            if (hediffToHeal.Severity <= 0)
                            {
                                Pawn.health.RemoveHediff(hediffToHeal);
                            }
                            heals -= actualHeal;
                            count--;
                            continue;
                        }
                        if (hediffToHeal is Hediff_MissingPart missingPart)
                        {
                            if (CannotRegenPart(missingPart.Part)) continue;
                            float severity = missingPart.Part.def.GetMaxHealth(Pawn);
                            float actualHeal = heals / count;
                            if (actualHeal < severity)
                            {
                                var part = missingPart.Part;
                                Hediff replacing = HediffMaker.MakeHediff(missingPart.lastInjury ?? HediffDefOf.Cut, Pawn, part);
                                Pawn.health.RemoveHediff(hediffToHeal);
                                Pawn.health.AddHediff(replacing, part);
                                replacing.Severity = severity - actualHeal;
                                heals -= actualHeal;
                            }
                            else
                            {
                                heals -= severity;
                                Pawn.health.RemoveHediff(hediffToHeal);
                            }
                        }
                    }
                }
            }
        }

        bool CannotRegenPart(BodyPartRecord part)
        {
            if (Unlocked) return false;
            if (part.def.hitPoints <= Props.maxPartHpForLocked) return false;
            return true;
        }


        public override string CompLabelPrefix => Unlocked ? Props.unlockedString : null;
        public override string CompLabelInBracketsExtra => $"{healPerDay} HP/d";
    }

    public class HediffCompProperties_Regen : HediffCompProperties
    {
        public int checkInterval = 600;

        public float healPerDay = 1;

        public float healPerDayUnlocked = 10;

        public int maxPartHpForLocked = 10;

        [MustTranslate]
        public string unlockedString;

        public HediffCompProperties_Regen()
        {
            compClass = typeof(HediffComp_Regen);
        }
    }

    public class IngestionOutcomeDoer_UnlockRegenLimit : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            foreach (var hediff in pawn.health.hediffSet.GetHediffComps<HediffComp_Regen>())
            {
                hediff.Unlocked = true;
            }
        }
    }
}
