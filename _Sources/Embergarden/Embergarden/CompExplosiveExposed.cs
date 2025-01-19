using RimWorld;
using Verse;

namespace Embergarden
{
    public class CompExplosiveExposed : CompExplosive
    {
        public void DoDetonate(Map map, bool ignoreUnspawned = false)
        {
            Detonate(map, ignoreUnspawned);
        }
    }
}
