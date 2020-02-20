using ColossalFramework;
using Harmony;
using RealGasStation.CustomAI;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class CargoTruckAIArriveAtSourcePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("ArriveAtSource", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null);
        }
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if ((data.m_transferType == 113) || (data.m_transferType == 112))
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    CargoTruckAIArriveAtSourceForRealGasStationPre(ref __instance, vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = true;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    data.Unspawn(vehicleID);
                }
                __result = true;
                return false;
            }
            return true;
        }

        public static void CargoTruckAIArriveAtSourceForRealGasStationPre(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data)
        {
            //DebugLog.LogToFileOnly("Error: CargoTruckAIArriveAtSourceForRealGasStationPre will not happen");
            if (MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] > 400)
            {
                MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] -= 400;
            }
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            PathManager instance = Singleton<PathManager>.instance;
            if (data.m_path != 0u)
            {
                instance.ReleasePath(data.m_path);
                data.m_path = 0;
            }
            __instance.SetTarget(vehicleID, ref data, data.m_targetBuilding);
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + 3000f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
            }
        }
    }
}
