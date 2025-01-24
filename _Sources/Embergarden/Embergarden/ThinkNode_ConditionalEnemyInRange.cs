using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Embergarden
{
    public class ThinkNode_HasCompAndConditionalEnemyInRange : ThinkNode_Conditional
    {
        public static Ability GetAbility(Pawn pawn)
        {
            if (pawn.abilities != null && pawn.abilities.abilities.Any())
            {
                foreach (var ab in pawn.abilities.abilities)
                {
                    if (ab.comps.ContainsAny(c => c.GetType() == typeof(AbilityCompEffect_Transform))) return ab;
                }
            }
            return null;
        }

        public static Verb GetVerb(Pawn pawn)
        {
            return GetAbility(pawn)?.verb ?? pawn.CurrentEffectiveVerb;
        }

        protected override bool Satisfied(Pawn pawn)
        {
            Verb verb = GetVerb(pawn);
            var t = pawn.mindState.enemyTarget;
            if (verb == null || t == null)
            {
                return false;
            }
            return verb.CanHitTarget(t) && t.Position.DistanceToSquared(pawn.Position) < verb.verbProps.range * verb.verbProps.range;
        }
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_HasCompAndConditionalEnemyInRange obj = (ThinkNode_HasCompAndConditionalEnemyInRange)base.DeepCopy(resolve);
            return obj;
        }
    }

    public class JobGiver_TurretWalkerTransform : JobGiver_AICastAbilityOnSelf
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            ability = ThinkNode_HasCompAndConditionalEnemyInRange.GetAbility(pawn).def;
            if (ability != null) return base.TryGiveJob(pawn);
            return null;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_TurretWalkerTransform obj = (JobGiver_TurretWalkerTransform)base.DeepCopy(resolve);
            return obj;
        }
    }
}
