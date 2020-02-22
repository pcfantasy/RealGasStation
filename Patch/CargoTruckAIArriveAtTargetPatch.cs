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
    public static class CargoTruckAIArriveAtTargetPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null);
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ref bool __result)
        {
            if ((data.m_transferType == 113) || (data.m_transferType == 112))
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    CargoTruckAIArriveAtTargetForRealGasStationPre(ref __instance, vehicleID, ref data);
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
            else
            {
                if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                {
                    // NON-STOCK CODE START
                    CargoTruckAIArriveAtTargetForRealGasStationPost(ref data);
                }
            }
            return true;
        }

        public static void CargoTruckAIArriveAtTargetForRealGasStationPost(ref Vehicle vehicleData)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (vehicleData.m_targetBuilding != 0)
            {
                Building buildingData = instance.m_buildings.m_buffer[vehicleData.m_targetBuilding];
                if (!(buildingData.Info.m_buildingAI is OutsideConnectionAI))
                {
                    if (GasStationAI.IsGasBuilding(vehicleData.m_targetBuilding) == true)
                    {
                        switch ((TransferManager.TransferReason)vehicleData.m_transferType)
                        {
                            case TransferManager.TransferReason.Petrol:

                                if (MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] <= 57000)
                                {
                                    MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] += vehicleData.m_transferSize;
                                }

                                if (Loader.isRealCityRunning)
                                {
                                    float productionValue = vehicleData.m_transferSize * CustomCargoTruckAI.GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryOil, ItemClass.Level.Level1);
                                }
                                vehicleData.m_transferSize = 0;
                                break;
                            default:
                                DebugLog.LogToFileOnly("Error: Find a import trade m_transferType = " + vehicleData.m_transferType.ToString()); break;
                        }
                    }
                }
            }
        }

        public static void CargoTruckAIArriveAtTargetForRealGasStationPre(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data)
        {
            var distance = Vector3.Distance(data.GetLastFramePosition(), Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.TargetGasBuilding[vehicleID]].m_position);

            if (distance < 80f)
            {
                if (MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] > 400)
                {
                    MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] -= 400;
                }
            }
            PathManager instance = Singleton<PathManager>.instance;
            if (data.m_path != 0u)
            {
                instance.ReleasePath(data.m_path);
                data.m_path = 0;
            }
            __instance.SetTarget(vehicleID, ref data, data.m_targetBuilding);
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;

            if (distance < 80f)
            {
                if (Loader.isRealCityRunning)
                {
                    var additionPrice = (RealGasStationThreading.reduceVehicle) ? 600 * RealGasStationThreading.reduceCargoDiv : 600;
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + additionPrice), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
                    //DebugLog.LogToFileOnly($"GasStation income = {(int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + additionPrice)}");
                }
            }
        }
    }
}
