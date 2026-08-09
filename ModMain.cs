using System.Reflection;

using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(
    typeof(FixedRecipeCost.ModMain),
    "Fixed Recipe Cost",
    "1.0.0",
    "Neokeld")]

[assembly: MelonGame(null, "DAVE THE DIVER")]

namespace FixedRecipeCost
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg(
                "Fixed Recipe Cost loaded");
        }
    }

	[HarmonyPatch]
	internal static class RecipeCostPatch
	{
		static MethodBase TargetMethod()
		{
			var type = AccessTools.TypeByName("GameFormulaManager");

			return AccessTools.Method(
				type,
				"RequiredIngredientsCount");
		}

		static bool Prefix(
			ref int __result,
			int level,
			int originCount)
		{
			__result = originCount;
			return false;
		}
	}
}
