using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class DefModExtension_MeleeConeAOE : DefModExtension
    {
        public float HalfAngel = 60f;
        public float radius = 2.5f;
        public DamageDef damageDef;
        public int damageAmount = 20;
        public float armorPenetration = 0.5f;
        public float screenShakeFactor = 0f;
        public bool doSoundEffects = false;

        public bool IsValid => !float.IsNaN(HalfAngel) && HalfAngel > 0f && HalfAngel <= 180f
            && !float.IsNaN(radius) && !float.IsInfinity(radius) && radius > 0f
            && damageDef != null && damageAmount >= 0
            && !float.IsNaN(armorPenetration) && !float.IsInfinity(armorPenetration) && armorPenetration >= 0f
            && !float.IsNaN(screenShakeFactor) && !float.IsInfinity(screenShakeFactor) && screenShakeFactor >= 0f;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (float.IsNaN(HalfAngel) || HalfAngel <= 0f || HalfAngel > 180f)
                yield return "MeleeConeAOE: HalfAngel must be in (0, 180] degrees";
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                yield return "MeleeConeAOE: radius must be finite and positive";
            if (damageDef == null) yield return "MeleeConeAOE: damageDef must be specified";
            if (damageAmount < 0) yield return "MeleeConeAOE: damageAmount must not be negative";
            if (float.IsNaN(armorPenetration) || float.IsInfinity(armorPenetration) || armorPenetration < 0f)
                yield return "MeleeConeAOE: armorPenetration must be finite and nonnegative";
            if (float.IsNaN(screenShakeFactor) || float.IsInfinity(screenShakeFactor) || screenShakeFactor < 0f)
                yield return "MeleeConeAOE: screenShakeFactor must be finite and nonnegative";
        }
    }
}
