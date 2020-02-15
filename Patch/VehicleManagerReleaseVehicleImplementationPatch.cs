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
            MainDataStore.TargetGasBuilding[vehicle] = 0;
            MainDataStore.alreadyAskForFuel[vehicle] = false;
            if (data.m_transferType == 112)
            {
                if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    //do not need to remove targetBuilding
                    data.m_targetBuilding = 0;
                }
            }
        }
    }
}
