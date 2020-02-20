using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Harmony;
using ICities;
using RealGasStation.CustomAI;
using RealGasStation.CustomManager;
using RealGasStation.NewAI;
using RealGasStation.UI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealGasStation
{
    public class RealGasStationThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public static FieldInfo _reduceVehicle = null;
        public static Assembly RealCity = null;
        public static Type RealCityClass = null;
        public static object RealCityInstance = null;
        public static bool reduceVehicle = false;
        public static Type MainDataStoreClass = null;
        public static object MainDataStoreInstance = null;
        public static FieldInfo _reduceCargoDiv = null;
        public static int reduceCargoDiv = 1;
        public static bool dummyCargoNeedFuel = false;
        public static bool dummyCarNeedFuel = false;
        public static bool cargoNeedFuel = false;
        public static bool carNeedFuel = false;
        public static ushort dummyCargoCount = 0;
        public static ushort dummyCarCount = 0;
        public static ushort cargoCount = 0;
        public static ushort carCount = 0;
        public const int HarmonyPatchNum = 12;


        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                VehicleManager instance1 = Singleton<VehicleManager>.instance;
                if (RealGasStation.IsEnabled)
                {
                    CheckDetour();
                }
            }
        }

        public void DetourAfterLoad()
        {
            //This is for Detour Other Mod method
            DebugLog.LogToFileOnly("Init DetourAfterLoad");
            bool detourFailed = false;

            if (Loader.isRealCityRunning)
            {
                RealCity = Assembly.Load("RealCity");
                RealCityClass = RealCity.GetType("RealCity.RealCity");
                RealCityInstance = Activator.CreateInstance(RealCityClass);
                _reduceVehicle = RealCityClass.GetField("reduceVehicle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                MainDataStoreClass = RealCity.GetType("RealCity.Util.MainDataStore");
                MainDataStoreInstance = Activator.CreateInstance(MainDataStoreClass);
                _reduceCargoDiv = MainDataStoreClass.GetField("reduceCargoDiv", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
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
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                DetourAfterLoad();
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");

                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "RealGasStation HarmonyDetourInit is failed, Send RealGasStation.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                    }
                    else
                    {
                        var harmony = HarmonyInstance.Create(HarmonyDetours.ID);
                        var methods = harmony.GetPatchedMethods();
                        int i = 0;
                        foreach (var method in methods)
                        {
                            var info = harmony.GetPatchInfo(method);
                            if (info.Owners?.Contains(harmony.Id) == true)
                            {
                                DebugLog.LogToFileOnly("Harmony patch method = " + method.Name.ToString());
                                if (info.Prefixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PreFix");
                                }
                                if (info.Postfixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PostFix");
                                }
                                i++;
                            }
                        }

                        if (i != HarmonyPatchNum)
                        {
                            string error = $"RealGasStation HarmonyDetour Patch Num is {i}, Right Num is {HarmonyPatchNum} Send RealGasStation.txt to Author.";
                            DebugLog.LogToFileOnly(error);
                            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                        }
                    }
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
                    RefreshFuel();
                    if (num4 == 255)
                    {
                        PlayerBuildingUI.refeshOnce = true;
                        if (!isFirstTime)
                        {
                            if (Loader.isRealCityRunning)
                            {
                                reduceVehicle = (bool)_reduceVehicle.GetValue(RealCityInstance);
                                if (reduceVehicle)
                                {
                                    reduceCargoDiv = (int)_reduceCargoDiv.GetValue(MainDataStoreInstance);
                                }
                                else
                                {
                                    reduceCargoDiv = 1;
                                }
                            }
                            else
                            {
                                reduceVehicle = false;
                                reduceCargoDiv = 1;
                            }
                        }
                    }

                    CustomTransferManager.CustomSimulationStepImpl();
                }
            }
        }

        public void RefreshFuel()
        {
            dummyCargoNeedFuel = dummyCargoNeedFuel ? false : dummyCargoNeedFuel;
            dummyCarNeedFuel = dummyCarNeedFuel ? false : dummyCarNeedFuel;
            cargoNeedFuel = cargoNeedFuel ? false : cargoNeedFuel;
            carNeedFuel = carNeedFuel ? false : cargoNeedFuel;

            dummyCargoNeedFuel = (dummyCargoCount > 1000) ? true : false;
            dummyCarNeedFuel = (dummyCarCount > 1000) ? true : false;
            cargoNeedFuel = (cargoCount > 1500) ? true : false;
            carNeedFuel = (carCount > 1500) ? true : false;

            dummyCargoCount = (dummyCargoCount > 1000) ? (ushort)0 : dummyCargoCount;
            dummyCarCount = (dummyCarCount > 1000) ? (ushort)0 : dummyCarCount;
            cargoCount = (cargoCount > 1500) ? (ushort)0 : cargoCount;
            carCount = (carCount > 1500) ? (ushort)0 : carCount;
        }
    }
}
