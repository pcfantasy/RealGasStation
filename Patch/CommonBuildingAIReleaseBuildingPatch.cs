using ColossalFramework;
using HarmonyLib;
using RealGasStation.Util;
using System.Reflection;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class CommonBuildingAIReleaseBuildingPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CommonBuildingAI).GetMethod("ReleaseBuilding");
        }
        public static void Postfix(ushort buildingID)
        {
            MainDataStore.petrolBuffer[buildingID] = 0;
            MainDataStore.resourceCategory[buildingID] = 0;
            MainDataStore.finalVehicleForFuelCount[buildingID] = 0;
            TransferManager.TransferOffer offer = default;
            offer.Building = buildingID;
            Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)126, offer);
            Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)127, offer);
        }
    }
}
