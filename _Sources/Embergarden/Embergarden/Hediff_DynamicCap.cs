using System;
using Verse;

namespace Embergarden
{
    public class Hediff_DynamicCap : HediffWithComps
    {
        public override HediffStage CurStage=>stage??= GenerateStage();
        public RandomCapacityModifier Ext => ext??=def.GetModExtension<RandomCapacityModifier>();
        private RandomCapacityModifier ext;
        private HediffStage stage;
        private PawnCapacityModifier capMod;
        private Random random;
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
                    capMods = st.capMods,
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
                capMod = newstage.capMods?.Find(m => m.capacity == Ext.capacity);
                if (capMod == null)
                {
                    capMod = new PawnCapacityModifier() { capacity = Ext.capacity };
                    newstage.capMods ??= [];
                    newstage.capMods.Add(capMod);
                }
                RandomizeCap(loading);
                return newstage;
            }
            pawn.health.RemoveHediff(this);
            return new HediffStage { label = "Error"};
        }
        
        public void RandomizeCap(bool loading = false)
        {
            if (capMod == null || Ext.secondsPerRandomize == default && !loading) return;
            random ??= new Random(pawn.thingIDNumber);
            if (!loading)
            {
                value = (float)random.NextDouble() * (Ext.range.max - Ext.range.min) + Ext.range.min;
            }
            if (Ext.offset)
            {
                capMod.offset = value;
            }
            else
            {
                capMod.postFactor = value;
            }
            nextRandomizeTick = Ext.secondsPerRandomize.SecondsToTicks() + GenTicks.TicksGame;
        }
        public override void Tick()
        {
            base.Tick();
            if (GenTicks.TicksGame >= nextRandomizeTick)
            {
                RandomizeCap();
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref nextRandomizeTick,"nextTick");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                GenerateStage(true);
            }
        }

        private int nextRandomizeTick;
        private float value;
    }
    public class RandomCapacityModifier:DefModExtension
    {
        public PawnCapacityDef capacity;
        public FloatRange range;
        public bool offset = true;
        public float secondsPerRandomize;
    }
}
