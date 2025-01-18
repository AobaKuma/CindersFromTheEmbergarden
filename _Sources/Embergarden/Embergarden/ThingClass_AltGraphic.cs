using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class ThingClass_AltGraphic : ThingWithComps
    {
        IAltFireMode altFireMode;

        IAltFireMode AltFireMode
        {
            get
            {
                altFireMode ??= GetComps<IAltFireMode>().FirstOrFallback(null);
                return altFireMode;
            }
        }

        DefModExtension_WeaponAltGraphic ext;

        DefModExtension_WeaponAltGraphic Ext
        {
            get
            {
                ext ??= def.GetModExtension<DefModExtension_WeaponAltGraphic>();
                return ext;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (AltFireMode != null && Ext != null && AltFireMode.IsSecondaryVerbSelected)
                {
                    return Ext.graphicData.Graphic;
                }
                return DefaultGraphic;
            }
        }
    }

    public class DefModExtension_WeaponAltGraphic : DefModExtension
    {
        public GraphicData graphicData;
    }

    public class StatPart_SecondaryMode : StatPart
    {
        [MustTranslate]
        public string desc;

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing) return null;
            var altFireMode = (req.Thing as ThingWithComps).GetComps<IAltFireMode>().FirstOrFallback(null);
            if (altFireMode != null)
            {
                foreach (var stofs in altFireMode.statOffsets)
                {
                    if (stofs.stat == this.parentStat)
                    {
                        return desc.Formatted(stofs.value.ToString().Named("OFFSET"));
                    }
                }
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing) return;
            var altFireMode = (req.Thing as ThingWithComps).GetComps<IAltFireMode>().FirstOrFallback(null);
            if (altFireMode != null)
            {
                foreach (var stofs in altFireMode.statOffsets)
                {
                    if (stofs.stat == this.parentStat)
                    {
                        val += stofs.value;
                    }
                }
            }
        }
    }
}
