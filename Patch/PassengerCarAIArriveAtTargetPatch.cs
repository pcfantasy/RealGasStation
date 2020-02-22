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
using UnityEngine;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class PassengerCarAIArriveAtTargetPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(PassengerCarAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null);
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref PassengerCarAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            //RealGasStation Mod related
            if (data.m_transferType == 112 || data.m_transferType == 113)
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    PassengerCarAIArriveAtTargetForRealGasStationPre(ref __instance, vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = false;
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

        public static void PassengerCarAIArriveAtTargetForRealGasStationPre(ref PassengerCarAI __instance, ushort vehicleID, ref Vehicle data)
        {
            var distance = Vector3.Distance(data.GetLastFramePosition(), Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.TargetGasBuilding[vehicleID]].m_position);

            if (distance < 80f)
            {
                if (MainDataStore.petrolBuffer[data.m_targetBuilding] > 400)
                {
                    MainDataStore.petrolBuffer[data.m_targetBuilding] -= 400;
                }
            }
            __instance.SetTarget(vehicleID, ref data, 0);
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (distance < 80f)
            {
                if (Loader.isRealCityRunning)
                {
                    var additionPrice = (RealGasStationThreading.reduceVehicle) ? 600 * RealGasStationThreading.reduceCargoDiv : 600;
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + additionPrice), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level1);
                }
            }
        }
    }
}
