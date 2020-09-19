using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using RealGasStation.CustomManager;
using RealGasStation.UI;
using RealGasStation.Util;
using System;
using System.Reflection;

namespace RealGasStation
{
    public class RealGasStationThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public static bool dummyCargoNeedFuel = false;
        public static bool dummyCarNeedFuel = false;
        public static bool cargoNeedFuel = false;
        public static bool carNeedFuel = false;
        public static ushort dummyCargoCount = 0;
        public static ushort dummyCarCount = 0;
        public static ushort cargoCount = 0;
        public static ushort carCount = 0;
        public const int HarmonyPatchNum = 13;


        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (RealGasStation.IsEnabled)
                {
                    CheckDetour();
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
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
                        var harmony = new Harmony(HarmonyDetours.ID);
                        var methods = harmony.GetPatchedMethods();
                        int i = 0;
                        foreach (var method in methods)
                        {
                            var info = Harmony.GetPatchInfo(method);
                            if (info.Owners?.Contains(harmony.Id) == true)
                            {
                                DebugLog.LogToFileOnly($"Harmony patch method = {method.FullDescription()}");
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
                    int num4 = (int)(currentFrameIndex & 255u);
                    if (num4 == 255)
                    {
                        PlayerBuildingUI.refeshOnce = true;
                    }

                    CustomTransferManager.CustomSimulationStepImpl();
                }
            }
        }

        public static void RefreshDummyCargoFuel()
        {
            dummyCargoNeedFuel = (dummyCargoCount > 1000) ? true : false;
            dummyCargoCount = (dummyCargoCount > 1000) ? (ushort)0 : dummyCargoCount;
        }

        public static void RefreshDummyCarFuel()
        {
            dummyCarNeedFuel = (dummyCarCount > 1000) ? true : false;
            dummyCarCount = (dummyCarCount > 1000) ? (ushort)0 : dummyCarCount;
        }

        public static void RefreshCargoFuel()
        {
            cargoNeedFuel = (cargoCount > 1500) ? true : false;
            cargoCount = (cargoCount > 1500) ? (ushort)0 : cargoCount;
        }

        public static void RefreshCarFuel()
        {
            carNeedFuel = (carCount > 1500) ? true : false;
            carCount = (carCount > 1500) ? (ushort)0 : carCount;
        }
    }
}
