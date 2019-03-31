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
        public static void CarAIPathfindFailurePostFix(ushort vehicleID, ref Vehicle data)
        {
            ushort i = vehicleID;
            if (data.m_transferType == 112)
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
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI && data.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                {
                    PassengerCarAI AI = (PassengerCarAI)data.Info.m_vehicleAI;
                    data.m_transferType = MainDataStore.preTranferReason[i];
                    AI.SetTarget((ushort)i, ref data, 0);
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
            }
        }

        public static void CarAICustomSimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics)
        {
            VehicleStatus(vehicleID, ref vehicleData);
        }

        //For detour AdvancedJuctionRule
        public static void CarAICustomSimulationStepPreFix(ushort vehicleID, ref Vehicle vehicleData)
        {
            VehicleStatus(vehicleID, ref vehicleData);
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
            if (data.m_transferType == 112)
            {
                MainDataStore.tempVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]++;
            }
        }

        public static void VehicleStatus(int i, ref Vehicle vehicle)
        {
            if (i < 16384)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num4 = (int)(currentFrameIndex & 255u);
                if (((num4 >> 4) & 15u) == (i & 15u))
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
                                        if (rand.Next(1000) < 2)
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
                                        if (rand.Next(1500) < 2)
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
                                        if (rand.Next(1000) < 2)
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
                                        if (rand.Next(1500) < 2)
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
