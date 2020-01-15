using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using RealGasStation.CustomAI;
using RealGasStation.CustomManager;

namespace RealGasStation.Util
{
    public static class HarmonyDetours
    {
        private static HarmonyInstance harmony = null;
        private static void ConditionalPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (harmony.GetPatchInfo(method)?.Owners?.Contains(harmony.Id) == true)
            {
                DebugLog.LogToFileOnly("Harmony patches already present for {0}" + fullMethodName.ToString());
            }
            else
            {
                DebugLog.LogToFileOnly("Patching {0}..." + fullMethodName.ToString());
                harmony.Patch(method, prefix, postfix);
            }
        }

        private static void ConditionalUnPatch(this HarmonyInstance harmony, MethodBase method, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            var fullMethodName = string.Format("{0}.{1}", method.ReflectedType?.Name ?? "(null)", method.Name);
            if (prefix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Prefix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Prefix);
            }
            if (postfix != null)
            {
                DebugLog.LogToFileOnly("UnPatching Postfix{0}..." + fullMethodName.ToString());
                harmony.Unpatch(method, HarmonyPatchType.Postfix);
            }
        }

        public static void Apply()
        {
            harmony = HarmonyInstance.Create("RealGasStation");
            //1
            var commonBuildingAIReleaseBuilding = typeof(CommonBuildingAI).GetMethod("ReleaseBuilding");
            var commonBuildingAIReleaseBuildingPostFix = typeof(CustomCommonBuildingAI).GetMethod("CommonBuildingAIReleaseBuildingPostfix");
            harmony.ConditionalPatch(commonBuildingAIReleaseBuilding,
                null,
                new HarmonyMethod(commonBuildingAIReleaseBuildingPostFix));
            //2
            var carAIPathfindFailure = typeof(CarAI).GetMethod("PathfindFailure", BindingFlags.NonPublic | BindingFlags.Instance);
            var carAIPathfindFailurePostFix = typeof(CustomCarAI).GetMethod("CarAIPathfindFailurePostFix");
            harmony.ConditionalPatch(carAIPathfindFailure,
                null,
                new HarmonyMethod(carAIPathfindFailurePostFix));
            //3
            var playerBuildingAISimulationStep = typeof(PlayerBuildingAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
            var playerBuildingAISimulationStepPostFix = typeof(CustomPlayerBuildingAI).GetMethod("PlayerBuildingAISimulationStepPostFix");
            harmony.ConditionalPatch(playerBuildingAISimulationStep,
                null,
                new HarmonyMethod(playerBuildingAISimulationStepPostFix));
            //4
            if (!Loader.isAdvancedJunctionRuleRunning)
            {
                var carAISimulationStep = typeof(CarAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(Vehicle.Frame).MakeByRefType(),
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(int)}, null);
                var carAISimulationStepPreFix = typeof(CustomCarAI).GetMethod("CarAISimulationStepPreFix");
                harmony.ConditionalPatch(carAISimulationStep,
                    new HarmonyMethod(carAISimulationStepPreFix),
                    null);
            }
            //5
            var vehicleManagerReleaseVehicleImplementation = typeof(VehicleManager).GetMethod("ReleaseVehicleImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
            var vehicleManagerReleaseVehicleImplementationPreFix = typeof(CustomVehicleManager).GetMethod("VehicleManagerReleaseVehicleImplementationPreFix");
            harmony.ConditionalPatch(vehicleManagerReleaseVehicleImplementation,
                new HarmonyMethod(vehicleManagerReleaseVehicleImplementationPreFix),
                null);

            //6
            var vehicleAICalculateTargetSpeed = typeof(VehicleAI).GetMethod("CalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null);
            var vehicleAICalculateTargetSpeedPreFix = typeof(CustomVehicleAI).GetMethod("VehicleAICalculateTargetSpeedPreFix");
            harmony.ConditionalPatch(vehicleAICalculateTargetSpeed,
                new HarmonyMethod(vehicleAICalculateTargetSpeedPreFix),
                null);
            Loader.HarmonyDetourFailed = false;
            DebugLog.LogToFileOnly("Harmony patches applied");
        }

        public static void DeApply()
        {
            //1
            var commonBuildingAIReleaseBuilding = typeof(CommonBuildingAI).GetMethod("ReleaseBuilding");
            var commonBuildingAIReleaseBuildingPostFix = typeof(CustomCommonBuildingAI).GetMethod("CommonBuildingAIReleaseBuildingPostfix");
            harmony.ConditionalUnPatch(commonBuildingAIReleaseBuilding,
                null,
                new HarmonyMethod(commonBuildingAIReleaseBuildingPostFix));
            //2
            var carAIPathfindFailure = typeof(CarAI).GetMethod("PathfindFailure", BindingFlags.NonPublic | BindingFlags.Instance);
            var carAIPathfindFailurePostFix = typeof(CustomCarAI).GetMethod("CarAIPathfindFailurePostFix");
            harmony.ConditionalUnPatch(carAIPathfindFailure,
                null,
                new HarmonyMethod(carAIPathfindFailurePostFix));
            //3
            var playerBuildingAISimulationStep = typeof(PlayerBuildingAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Building.Frame).MakeByRefType() }, null);
            var playerBuildingAISimulationStepPostFix = typeof(CustomPlayerBuildingAI).GetMethod("PlayerBuildingAISimulationStepPostFix");
            harmony.ConditionalUnPatch(playerBuildingAISimulationStep,
                null,
                new HarmonyMethod(playerBuildingAISimulationStepPostFix));
            //4
            if (!Loader.isAdvancedJunctionRuleRunning)
            {
                var carAISimulationStep = typeof(CarAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(Vehicle.Frame).MakeByRefType(),
                typeof(ushort),
                typeof(Vehicle).MakeByRefType(),
                typeof(int)}, null);
                var carAISimulationStepPreFix = typeof(CustomCarAI).GetMethod("CarAISimulationStepPreFix");
                harmony.ConditionalUnPatch(carAISimulationStep,
                    new HarmonyMethod(carAISimulationStepPreFix),
                    null);
            }
            //5
            var vehicleManagerReleaseVehicleImplementation = typeof(VehicleManager).GetMethod("ReleaseVehicleImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
            var vehicleManagerReleaseVehicleImplementationPreFix = typeof(CustomVehicleManager).GetMethod("VehicleManagerReleaseVehicleImplementationPreFix");
            harmony.ConditionalUnPatch(vehicleManagerReleaseVehicleImplementation,
                new HarmonyMethod(vehicleManagerReleaseVehicleImplementationPreFix),
                null);

            //6
            var vehicleAICalculateTargetSpeed = typeof(VehicleAI).GetMethod("CalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null);
            var vehicleAICalculateTargetSpeedPreFix = typeof(CustomVehicleAI).GetMethod("VehicleAICalculateTargetSpeedPreFix");
            harmony.ConditionalUnPatch(vehicleAICalculateTargetSpeed,
                new HarmonyMethod(vehicleAICalculateTargetSpeedPreFix),
                null);
            DebugLog.LogToFileOnly("Harmony patches DeApplied");
        }
    }
}
