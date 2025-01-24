using UnityEngine;
using Verse;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    public class CompProperties_Transformable : CompProperties
    {
        //For Comp_TurretTransformable
        public float idleSeconds;
        public bool autoAI;
        public PawnKindDef pawnKind;

        public string defaultLabel;
        public string defaultDesc;
        public string icon;

        Texture2D icon2D;
        public Texture2D Icon2D
        {
            get
            {
                if (!icon.NullOrEmpty())
                {
                    icon2D = new CachedTexture(icon).Texture;
                }
                return icon2D;
            }
        }
        public override void PostLoadSpecial(ThingDef parent)
        {
            base.PostLoadSpecial(parent);
        }
    }
}
