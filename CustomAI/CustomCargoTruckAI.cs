using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomCargoTruckAI : CargoTruckAI
    {
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
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * GetResourcePrice(TransferManager.TransferReason.Petrol) + 1800f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level2);
            }
        }

        private bool ArriveAtSource(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            if (data.m_transferType == 112)
            {
                CargoTruckAIArriveAtSourceForRealGasStationPre(vehicleID, ref data);
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
            if (data.m_transferType == 112)
            {
                CargoTruckAIArriveAtTargetForRealGasStationPre(vehicleID, ref data);
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
                this.CargoTruckAIArriveAtTargetForRealGasStationPost(vehicleID, ref data);
                /// NON-STOCK CODE END ///
                amountDelta = data.m_transferSize;
            }
            if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
            {
                amountDelta = Mathf.Min(0, data.m_transferSize - m_cargoCapacity);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_targetBuilding].Info;
            info.m_buildingAI.ModifyMaterialBuffer(data.m_targetBuilding, ref instance.m_buildings.m_buffer[data.m_targetBuilding], (TransferManager.TransferReason)data.m_transferType, ref amountDelta);
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
            if (RealGasStationThreading.reduceVehicle)
            {
                switch (material)
                {
                    case TransferManager.TransferReason.Petrol:
                        return 3f;
                    case TransferManager.TransferReason.Food:
                        return 1.5f;
                    case TransferManager.TransferReason.Lumber:
                        return 2f;
                    case TransferManager.TransferReason.Coal:
                        return 2.5f;
                    default: DebugLog.LogToFileOnly("Error: Unknow material in RealGasStation = " + material.ToString()); return 0f;
                }
            }
            else
            {
                switch (material)
                {
                    case TransferManager.TransferReason.Petrol:
                        return 3f * RealGasStationThreading.reduceCargoDiv;
                    case TransferManager.TransferReason.Food:
                        return 1.5f * RealGasStationThreading.reduceCargoDiv;
                    case TransferManager.TransferReason.Lumber:
                        return 2f * RealGasStationThreading.reduceCargoDiv;
                    case TransferManager.TransferReason.Coal:
                        return 2.5f * RealGasStationThreading.reduceCargoDiv;
                    default: DebugLog.LogToFileOnly("Error: Unknow material in RealGasStation = " + material.ToString()); return 0f;
                }
            }
        }

        public void CargoTruckAIArriveAtTargetForRealGasStationPost(ushort vehicleID, ref Vehicle vehicleData)
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

        public override string GetLocalizedStatus(ushort vehicleID, ref Vehicle data, out InstanceID target)
        {
            if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
            {
                ushort targetBuilding = data.m_targetBuilding;
                if ((data.m_flags & Vehicle.Flags.GoingBack) != 0)
                {
                    target = InstanceID.Empty;
                    TransferManager.TransferReason transferType = (TransferManager.TransferReason)data.m_transferType;
                    if (transferType == (TransferManager.TransferReason)112)
                    {
                        return Localization.Get("FOR_FUEL");
                    }
                    return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_RETURN");
                }
                if ((data.m_flags & Vehicle.Flags.WaitingTarget) != 0)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_UNLOAD");
                }
                if (targetBuilding != 0)
                {
                    Building.Flags flags = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags;
                    TransferManager.TransferReason transferType = (TransferManager.TransferReason)data.m_transferType;
                    if ((data.m_flags & Vehicle.Flags.Exporting) != 0 || (flags & Building.Flags.IncomingOutgoing) != 0)
                    {
                        target = InstanceID.Empty;
                        if (transferType == (TransferManager.TransferReason)112)
                        {
                            return Localization.Get("FOR_FUEL");
                        }
                        return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_EXPORT", transferType.ToString());
                    }
                    if ((data.m_flags & Vehicle.Flags.Importing) != 0)
                    {
                        target = InstanceID.Empty;
                        target.Building = targetBuilding;
                        if (transferType == (TransferManager.TransferReason)112)
                        {
                            return Localization.Get("FOR_FUEL");
                        }
                        return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_IMPORT", transferType.ToString());
                    }
                    target = InstanceID.Empty;
                    target.Building = targetBuilding;
                    if (transferType == (TransferManager.TransferReason)110)
                    {
                        return Localization.Get("TRANSFER_CONSTRUCTION");
                    }
                    else if (transferType == (TransferManager.TransferReason)111)
                    {
                        return Localization.Get("TRANSFER_OPERATION");
                    }
                    else if (transferType == (TransferManager.TransferReason)112)
                    {
                        return Localization.Get("FOR_FUEL");
                    }
                    else
                    {
                        return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CARGOTRUCK_DELIVER", transferType.ToString());
                    }
                }
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
        }

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if (targetBuilding == data.m_targetBuilding)
            {
                if (data.m_path == 0)
                {
                    if (!StartPathFind(vehicleID, ref data))
                    {
                        data.Unspawn(vehicleID);
                    }
                }
                else
                {
                    TrySpawn(vehicleID, ref data);
                }
            }
            else
            {
                if (data.m_transferType != 112)
                {
                    RemoveTarget(vehicleID, ref data);
                    data.m_targetBuilding = targetBuilding;
                }
                data.m_flags &= ~Vehicle.Flags.WaitingTarget;
                data.m_waitCounter = 0;
                if (targetBuilding != 0)
                {
                    if (data.m_transferType != 112)
                    {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
                    }
                    if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags & Building.Flags.IncomingOutgoing) != 0)
                    {
                        if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                        {
                            data.m_flags |= Vehicle.Flags.Exporting;
                        }
                        else if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                        {
                            data.m_flags |= Vehicle.Flags.Importing;
                        }
                    }
                }
                else
                {
                    if ((data.m_flags & Vehicle.Flags.TransferToTarget) != 0)
                    {
                        if (data.m_transferSize > 0)
                        {
                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                            offer.Priority = 7;
                            offer.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer.Position = data.GetLastFramePosition();
                            }
                            offer.Amount = 1;
                            offer.Active = true;
                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)data.m_transferType, offer);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                    if ((data.m_flags & Vehicle.Flags.TransferToSource) != 0)
                    {
                        if (data.m_transferSize < m_cargoCapacity)
                        {
                            TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
                            offer2.Priority = 7;
                            offer2.Vehicle = vehicleID;
                            if (data.m_sourceBuilding != 0)
                            {
                                offer2.Position = (data.GetLastFramePosition() + Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position) * 0.5f;
                            }
                            else
                            {
                                offer2.Position = data.GetLastFramePosition();
                            }
                            offer2.Amount = 1;
                            offer2.Active = true;
                            Singleton<TransferManager>.instance.AddIncomingOffer((TransferManager.TransferReason)data.m_transferType, offer2);
                            data.m_flags |= Vehicle.Flags.WaitingTarget;
                        }
                        else
                        {
                            data.m_flags |= Vehicle.Flags.GoingBack;
                        }
                    }
                }
                if (data.m_cargoParent != 0)
                {
                    if (data.m_path != 0)
                    {
                        if (data.m_path != 0)
                        {
                            Singleton<PathManager>.instance.ReleasePath(data.m_path);
                        }
                        data.m_path = 0u;
                    }
                }
                else
                {
                    bool success = false;
                    if (data.m_transferType == 112)
                    {
                        ushort tempTargetBuilding = data.m_targetBuilding;
                        data.m_targetBuilding = MainDataStore.TargetGasBuilding[vehicleID];
                        success = StartPathFind(vehicleID, ref data);
                        data.m_targetBuilding = tempTargetBuilding;
                    }
                    else
                    {
                        success = StartPathFind(vehicleID, ref data);
                    }
                    if (!success)
                    {
                        if (data.m_transferType == 112)
                        {
                            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                            PathManager instance = Singleton<PathManager>.instance;
                            if (data.m_path != 0u)
                            {
                                instance.ReleasePath(data.m_path);
                                data.m_path = 0;
                            }
                            SetTarget(vehicleID, ref data, data.m_targetBuilding);
                            MainDataStore.TargetGasBuilding[vehicleID] = 0;
                        }
                        else
                        {
                            data.Unspawn(vehicleID);
                        }
                    }
                }
            }
        }

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
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
                if (leaderData.m_transferType == 112 && (distance < 50))
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
    }
}
