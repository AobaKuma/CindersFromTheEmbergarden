using Verse;

namespace Embergarden
{
    public class Hediff_DownedAction : HediffWithComps
    {
        public override void Notify_Downed()
        {
            base.Notify_Downed();
            for (int num = comps.Count - 1; num >= 0; num--)
            {
                if (comps[num] is HediffComp_DownedAction c) c.Notify_Downed();
            }
        }
    }

    public abstract class HediffComp_DownedAction : HediffComp
    {
        public abstract void Notify_Downed();
    }
}
