using ColossalFramework;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.CustomAI
{
    public class CustomCarAI
    {
        public static byte[] watingPathTime = new byte[65536];
        public static ushort[] stuckTime = new ushort[65536];
        public static void CarAIPathfindFailurePostFix(ushort vehicleID, ref Vehicle data)
        {
            ushort i = vehicleID;
            if ((data.m_transferType == 112) || (data.m_transferType == 113))
            {
                if (data.Info.m_vehicleAI is CargoTruckAI && (data.m_targetBuilding != 0))
                {
                    CargoTruckAI AI = (CargoTruckAI)data.Info.m_vehicleAI;
#if DEBUG
                    DebugLog.LogToFileOnly("PathFind not success " + i.ToString() + "transferType = " + vehicle.m_transferType.ToString() + "And MainDataStore.TargetGasBuilding[vehicleID] = " + MainDataStore.TargetGasBuilding[i].ToString() + "data.m_targetBuilding = " + vehicle.m_targetBuilding.ToString());
#endif
                    AI.SetTarget((ushort)i, ref data, data.m_targetBuilding);
#if DEBUG
                                DebugLog.LogToFileOnly("Reroute to target " + i.ToString() + "vehicle.m_path = " + vehicle.m_path.ToString() + vehicle.m_flags.ToString());
#endif
                    data.m_transferType = MainDataStore.preTranferReason[i];
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI && data.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                {
                    PassengerCarAI AI = (PassengerCarAI)data.Info.m_vehicleAI;
                    AI.SetTarget((ushort)i, ref data, 0);
                    data.m_transferType = MainDataStore.preTranferReason[i];
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
            }
        }

        public static void CarAISimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            VehicleStatus(vehicleID, ref vehicleData);
            if (!Loader.isRealCityRunning)
            {
                DetectStuckVehicle(vehicleID, ref vehicleData);
            }
        }

        //For detour AdvancedJuctionRule
        public static void CarAICustomSimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleStatus(vehicleID, ref vehicleData);
            if (!Loader.isRealCityRunning)
            {
                DetectStuckVehicle(vehicleID, ref vehicleData);
            }
        }

        public static void DetectStuckVehicle(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingPath))
            {
                stuckTime[vehicleID] = 0;
                if (vehicleData.m_path != 0)
                {
                    watingPathTime[vehicleID]++;
                }
                if (watingPathTime[vehicleID] > 192)
                {
                    ushort building = 0;
                    if (!vehicleData.m_flags.IsFlagSet(Vehicle.Flags.GoingBack))
                    {
                        building = vehicleData.m_targetBuilding;
                    }
                    else
                    {
                        building = vehicleData.m_sourceBuilding;
                    }

                    var buildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
                    DebugLog.LogToFileOnly("DebugInfo: Stuck at waitingpath vehicle target building m_class is " + buildingData.Info.m_class.ToString());
                    DebugLog.LogToFileOnly("DebugInfo: Stuck at waitingpath vehicle target name is " + buildingData.Info.name.ToString());
                    DebugLog.LogToFileOnly("DebugInfo: Stuck at waitingpath vehicle transfer type is " + vehicleData.m_transferType.ToString());
                    DebugLog.LogToFileOnly("DebugInfo: Stuck at waitingpath vehicle flag is " + vehicleData.m_flags.ToString());
                    DebugLog.LogToFileOnly("DebugInfo: Stuck at waitingpath vehicle ai is " + vehicleData.Info.m_vehicleAI.ToString());
                    watingPathTime[vehicleID] = 0;
                    Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                    vehicleData.m_path = 0u;
                    vehicleData.m_flags = (vehicleData.m_flags & ~Vehicle.Flags.WaitingPath);
                }
            }
            else
            {
                watingPathTime[vehicleID] = 0;
                if (!vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingCargo) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingSpace) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingLoading) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingTarget) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.WaitingSpace) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.Stopped) && !vehicleData.m_flags.IsFlagSet(Vehicle.Flags.Congestion))
                {
                    float realSpeed = (float)Math.Sqrt(vehicleData.GetLastFrameVelocity().x * vehicleData.GetLastFrameVelocity().x + vehicleData.GetLastFrameVelocity().y * vehicleData.GetLastFrameVelocity().y + vehicleData.GetLastFrameVelocity().z * vehicleData.GetLastFrameVelocity().z);
                    if (realSpeed < 0.1f)
                    {
                        stuckTime[vehicleID]++;
                    }
                    else
                    {
                        stuckTime[vehicleID] = 0;
                    }

                    if (stuckTime[vehicleID] > 1600)
                    {
                        DebugLog.LogToFileOnly("DebugInfo: Stuck vehicle transfer type is " + vehicleData.m_transferType.ToString());
                        DebugLog.LogToFileOnly("DebugInfo: Stuck vehicle flag is " + vehicleData.m_flags.ToString());
                        DebugLog.LogToFileOnly("DebugInfo: Stuck vehicle ai is " + vehicleData.Info.m_vehicleAI.ToString());
                        stuckTime[vehicleID] = 0;
                        Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                        vehicleData.m_path = 0u;
                        Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleID);
                    }
                }
            }
        }

        public static ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            return instance2;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }

        public static void GetForFuelCount(ushort vehicleID, ref Vehicle data)
        {
            if ((data.m_transferType == 112) || (data.m_transferType == 113))
            {
                MainDataStore.tempVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]++;
            }
        }

        public static void VehicleStatus(int i, ref Vehicle vehicle)
        {
            if (i < Singleton<VehicleManager>.instance.m_vehicles.m_size)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num4 = (int)(currentFrameIndex & 4095u);
                if (((num4 >> 8) & 255u) == (i & 255u))
                {
                    GetForFuelCount((ushort)i, ref vehicle);
                    VehicleManager instance = Singleton<VehicleManager>.instance;
                    if (!vehicle.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && (vehicle.m_cargoParent == 0) && vehicle.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.GoingBack) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Parking))
                    {
                        if (vehicle.Info.m_vehicleAI is CargoTruckAI && (vehicle.m_targetBuilding != 0))
                        {
                            if (!MainDataStore.alreadyAskForFuel[i])
                            {
                                if (GasStationAI.IsGasBuilding(vehicle.m_targetBuilding))
                                {
                                    MainDataStore.alreadyAskForFuel[i] = true;
                                }
                                else
                                {
                                    System.Random rand = new System.Random();
                                    if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.DummyTraffic))
                                    {
                                        if (rand.Next(62) < 2)
                                        {
                                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                            offer.Priority = rand.Next(8);
                                            offer.Vehicle = (ushort)i;
                                            offer.Position = vehicle.GetLastFramePosition();
                                            offer.Amount = 1;
                                            offer.Active = true;
                                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)113, offer);
                                            MainDataStore.alreadyAskForFuel[i] = true;
                                        }
                                    }
                                    else
                                    {
                                        if (rand.Next(93) < 2)
                                        {
                                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                            offer.Priority = rand.Next(8);
                                            offer.Vehicle = (ushort)i;
                                            offer.Position = vehicle.GetLastFramePosition();
                                            offer.Amount = 1;
                                            offer.Active = true;
                                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)113, offer);
                                            MainDataStore.alreadyAskForFuel[i] = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (vehicle.Info.m_vehicleAI is PassengerCarAI && vehicle.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                        {
                            if (!MainDataStore.alreadyAskForFuel[i])
                            {
                                if (GasStationAI.IsGasBuilding(vehicle.m_targetBuilding))
                                {
                                    MainDataStore.alreadyAskForFuel[i] = true;
                                }
                                else
                                {
                                    System.Random rand = new System.Random();
                                    ushort citizen = GetDriverInstance((ushort)i, ref vehicle);
                                    if (Singleton<CitizenManager>.instance.m_citizens.m_buffer[Singleton<CitizenManager>.instance.m_instances.m_buffer[citizen].m_citizen].m_flags.IsFlagSet(Citizen.Flags.DummyTraffic))
                                    {
                                        if (rand.Next(62) < 2)
                                        {
                                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                            offer.Priority = rand.Next(8);
                                            offer.Vehicle = (ushort)i;
                                            offer.Position = vehicle.GetLastFramePosition();
                                            offer.Amount = 1;
                                            offer.Active = true;
                                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)112, offer);
                                            MainDataStore.alreadyAskForFuel[i] = true;
                                        }
                                    }
                                    else
                                    {
                                        if (rand.Next(93) < 2)
                                        {
                                            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                            offer.Priority = rand.Next(8);
                                            offer.Vehicle = (ushort)i;
                                            offer.Position = vehicle.GetLastFramePosition();
                                            offer.Amount = 1;
                                            offer.Active = true;
                                            Singleton<TransferManager>.instance.AddOutgoingOffer((TransferManager.TransferReason)112, offer);
                                            MainDataStore.alreadyAskForFuel[i] = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Error: invalid vehicleID = " + i.ToString());
            }
        }
    }
}
