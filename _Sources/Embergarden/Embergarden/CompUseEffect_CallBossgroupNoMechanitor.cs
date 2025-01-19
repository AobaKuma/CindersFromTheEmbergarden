using RimWorld;
using Verse;

namespace Embergarden
{
    public class CompUseEffect_CallBossgroupNoMechanitor : CompUseEffect_CallBossgroup
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            return Props.bossgroupDef.Worker.CanResolve(p);
        }
    }
}
