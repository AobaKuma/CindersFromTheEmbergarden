using RimWorld;
using Verse;

namespace Embergarden
{
    public class ThoughtWorker_OtherHasTrait : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (def.GetModExtension<ThoughtWorkerExt>() is ThoughtWorkerExt ext && other.story.traits.HasTrait(ext.traitdef))
            {
                return true;
            }
            return false;
        }
    }

    public class ThoughtWorkerExt : DefModExtension
    {
        public TraitDef traitdef;
    }
}
