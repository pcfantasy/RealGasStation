using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomCargoTruckAI : CargoTruckAI
    {
        public static void ProcessResourceArriveAtTargetForRealCity(ushort vehicleID, ref Vehicle data, ref int num)
        {
            DebugLog.LogToFileOnly("Error: Should be detour by RealCity @ ProcessResourceArriveAtTarget");
        }

        public static void CargoTruckAIArriveAtTargetForRealConstruction(ushort vehicleID, ref Vehicle data)
        {
            DebugLog.LogToFileOnly("Error: Should be detour by RealConstruction @ CargoTruckAIArriveAtTargetForRealConstruction");
        }

        public void CargoTruckAIArriveAtTargetForRealGasStationPre(ushort vehicleID, ref Vehicle data)
        {
            if (MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] > 400)
            {
                MainDataStore.petrolBuffer[MainDataStore.TargetGasBuilding[vehicleID]] -= 400;
            }
            PathManager instance = Singleton<PathManager>.instance;
            if (data.m_path != 0u)
            {
                instance.ReleasePath(data.m_path);
                data.m_path = 0;
            }
            SetTarget(vehicleID, ref data, data.m_targetBuilding);
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;

            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * GetResourcePrice(TransferManager.TransferReason.Petrol) + 1800f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
            }
        }

        public void CargoTruckAIArriveAtSourceForRealGasStationPre(ushort vehicleID, ref Vehicle data)
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
            SetTarget(vehicleID, ref data, data.m_targetBuilding);
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * GetResourcePrice(TransferManager.TransferReason.Petrol) + 3000f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
            }
        }

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            if ((data.m_transferType == 113) || (data.m_transferType == 112))
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    CargoTruckAIArriveAtSourceForRealGasStationPre(vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = true;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    data.Unspawn(vehicleID);
                }
                return true;
            }

            if (data.m_sourceBuilding == 0)
            {
                Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                return true;
            }
            int amountDelta = 0;
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                amountDelta = data.m_transferSize;
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                info.m_buildingAI.ModifyMaterialBuffer(data.m_sourceBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta, 0, data.m_transferSize);
            }
            RemoveSource(vehicleID, ref data);
            Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
            return true;
        }


        private void RemoveSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                data.m_sourceBuilding = 0;
            }
        }

        private bool ArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            if ((data.m_transferType == 113) || (data.m_transferType == 112))
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    CargoTruckAIArriveAtTargetForRealGasStationPre(vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = true;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    data.Unspawn(vehicleID);
                }
                return true;
            }
            /// NON-STOCK CODE END ///
            if (data.m_targetBuilding == 0)
            {
                return true;
            }
            int amountDelta = 0;
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                // NON-STOCK CODE START
                CargoTruckAIArriveAtTargetForRealGasStationPost(vehicleID, ref data);
                // RealConstruction and RealGasStation mod
                if (Loader.isRealConstructionRunning)
                {
                    CargoTruckAIArriveAtTargetForRealConstruction(vehicleID, ref data);
                }
                /// NON-STOCK CODE END ///
                amountDelta = data.m_transferSize;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                amountDelta = Mathf.Min(0, data.m_transferSize - m_cargoCapacity);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;

            // NON-STOCK CODE START
            if (Loader.isRealCityRunning)
            {
                ProcessResourceArriveAtTargetForRealCity(vehicleID, ref data, ref amountDelta);
            }
            else
            {
                info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
            }
            /// NON-STOCK CODE END ///
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                data.m_transferSize = (ushort)Mathf.Clamp(data.m_transferSize - amountDelta, 0, data.m_transferSize);
                if (data.m_sourceBuilding != 0)
                {
                    IndustryBuildingAI.ExchangeResource((TransferManager.TransferReason)data.m_transferType, amountDelta, data.m_sourceBuilding, data.m_targetBuilding);
                }
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                data.m_transferSize += (ushort)Mathf.Max(0, -amountDelta);
            }
            if (data.m_sourceBuilding != 0 && (instance.m_buildings.m_buffer[data.m_sourceBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Outgoing)
            {
                BuildingInfo info2 = instance.m_buildings.m_buffer[data.m_sourceBuilding].Info;
                ushort num = instance.FindBuilding(instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position, 200f, info2.m_class.m_service, ItemClass.SubService.None, Building.Flags.Incoming, Building.Flags.Outgoing);
                if (num != 0)
                {
                    instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveOwnVehicle(vehicleID, ref data);
                    data.m_sourceBuilding = num;
                    instance.m_buildings.m_buffer[data.m_sourceBuilding].AddOwnVehicle(vehicleID, ref data);
                }
            }
            if ((instance.m_buildings.m_buffer[data.m_targetBuilding].m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.Incoming)
            {
                ushort num2 = instance.FindBuilding(instance.m_buildings.m_buffer[data.m_targetBuilding].m_position, 200f, info.m_class.m_service, ItemClass.SubService.None, Building.Flags.Outgoing, Building.Flags.Incoming);
                if (num2 != 0)
                {
                    data.Unspawn(vehicleID);
                    BuildingInfo info3 = instance.m_buildings.m_buffer[num2].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    Vector3 position;
                    Vector3 target;
                    info3.m_buildingAI.CalculateSpawnPosition(num2, ref instance.m_buildings.m_buffer[num2], ref randomizer, m_info, out position, out target);
                    Quaternion rotation = Quaternion.identity;
                    Vector3 forward = target - position;
                    if (forward.sqrMagnitude > 0.01f)
                    {
                        rotation = Quaternion.LookRotation(forward);
                    }
                    data.m_frame0 = new Vehicle.Frame(position, rotation);
                    data.m_frame1 = data.m_frame0;
                    data.m_frame2 = data.m_frame0;
                    data.m_frame3 = data.m_frame0;
                    data.m_targetPos0 = position;
                    data.m_targetPos0.w = 2f;
                    data.m_targetPos1 = target;
                    data.m_targetPos1.w = 2f;
                    data.m_targetPos2 = data.m_targetPos1;
                    data.m_targetPos3 = data.m_targetPos1;
                    FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
                    SetTarget(vehicleID, ref data, 0);
                    return true;
                }
            }
            SetTarget(vehicleID, ref data, 0);
            return false;
        }

        public static float GetResourcePrice(TransferManager.TransferReason material)
        {
            //Need to sync with RealCity mod
            float num = 0f;
            if (RealGasStationThreading.reduceVehicle)
            {
                switch (material)
                {
                    case TransferManager.TransferReason.Petrol:
                        num = 3f; break;
                    case TransferManager.TransferReason.Food:
                        num = 1.5f; break;
                    case TransferManager.TransferReason.Lumber:
                        num = 2f; break;
                    case TransferManager.TransferReason.Coal:
                        num = 2.5f; break;
                    default: DebugLog.LogToFileOnly("Error: Unknow material in RealGasStation = " + material.ToString()); num = 0f; break;
                }
            }
            else
            {
                switch (material)
                {
                    case TransferManager.TransferReason.Petrol:
                        num = 3f * RealGasStationThreading.reduceCargoDiv; break;
                    case TransferManager.TransferReason.Food:
                        num = 1.5f * RealGasStationThreading.reduceCargoDiv; break;
                    case TransferManager.TransferReason.Lumber:
                        num = 2f * RealGasStationThreading.reduceCargoDiv; break;
                    case TransferManager.TransferReason.Coal:
                        num = 2.5f * RealGasStationThreading.reduceCargoDiv; break;
                    default: DebugLog.LogToFileOnly("Error: Unknow material in RealGasStation = " + material.ToString()); num = 0f; break;
                }
            }
            return (float)(UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Science, 100) / 100f) * num;
        }

        public static void CargoTruckAIArriveAtTargetForRealGasStationPost(ushort vehicleID, ref Vehicle vehicleData)
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

                                vehicleData.m_transferSize = 0;
                                if (MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] <= 57000)
                                {
                                    MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] += 8000;
                                }
                                if (Loader.isRealCityRunning)
                                {
                                    float productionValue = 8000 * GetResourcePrice((TransferManager.TransferReason)vehicleData.m_transferType);
                                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.ResourcePrice, (int)productionValue, ItemClass.Service.PlayerIndustry, ItemClass.SubService.PlayerIndustryOil, ItemClass.Level.Level1);
                                }
                                break;
                            default:
                                DebugLog.LogToFileOnly("Error: Find a import trade m_transferType = " + vehicleData.m_transferType.ToString()); break;
                        }
                    }
                }
            }
        }

        public override void UpdateBuildingTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
        {
            if ((leaderData.m_flags & Vehicle.Flags.WaitingTarget) != (Vehicle.Flags)0)
            {
                return;
            }

            if ((leaderData.m_flags & Vehicle.Flags.GoingBack) != (Vehicle.Flags)0)
            {
                if (leaderData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[(int)leaderData.m_sourceBuilding].Info;
                    Randomizer randomizer = new Randomizer((int)vehicleID);
                    Vector3 targetPos;
                    Vector3 vector;
                    info.m_buildingAI.CalculateUnspawnPosition(leaderData.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)leaderData.m_sourceBuilding], ref randomizer, this.m_info, out targetPos, out vector);
                    vehicleData.SetTargetPos(index++, base.CalculateTargetPoint(refPos, targetPos, minSqrDistance, 4f));
                    return;
                }
            }
            else if (leaderData.m_targetBuilding != 0)
            {
                // NON-STOCK CODE START
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]].Info;
                float distance = Vector3.Distance(instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]].m_position, leaderData.GetLastFramePosition());
                if (((leaderData.m_transferType == 113) || (leaderData.m_transferType == 112)) && (distance < 50))
                {
                    if (leaderID == vehicleID)
                    {
                        if (MainDataStore.TargetGasBuilding[leaderID] != 0)
                        {
                            Randomizer randomizer2 = new Randomizer((int)vehicleID);
                            Vector3 targetPos2;
                            Vector3 vector2;
                            info2.m_buildingAI.CalculateUnspawnPosition(MainDataStore.TargetGasBuilding[leaderID], ref instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]], ref randomizer2, this.m_info, out targetPos2, out vector2);
                            vehicleData.SetTargetPos(index++, base.CalculateTargetPoint(refPos, targetPos2, minSqrDistance, 4f));
                            return;
                        }
                    }
                    else
                    {
                        DebugLog.LogToFileOnly("Error: No such case for RealGasStation");
                    }
                }
                else
                {
                    /// NON-STOCK CODE END
                    Randomizer randomizer2 = new Randomizer((int)vehicleID);
                    Vector3 targetPos2;
                    Vector3 vector2;
                    info2.m_buildingAI.CalculateUnspawnPosition(leaderData.m_targetBuilding, ref instance2.m_buildings.m_buffer[(int)leaderData.m_targetBuilding], ref randomizer2, this.m_info, out targetPos2, out vector2);
                    vehicleData.SetTargetPos(index++, base.CalculateTargetPoint(refPos, targetPos2, minSqrDistance, 4f));
                    return;
                }
            }
        }

        public static bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            var m_info = vehicleData.Info;
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return true;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    Vector3 a, target;
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out a, out target);


                    var inst = Singleton<CargoTruckAI>.instance;

                    InitDelegate();
                    return CargoTruckAIStartPathFindDG(inst, vehicleID, ref vehicleData, vehicleData.m_targetPos3, target, true, true, false);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
                Vector3 b, target2;
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding], ref randomizer2, m_info, out b, out target2);

                var inst = Singleton<CargoTruckAI>.instance;

                InitDelegate();
                return CargoTruckAIStartPathFindDG(inst, vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2, true, true, false);
            }
            return false;
        }

        public static void InitDelegate()
        {
            if (CargoTruckAIStartPathFindDG != null)
            {
                return;
            }

            CargoTruckAIStartPathFindDG = FastDelegateFactory.Create<CargoTruckAIStartPathFind>(typeof(CargoTruckAI), "StartPathFind", instanceMethod: true);
        }

        public delegate bool CargoTruckAIStartPathFind(CargoTruckAI CargoTruckAI, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget);
        public static CargoTruckAIStartPathFind CargoTruckAIStartPathFindDG;
    }
}
