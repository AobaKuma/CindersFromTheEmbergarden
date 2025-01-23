using System;
using System.Runtime.CompilerServices;
using Verse;

namespace Embergarden
{
    public class Hediff_DynamicCap : HediffWithComps
    {
        public override HediffStage CurStage => stage ??= GenerateStage();
        public RandomCapacityModifier Ext => ext ??= def.GetModExtension<RandomCapacityModifier>();
        private RandomCapacityModifier ext;
        private HediffStage stage;
        private PawnCapacityModifier capMod;
        private Random random;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            RandomizeCap();
        }

        private HediffStage GenerateStage(bool loading = false)
        {
            if (Ext != null)
            {
                var st = def.stages[0];
                var newstage = new HediffStage()
                {
                    minSeverity = st.minSeverity,
                    label = st.label,
                    overrideLabel = st.overrideLabel,
                    untranslatedLabel = st.untranslatedLabel,
                    becomeVisible = st.becomeVisible,
                    lifeThreatening = st.lifeThreatening,
                    tale = st.tale,
                    vomitMtbDays = st.vomitMtbDays,
                    deathMtbDays = st.deathMtbDays,
                    mtbDeathDestroysBrain = st.mtbDeathDestroysBrain,
                    painFactor = st.painFactor,
                    painOffset = st.painOffset,
                    totalBleedFactor = st.totalBleedFactor,
                    naturalHealingFactor = st.naturalHealingFactor,
                    regeneration = st.regeneration,
                    showRegenerationStat = st.showRegenerationStat,
                    forgetMemoryThoughtMtbDays = st.forgetMemoryThoughtMtbDays,
                    pctConditionalThoughtsNullified = st.pctConditionalThoughtsNullified,
                    pctAllThoughtNullification = st.pctAllThoughtNullification,
                    opinionOfOthersFactor = st.opinionOfOthersFactor,
                    fertilityFactor = st.fertilityFactor,
                    hungerRateFactor = st.hungerRateFactor,
                    hungerRateFactorOffset = st.hungerRateFactorOffset,
                    restFallFactor = st.restFallFactor,
                    restFallFactorOffset = st.restFallFactorOffset,
                    socialFightChanceFactor = st.socialFightChanceFactor,
                    foodPoisoningChanceFactor = st.foodPoisoningChanceFactor,
                    mentalBreakMtbDays = st.mentalBreakMtbDays,
                    mentalBreakExplanation = st.mentalBreakExplanation,
                    blocksMentalBreaks = st.blocksMentalBreaks,
                    blocksInspirations = st.blocksInspirations,
                    overrideMoodBase = st.overrideMoodBase,
                    severityGainFactor = st.severityGainFactor,
                    allowedMentalBreakIntensities = st.allowedMentalBreakIntensities,
                    makeImmuneTo = st.makeImmuneTo,
                    hediffGivers = st.hediffGivers,
                    mentalStateGivers = st.mentalStateGivers,
                    statOffsets = st.statOffsets,
                    statFactors = st.statFactors,
                    statOffsetsBySeverity = st.statOffsetsBySeverity,
                    statFactorsBySeverity = st.statFactorsBySeverity,
                    damageFactors = st.damageFactors,
                    multiplyStatChangesBySeverity = st.multiplyStatChangesBySeverity,
                    statOffsetEffectMultiplier = st.statOffsetEffectMultiplier,
                    statFactorEffectMultiplier = st.statFactorEffectMultiplier,
                    capacityFactorEffectMultiplier = st.capacityFactorEffectMultiplier,
                    disabledWorkTags = st.disabledWorkTags,
                    overrideTooltip = st.overrideTooltip,
                    extraTooltip = st.extraTooltip,
                    blocksSleeping = st.blocksSleeping,
                    partEfficiencyOffset = st.partEfficiencyOffset,
                    partIgnoreMissingHP = st.partIgnoreMissingHP,
                    destroyPart = st.destroyPart
                };
                capMod = new PawnCapacityModifier() { capacity = Ext.capacity };
                newstage.capMods ??= [];
                newstage.capMods.Add(capMod);
                foreach (var mod in st.capMods)
                {
                    if (mod.capacity != Ext.capacity)
                    {
                        newstage.capMods.Add(mod);
                    }
                }
                if (Ext.offset)
                {
                    capMod.offset *= severityInt;
                }
                else
                {
                    capMod.postFactor *= severityInt;
                }
                return newstage;
            }
            pawn.health.RemoveHediff(this);
            return new HediffStage { label = "Error" };
        }

        public void RandomizeCap()
        {
            random ??= new Random(pawn.thingIDNumber);
            severityInt = (float)random.NextDouble() * (Ext.range.max - Ext.range.min) + Ext.range.min;
            stage = null;
        }
    }
    public class RandomCapacityModifier : DefModExtension
    {
        public PawnCapacityDef capacity;
        public FloatRange range;
        public bool offset = true;
    }
}
