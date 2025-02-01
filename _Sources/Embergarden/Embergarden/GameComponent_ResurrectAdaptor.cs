using System.Collections.Generic;
using Verse;

namespace Embergarden
{
    public class GameComponent_ResurrectAdaptor : GameComponent
    {
        public GameComponent_ResurrectAdaptor(Game game)
        {
        }

        public List<HediffComp_LimitedResurrect> hediffs = [];

        public override void GameComponentTick()
        {
            if (hediffs.Count > 0)
            {
                IReadOnlyCollection<HediffComp_LimitedResurrect> ro = hediffs;
                foreach (var resurrect in ro)
                {
                    Log.Message(Find.TickManager.TicksGame);
                    hediffs.Remove(resurrect);
                }
            }
        }
    }
}
