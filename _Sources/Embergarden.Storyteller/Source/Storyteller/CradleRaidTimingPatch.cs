using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Embergarden.Storytelling
{
    // 原版天数筛选无独立回调
    [HarmonyPatch]
    public static class CradleRaidTimingPatch
    {
        private const float NoRaidDelay = 0f;
        private static readonly FieldInfo EarliestRaidDaysField =
            AccessTools.Field(typeof(FactionDef), nameof(FactionDef.earliestRaidDays));
        private static readonly MethodInfo ResolveRaidDaysMethod =
            AccessTools.Method(typeof(CradleRaidTimingPatch), nameof(EarliestRaidDays));

        private static MethodBase TargetMethod()
        {
#if RW15
            var parameters = new[] { typeof(Faction), typeof(Map), typeof(bool) };
#else
            var parameters = new[] { typeof(Faction), typeof(IncidentParms), typeof(bool) };
#endif
            return AccessTools.Method(typeof(IncidentWorker_RaidEnemy),
                "FactionCanBeGroupSource", parameters);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> result = instructions.ToList();
            if (result.Count(instruction => instruction.LoadsField(EarliestRaidDaysField)) != 1)
            {
                Log.Error("[CinderStory] 未匹配派系袭击天数判定 保留原版行为");
                return result;
            }
            foreach (CodeInstruction instruction in result)
            {
                if (!instruction.LoadsField(EarliestRaidDaysField)) continue;
                instruction.opcode = OpCodes.Call;
                instruction.operand = ResolveRaidDaysMethod;
            }
            return result;
        }

        public static float EarliestRaidDays(FactionDef faction)
        {
            if (EmberCycleGame.Selected && faction.defName == CycleTuning.CradleFactionName)
                return NoRaidDelay;
            return faction.earliestRaidDays;
        }
    }
}
