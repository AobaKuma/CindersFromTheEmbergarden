using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse.AI.Group;

namespace Embergarden
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class IncidentWorker_RaidPatch
    {
        public static void Prefix(ref IncidentParms parms)
        {
            if (parms.faction.def.GetModExtension<ModExtension_FactionBehavior>() is ModExtension_FactionBehavior modExtension)
            {
                parms.canSteal = modExtension.steal;
                parms.canKidnap = modExtension.canKidnap;
                parms.canTimeoutOrFlee = modExtension.timeoutFlee;
            }
        }
    }

    public class ModExtension_FactionBehavior : DefModExtension
    {
        public bool steal = false;

        public bool canKidnap = false;

        public bool timeoutFlee = false;
    }
}
