using RimWorld;
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
                var ro = hediffs.ToArray();
                foreach (var resurrect in ro)
                {
                    resurrect.Resurrect();
                    hediffs.Remove(resurrect);
                }
            }
        }

        public override void LoadedGame()
        {
            var def = DefDatabase<FactionDef>.GetNamed("Cinder_ESDSP", false);
            if (def != null && Find.FactionManager.FirstFactionOfDef(def) == null)
            {
                Log.Warning("[Cinder from the Embergarden] Cradle faction missing. Recreating");
                FactionGenerator.GenerateFactionsIntoWorld([def]);
            }
        }
    }
}
