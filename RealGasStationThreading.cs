using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using RealGasStation.UI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation
{
    public class RealGasStationThreading : ThreadingExtensionBase
    {
        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex + 1u;
                int num7 = (int)(currentFrameIndex & 15u);
                int num8 = num7 * 1024;
                int num9 = (num7 + 1) * 1024 - 1;
                //DebugLog.LogToFileOnly("currentFrameIndex num2 = " + currentFrameIndex.ToString());
                VehicleManager instance1 = Singleton<VehicleManager>.instance;
                if (RealGasStation.IsEnabled)
                {
                    for (int i = num8; i <= num9; i = i + 1)
                    {
                        VehicleStatus(i, currentFrameIndex, ref instance1.m_vehicles.m_buffer[i]);
                    }

                    if (SingletonLite<LocaleManager>.instance.language.Contains("zh") && (MainDataStore.lastLanguage == 1))
                    {
                        //MainDataStore.lastLanguage = (byte)(SingletonLite<LocaleManager>.instance.language.Contains("zh") ? 1 : 0);
                    }
                    else if (!SingletonLite<LocaleManager>.instance.language.Contains("zh") && (MainDataStore.lastLanguage != 1))
                    {
                        //MainDataStore.lastLanguage = (byte)(SingletonLite<LocaleManager>.instance.language.Contains("zh") ? 1 : 0);
                    }
                    else
                    {
                        MainDataStore.lastLanguage = (byte)(SingletonLite<LocaleManager>.instance.language.Contains("zh") ? 1 : 0);
                        Language.LanguageSwitch(MainDataStore.lastLanguage);
                        PlayerBuildingUI.refeshOnce = true;
                    }


                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    int num4 = (int)(currentFrameIndex & 255u);
                    int num5 = num4 * 192;
                    int num6 = (num4 + 1) * 192 - 1;
                    //DebugLog.LogToFileOnly("currentFrameIndex num2 = " + currentFrameIndex.ToString());
                    if (num4 == 255)
                    {
                        PlayerBuildingUI.refeshOnce = true;
                    }
                    for (int i = num5; i <= num6; i = i + 1)
                    {
                        if (instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Created) && (!instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Deleted)) && (!instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Untouchable)))
                        {
                            if (!(instance.m_buildings.m_buffer[i].Info.m_buildingAI is OutsideConnectionAI) && !((instance.m_buildings.m_buffer[i].Info.m_buildingAI is DecorationBuildingAI)) && !(instance.m_buildings.m_buffer[i].Info.m_buildingAI is WildlifeSpawnPointAI))
                            {
                                if (IsGasBuilding((ushort)i))
                                {
                                    if (instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Completed))
                                    {
                                        ProcessGasBuildingIncoming((ushort)i, ref instance.m_buildings.m_buffer[i]);
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }




        void ProcessGasBuildingIncoming(ushort buildingID, ref Building buildingData)
        {
            int num27 = 0;
            int num28 = 0;
            int num29 = 0;
            int value = 0;
            int num34 = 0;
            TransferManager.TransferReason incomingTransferReason = default(TransferManager.TransferReason);

            //Petrol
            incomingTransferReason = TransferManager.TransferReason.Petrol;
            num27 = 0;
            num28 = 0;
            num29 = 0;
            value = 0;
            num34 = 0;
            if (incomingTransferReason != TransferManager.TransferReason.None && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num27, ref num28, ref num29, ref value);
                buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
            }

            num34 = 50000 - MainDataStore.petrolBuffer[buildingID] - num29;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (num34 >= 0)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 7;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = (int)(num34 / 8000);
                    offer.Active = false;

                    if (offer.Amount > 0)
                    {
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                    }
                }
            }

            //fuel
            incomingTransferReason = (TransferManager.TransferReason)112;
            num27 = 0;
            num28 = 0;
            num29 = 0;
            value = 0;
            num34 = 0;
            if (incomingTransferReason != TransferManager.TransferReason.None)
            {
                CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num27, ref num28, ref num29, ref value);
                buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
            }

            num34 = MainDataStore.petrolBuffer[buildingID] - num29;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (num34 >= 0)
                {
                    System.Random rand = new System.Random();
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = rand.Next(7) + 1 ;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = (int)((num34 - 0) / 400);
                    offer.Active = false;

                    if ((int)(num34 / 400) > 0)
                    {
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                    }
                }
            }
        }

        protected void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if (material == (TransferManager.TransferReason)112)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
                    cargo += 400;
                    capacity += 400;
                    count++;
                    if ((instance.m_vehicles.m_buffer[(int)num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != (Vehicle.Flags)0)
                    {
                        outside++;
                    }
                }
                else if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)num].m_transferType == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
                    int a;
                    int num3;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[(int)num], out a, out num3);
                    cargo += Mathf.Min(a, num3);
                    capacity += num3;
                    count++;
                    if ((instance.m_vehicles.m_buffer[(int)num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != (Vehicle.Flags)0)
                    {
                        outside++;
                    }
                }
                num = instance.m_vehicles.m_buffer[(int)num].m_nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }


        public static bool IsGasBuilding(ushort id)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            int num = instance.m_buildings.m_buffer[id].Info.m_buildingAI.GetConstructionCost();
            if (num == 508600)
            {
                return true;
            }

            return false;
        }



        // PassengerCarAI
        private ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
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

        public void VehicleStatus(int i, uint currentFrameIndex, ref Vehicle vehicle)
        {
            int num4 = (int)(currentFrameIndex & 255u);


            if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.Created) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && vehicle.m_flags.IsFlagSet(Vehicle.Flags.WaitingPath))
            {
                if (vehicle.Info.m_vehicleAI is CargoTruckAI && (vehicle.m_targetBuilding != 0))
                {
                    PathManager instance1 = Singleton<PathManager>.instance;
                    byte pathFindFlags = instance1.m_pathUnits.m_buffer[(int)((UIntPtr)vehicle.m_path)].m_pathFindFlags;
                    if ((pathFindFlags & 8) != 0)
                    {
                        if (vehicle.m_transferType == 112)
                        {
                            if (vehicle.m_targetBuilding != 0)
                            {
                                Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicle.m_targetBuilding].RemoveGuestVehicle((ushort)i, ref vehicle);
                                vehicle.m_targetBuilding = 0;
                            }
                            vehicle.m_transferType = MainDataStore.preTranferReason[i];
                            CargoTruckAI AI = vehicle.Info.m_vehicleAI as CargoTruckAI;
                            AI.SetTarget((ushort)i, ref vehicle, MainDataStore.preTargetBuilding[i]);
                            //DebugLog.LogToFileOnly("CargoTruckAI, can not find path to gas station, set target to original again" + i.ToString());
                        }
                    }
                    else if ((pathFindFlags & 4) != 0)
                    {
                        if (vehicle.m_transferType == 112)
                        {
                            //DebugLog.LogToFileOnly("CargoTruckAI, find path to gas station" + i.ToString());
                        }
                    }
                }
                else if (vehicle.Info.m_vehicleAI is PassengerCarAI && vehicle.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                {
                    PathManager instance1 = Singleton<PathManager>.instance;
                    byte pathFindFlags = instance1.m_pathUnits.m_buffer[(int)((UIntPtr)vehicle.m_path)].m_pathFindFlags;
                    if ((pathFindFlags & 8) != 0)
                    {
                        if (vehicle.m_transferType == 112)
                        {
                            if (vehicle.m_targetBuilding != 0)
                            {
                                Singleton<BuildingManager>.instance.m_buildings.m_buffer[vehicle.m_targetBuilding].RemoveGuestVehicle((ushort)i, ref vehicle);
                                vehicle.m_targetBuilding = 0;
                            }
                            PassengerCarAI AI = vehicle.Info.m_vehicleAI as PassengerCarAI;
                            vehicle.m_transferType = MainDataStore.preTranferReason[i];
                            AI.SetTarget((ushort)i, ref vehicle, MainDataStore.preTargetBuilding[i]);
                            //DebugLog.LogToFileOnly("PassengerCarAI, can not find path to gas station, set target to original again" + i.ToString());
                        }
                        else if ((pathFindFlags & 4) != 0)
                        {
                            if (vehicle.m_transferType == 112)
                            {
                               // DebugLog.LogToFileOnly("PassengerCarAI, find path to gas station" + i.ToString());
                            }
                        }
                    }
                }
            }


            if (((num4 >> 4) & 15u) == (i & 15u))
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;




                if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.Created) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && (vehicle.m_cargoParent == 0) && vehicle.m_flags.IsFlagSet(Vehicle.Flags.Spawned))
                {
                    if (vehicle.Info.m_vehicleAI is CargoTruckAI && (vehicle.m_targetBuilding != 0))
                    {
                        if (!MainDataStore.alreadyAskForFuel[i])
                        {
                            if (IsGasBuilding(vehicle.m_targetBuilding))
                            {
                                MainDataStore.alreadyAskForFuel[i] = true;
                            }
                            else
                            {
                                System.Random rand = new System.Random();
                                if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.DummyTraffic))
                                {
                                    if (rand.Next(500) < 2)
                                    {
                                        //DebugLog.LogToFileOnly("try to send fuel demand");
                                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                        offer.Priority = rand.Next(7) + 1;
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
                                    if (rand.Next(2000) < 2)
                                    {
                                        //DebugLog.LogToFileOnly("try to send fuel demand");
                                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                        offer.Priority = rand.Next(7) + 1;
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
                            if (IsGasBuilding(vehicle.m_targetBuilding))
                            {
                                MainDataStore.alreadyAskForFuel[i] = true;
                            }
                            else
                            {
                                ushort citizen = GetDriverInstance((ushort)i, ref vehicle);
                                System.Random rand = new System.Random();
                                if (Singleton<CitizenManager>.instance.m_citizens.m_buffer[Singleton<CitizenManager>.instance.m_instances.m_buffer[citizen].m_citizen].m_flags.IsFlagSet(Citizen.Flags.DummyTraffic))
                                {
                                    if (rand.Next(500) < 2)
                                    {
                                        //DebugLog.LogToFileOnly("try to send fuel demand PassengerCarAI");
                                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                        offer.Priority = rand.Next(7) + 1;
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
                                    if (rand.Next(2000) < 2)
                                    {
                                        //DebugLog.LogToFileOnly("try to send fuel demand PassengerCarAI");
                                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                                        offer.Priority = rand.Next(7) + 1;
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
                else
                {
                    MainDataStore.alreadyAskForFuel[i] = false;
                }
            }
        }
    }
}
