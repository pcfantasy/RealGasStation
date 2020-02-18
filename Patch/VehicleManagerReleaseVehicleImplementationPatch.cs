using Harmony;
using RealGasStation.Util;
using System;
using System.Reflection;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class VehicleManagerReleaseVehicleImplementationPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(VehicleManager).GetMethod("ReleaseVehicleImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        }
        public static void Prefix(ushort vehicle, ref Vehicle data)
        {
            //DebugLog.LogToFileOnly($"remove vehicle {vehicle} MainDataStore.TargetGasBuilding[vehicle] {MainDataStore.TargetGasBuilding[vehicle]} data.m_targetBuilding {data.m_targetBuilding}");
            if (data.m_transferType == 112 || data.m_transferType == 113)
            {
                if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicle]] > 0)
                    MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicle]]--;
                if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    //do not need to remove targetBuilding
                    data.m_targetBuilding = 0;
                }
                MainDataStore.TargetGasBuilding[vehicle] = 0;
                MainDataStore.alreadyAskForFuel[vehicle] = false;
            }
        }
    }
}
