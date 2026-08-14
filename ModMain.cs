using System.Reflection;

using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(
    typeof(LessGrindMoreDiveMelonLoader.ModMain),
    "Less Grind More Dive Melon Loader",
    "1.2.0",
    "Neokeld")]

[assembly: MelonGame(null, "DAVE THE DIVER")]

namespace LessGrindMoreDiveMelonLoader
{
    public class ModMain : MelonMod
    {
        public static MelonPreferences_Category ConfigCategory;

        public static MelonPreferences_Entry<int> ExtraLiftDrones;
        public static MelonPreferences_Entry<int> ExtraCrabTraps;
        public static MelonPreferences_Entry<bool> FixedRecipeCost;
        public static MelonPreferences_Entry<float> MermanVillageSpeedBoost;

        public override void OnInitializeMelon()
        {
            ConfigCategory = MelonPreferences.CreateCategory("LessGrindMoreDive");

            ExtraLiftDrones = ConfigCategory.CreateEntry(
                "ExtraLiftDrones",
                5,
                "Additional Lift Drones");

            ExtraCrabTraps = ConfigCategory.CreateEntry(
                "ExtraCrabTraps",
                5,
                "Additional Crab Traps");

            FixedRecipeCost = ConfigCategory.CreateEntry(
                "FixedRecipeCost",
                true,
                "Use original recipe ingredient count");

            MermanVillageSpeedBoost = ConfigCategory.CreateEntry(
                "MermanVillageSpeed",
                5f,
                "Speed boost in the Merman Village");

            MelonPreferences.Save();

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
            if (!ModMain.FixedRecipeCost.Value)
                return true;
            
            __result = originCount;
            return false;
        }
    }

    public static class SpeedManager
    {
        private static bool _bonusEnabled = false;

        public static bool isBonusEnabled()
        {
            return _bonusEnabled;
        }

        public static void EnterMermanVillage()
        {
            if(ModMain.MermanVillageSpeedBoost.Value > 0f) {
                _bonusEnabled = true;
                
                MelonLogger.Msg($"Enter Merman Village, speed boost {ModMain.MermanVillageSpeedBoost.Value}");
            }
        }

        public static void LeaveMermanVillage()
        {
            if(ModMain.MermanVillageSpeedBoost.Value > 0f) {
                _bonusEnabled = false;
                
                MelonLogger.Msg("Leave Merman Village");
            }
        }
    }

    [HarmonyPatch]
    internal static class MermanVillageEnterPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("MermanVillageSceneManager"),
                "Start");
        }

        static void Prefix()
        {
            SpeedManager.EnterMermanVillage();
        }
    }

    [HarmonyPatch]
    internal static class MermanVillageExitPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("MermanVillageSceneManager"),
                "OnDestroy");
        }

        static void Prefix()
        {
            SpeedManager.LeaveMermanVillage();
        }
    }

    [HarmonyPatch]
    internal static class PlayerCharacterInitPatch
    {
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
                droneProp.SetValue(__instance, current + ModMain.ExtraLiftDrones.Value);
            }

            if (trapProp != null)
            {
                int current = (int)trapProp.GetValue(__instance);
                trapProp.SetValue(__instance, current + ModMain.ExtraCrabTraps.Value);
            }

            MelonLogger.Msg(
                $"Added +{ModMain.ExtraLiftDrones.Value} LiftDrones and +{ModMain.ExtraCrabTraps.Value} CrabTraps");
        }
    }
    
    [HarmonyPatch]
    internal static class PlayerCharacterDetermineMoveSpeedPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("PlayerCharacter"),
                "DetermineMoveSpeed");
        }

        static void Postfix(
            ref float __result)
        {
            if (SpeedManager.isBonusEnabled())
                __result = __result * ModMain.MermanVillageSpeedBoost.Value;
        }
    }
}
