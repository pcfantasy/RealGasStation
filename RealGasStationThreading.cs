using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using RealGasStation.CustomAI;
using RealGasStation.CustomManager;
using RealGasStation.UI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation
{
    public class RealGasStationThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex + 1u;
                int num7 = (int)(currentFrameIndex & 15u);
                int num8 = num7 * 1024;
                int num9 = (num7 + 1) * 1024 - 1;
                VehicleManager instance1 = Singleton<VehicleManager>.instance;
                if (RealGasStation.IsEnabled)
                {
                    for (int i = num8; i <= num9; i = i + 1)
                    {
                        VehicleStatus(i, currentFrameIndex, ref instance1.m_vehicles.m_buffer[i]);
                    }

                    CheckDetour();
                    CheckLanguage();

                }
            }
        }



        public void DetourAfterLoad()
        {
            //This is for Detour RealCity method
            DebugLog.LogToFileOnly("Init DetourAfterLoad");
            bool detourFailed = false;

            if (Loader.realCityRunning)
            {
                Assembly as1 = Assembly.Load("RealCity");
                //1
                DebugLog.LogToFileOnly("Detour RealCityCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPre calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealCity.CustomAI.RealCityCargoTruckAI").GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour RealCityCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPre");
                    detourFailed = true;
                }

                //2
                DebugLog.LogToFileOnly("Detour RealCityCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPost calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealCity.CustomAI.RealCityCargoTruckAI").GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPost", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPost", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour RealCityCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPost");
                    detourFailed = true;
                }

                //3
                DebugLog.LogToFileOnly("Detour RealCityCargoTruckAI::CargoTruckAIArriveAtSourceForRealGasStationPre calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealCity.CustomAI.RealCityCargoTruckAI").GetMethod("CargoTruckAIArriveAtSourceForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("CargoTruckAIArriveAtSourceForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour RealCityCargoTruckAI::CargoTruckAIArriveAtSourceForRealGasStationPre");
                    detourFailed = true;
                }

                //4
                DebugLog.LogToFileOnly("Detour RealCityPassengerCarAI::PassengerCarAIArriveAtTargetForRealGasStationPre calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealCity.CustomAI.RealCityPassengerCarAI").GetMethod("PassengerCarAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomPassengerCarAI).GetMethod("PassengerCarAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour RealCityPassengerCarAI::PassengerCarAIArriveAtTargetForRealGasStationPre");
                    detourFailed = true;
                }
            }
            else if (Loader.realConstructionRunning)
            {
                Assembly as1 = Assembly.Load("RealConstruction");
                //1
                DebugLog.LogToFileOnly("Detour CustomCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPre calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealConstruction.CustomAI.CustomCargoTruckAI").GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPre", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CustomCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPre");
                    detourFailed = true;
                }

                //2
                DebugLog.LogToFileOnly("Detour CustomCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPost calls");
                try
                {
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("RealConstruction.CustomAI.CustomCargoTruckAI").GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPost", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("CargoTruckAIArriveAtTargetForRealGasStationPost", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CustomCargoTruckAI::CargoTruckAIArriveAtTargetForRealGasStationPost");
                    detourFailed = true;
                }
            }
            else
            {
                //detourFailed = false;
            }

            if (detourFailed)
            {
                DebugLog.LogToFileOnly("DetourAfterLoad failed");
            }
            else
            {
                DebugLog.LogToFileOnly("DetourAfterLoad successful");
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited)
            {
                isFirstTime = false;
                DetourAfterLoad();
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");
                    List<string> list = new List<string>();
                    foreach (Loader.Detour current in Loader.Detours)
                    {
                        if (!RedirectionHelper.IsRedirected(current.OriginalMethod, current.CustomMethod))
                        {
                            list.Add(string.Format("{0}.{1} with {2} parameters ({3})", new object[]
                            {
                    current.OriginalMethod.DeclaringType.Name,
                    current.OriginalMethod.Name,
                    current.OriginalMethod.GetParameters().Length,
                    current.OriginalMethod.DeclaringType.AssemblyQualifiedName
                            }));
                        }
                    }
                    DebugLog.LogToFileOnly(string.Format("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Detours checked. Result: {0} missing detours", list.Count));
                    if (list.Count > 0)
                    {
                        string error = "RealGasStation detected an incompatibility with another mod! You can continue playing but it's NOT recommended. RealGasStation will not work as expected. See RealGasStation.log for technical details.";
                        DebugLog.LogToFileOnly(error);
                        string text = "The following methods were overriden by another mod:";
                        foreach (string current2 in list)
                        {
                            text += string.Format("\n\t{0}", current2);
                        }
                        DebugLog.LogToFileOnly(text);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", text, true);
                    }
                }
            }
        }

        public void CheckLanguage()
        {
            if (SingletonLite<LocaleManager>.instance.language.Contains("zh") && (MainDataStore.lastLanguage == 1))
            {
            }
            else if (!SingletonLite<LocaleManager>.instance.language.Contains("zh") && (MainDataStore.lastLanguage != 1))
            {
            }
            else
            {
                MainDataStore.lastLanguage = (byte)(SingletonLite<LocaleManager>.instance.language.Contains("zh") ? 1 : 0);
                Language.LanguageSwitch(MainDataStore.lastLanguage);
                PlayerBuildingUI.refeshOnce = true;
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

            //Fuel
            incomingTransferReason = (TransferManager.TransferReason)112;
            num34 = MainDataStore.petrolBuffer[buildingID] - MainDataStore.finalVehicleForFuelCount[buildingID] * 400;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (num34 >= 0)
                {
                    System.Random rand = new System.Random();
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = rand.Next(8);
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
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)num].m_transferType == material)
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


        public void GetForFuelCount(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_transferType == 112)
            {
                MainDataStore.tempVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]++;
            }
        }


        public void VehicleStatus(int i, uint currentFrameIndex, ref Vehicle vehicle)
        {
            int num4 = (int)(currentFrameIndex & 255u);

            if (vehicle.m_transferType == 112)
            {
                if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.Created) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Deleted))
                {
                    if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.WaitingPath))
                    {
                        if (vehicle.Info.m_vehicleAI is CargoTruckAI && (vehicle.m_targetBuilding != 0))
                        {
                            PathManager instance1 = Singleton<PathManager>.instance;
                            byte pathFindFlags = instance1.m_pathUnits.m_buffer[(int)((UIntPtr)vehicle.m_path)].m_pathFindFlags;
                            if ((pathFindFlags & 8) != 0)
                            {
                                vehicle.m_transferType = MainDataStore.preTranferReason[i];
                                PathManager instance = Singleton<PathManager>.instance;
#if DEBUG
                                DebugLog.LogToFileOnly("PathFind not success " + i.ToString() + "vehicle.m_path = " + vehicle.m_path.ToString() + vehicle.m_flags.ToString());
#endif
                                if (vehicle.m_path != 0u)
                                {
                                    instance.ReleasePath(vehicle.m_path);
                                    vehicle.m_path = 0;
                                }
                                CargoTruckAI AI = vehicle.Info.m_vehicleAI as CargoTruckAI;
#if DEBUG
                                DebugLog.LogToFileOnly("PathFind not success " + i.ToString() + "transferType = " + vehicle.m_transferType.ToString() + "And MainDataStore.TargetGasBuilding[vehicleID] = " + MainDataStore.TargetGasBuilding[i].ToString() + "data.m_targetBuilding = " + vehicle.m_targetBuilding.ToString());
#endif
                                AI.SetTarget((ushort)i, ref vehicle, vehicle.m_targetBuilding);
#if DEBUG
                                DebugLog.LogToFileOnly("Reroute to target " + i.ToString() + "vehicle.m_path = " + vehicle.m_path.ToString() + vehicle.m_flags.ToString());
#endif
                                MainDataStore.TargetGasBuilding[i] = 0;
                            }
                        }
                        else if (vehicle.Info.m_vehicleAI is PassengerCarAI && vehicle.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                        {
                            PathManager instance1 = Singleton<PathManager>.instance;
                            byte pathFindFlags = instance1.m_pathUnits.m_buffer[(int)((UIntPtr)vehicle.m_path)].m_pathFindFlags;
                            if ((pathFindFlags & 8) != 0)
                            {
                                PassengerCarAI AI = vehicle.Info.m_vehicleAI as PassengerCarAI;
                                vehicle.m_transferType = MainDataStore.preTranferReason[i];
                                AI.SetTarget((ushort)i, ref vehicle, 0);
                                MainDataStore.TargetGasBuilding[i] = 0;
                            }
                        }
                    }
                }
                else
                {
                    vehicle.m_transferType = 0;
                }
            }


            if (((num4 >> 4) & 15u) == (i & 15u))
            {
                GetForFuelCount((ushort)i, ref vehicle);
                VehicleManager instance = Singleton<VehicleManager>.instance;
                if (vehicle.m_flags.IsFlagSet(Vehicle.Flags.Created) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && (vehicle.m_cargoParent == 0) && vehicle.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !vehicle.m_flags.IsFlagSet(Vehicle.Flags.GoingBack))
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
                            if (IsGasBuilding(vehicle.m_targetBuilding))
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
                else
                {
                    //MainDataStore.alreadyAskForFuel[i] = false;
                }
            }
        }

        public override void OnAfterSimulationFrame()
        {
            base.OnAfterSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (RealGasStation.IsEnabled)
                {
                    uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    int num4 = (int)(currentFrameIndex & 255u);
                    int num5 = num4 * 192;
                    int num6 = (num4 + 1) * 192 - 1;
                    ////DebugLog.LogToFileOnly("currentFrameIndex num2 = " + currentFrameIndex.ToString());
                    if (num4 == 255)
                    {
                        PlayerBuildingUI.refeshOnce = true;
                    }
                    for (int i = num5; i <= num6; i = i + 1)
                    {
                        if (instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Created) && (!instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Deleted)))
                        {
                            MainDataStore.isBuildingReleased[i] = false;
                            MainDataStore.finalVehicleForFuelCount[i] = MainDataStore.tempVehicleForFuelCount[i];
                            MainDataStore.tempVehicleForFuelCount[i] = 0;
                            if (!instance.m_buildings.m_buffer[i].m_flags.IsFlagSet(Building.Flags.Untouchable))
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
                        else
                        {
                            if (!MainDataStore.isBuildingReleased[i])
                            {
                                MainDataStore.isBuildingReleased[i] = true;
                                CustomCommonBuildingAI.CustomReleaseBuilding((ushort)i);
                            }
                        }
                    }

                    int num7 = (int)(currentFrameIndex & 15u);
                    int num8 = num7 * 1024;
                    int num9 = (num7 + 1) * 1024 - 1;
                    for (int i = num8; i <= num9; i = i + 1)
                    {
                        if ((MainDataStore.TargetGasBuilding[i] != 0) || MainDataStore.alreadyAskForFuel[i])
                        {
                            if (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[i].m_flags.IsFlagSet(Vehicle.Flags.Created) && !Singleton<VehicleManager>.instance.m_vehicles.m_buffer[i].m_flags.IsFlagSet(Vehicle.Flags.Deleted))
                            {

                            }
                            else
                            {
                                MainDataStore.TargetGasBuilding[i] = 0;
                                MainDataStore.alreadyAskForFuel[i] = false;
                            }
                        }
                    }
                    CustomTransferManager.CustomSimulationStepImpl();
                }
            }
        }
    }
}
