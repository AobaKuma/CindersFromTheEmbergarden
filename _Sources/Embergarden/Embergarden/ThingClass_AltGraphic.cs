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
}
