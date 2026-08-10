using System.Reflection;

using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(
    typeof(LessGrindMoreDiveMelonLoader.ModMain),
    "Fixed Recipe Cost",
    "1.1.0",
    "Neokeld")]

[assembly: MelonGame(null, "DAVE THE DIVER")]

namespace LessGrindMoreDiveMelonLoader
{
    public class ModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg(
                "Less Grind More Dive Melon Loader loaded");
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

    [HarmonyPatch]
    internal static class PlayerCharacterInitPatch
    {
        private const int ExtraLiftDrones = 5;
        private const int ExtraCrabTraps = 5;

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("PlayerCharacter"),
                "Init");
        }

        private static void Postfix(object __instance)
        {
            if (__instance == null)
                return;

            var playerType = __instance.GetType();

            var droneProp = AccessTools.Property(playerType, "AvailableLiftDroneCount");
            var trapProp = AccessTools.Property(playerType, "AvailableCrabTrapCount");

            if (droneProp != null)
            {
                int current = (int)droneProp.GetValue(__instance);
                droneProp.SetValue(__instance, current + ExtraLiftDrones);
            }

            if (trapProp != null)
            {
                int current = (int)trapProp.GetValue(__instance);
                trapProp.SetValue(__instance, current + ExtraCrabTraps);
            }

            MelonLogger.Msg(
                $"Added +{ExtraLiftDrones} LiftDrones and +{ExtraCrabTraps} CrabTraps");
        }
    }
}
