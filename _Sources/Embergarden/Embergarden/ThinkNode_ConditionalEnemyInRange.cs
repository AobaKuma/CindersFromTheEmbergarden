using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Embergarden
{
    public class ThinkNode_ConditionalEnemyInRange : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            Verb verb = pawn.CurrentEffectiveVerb;
            var t = pawn.mindState.enemyTarget;
            if (verb == null || t == null)
            {
                return false;
            }
            return t.Position.DistanceToSquared(pawn.Position) < verb.verbProps.range * verb.verbProps.range;
        }
    }
}
