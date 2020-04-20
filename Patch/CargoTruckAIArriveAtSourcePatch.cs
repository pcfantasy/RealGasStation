using ColossalFramework;
using HarmonyLib;
using RealGasStation.CustomAI;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class CargoTruckAIArriveAtSourcePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("ArriveAtSource", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null);
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if ((data.m_transferType == 127) || (data.m_transferType == 126))
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    CargoTruckAIArriveAtSourceForRealGasStationPre(ref __instance, vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = true;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                        MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    MainDataStore.TargetGasBuilding[vehicleID] = 0;
                    data.Unspawn(vehicleID);
                }
                __result = true;
                return false;
            }
            return true;
        }

        public static void CargoTruckAIArriveAtSourceForRealGasStationPre(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data)
        {
            var distance = Vector3.Distance(data.GetLastFramePosition(), Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.TargetGasBuilding[vehicleID]].m_position);

            if (distance < 80f)
            {
                if (MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] > 400)
                {
                    MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] -= 400;
                }
            }
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            PathManager instance = Singleton<PathManager>.instance;
            if (data.m_path != 0u)
            {
                instance.ReleasePath(data.m_path);
                data.m_path = 0;
            }
            __instance.SetTarget(vehicleID, ref data, data.m_targetBuilding);
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;

            if (distance < 80f)
            {
                if (Loader.isRealCityRunning)
                {
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + 1000), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
                }
            }
        }
    }
}
