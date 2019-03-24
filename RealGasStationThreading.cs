using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
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
            else if (Loader.isRealConstructionRunning)
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

            DebugLog.LogToFileOnly("Detour AdvancedJunctionRule.NewCarAI::VehicleStatusForRealGasStation calls");
            if (Loader.isAdvancedJunctionRuleRunning)
            {
                try
                {
                    Assembly as1 = Assembly.Load("AdvancedJunctionRule");
                    Loader.Detours.Add(new Loader.Detour(as1.GetType("AdvancedJunctionRule.CustomAI.NewCarAI").GetMethod("VehicleStatusForRealGasStation", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType()}, null), 
                typeof(CustomCarAI).GetMethod("CustomCarAICustomSimulationStepPreFix", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {
                typeof(ushort),
                typeof(Vehicle).MakeByRefType()}, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour AdvancedJunctionRule.NewCarAI::VehicleStatusForRealGasStation");
                    detourFailed = true;
                }
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
                        string error = "RealGasStation detected an incompatibility with another mod! You can continue playing but it's NOT recommended. RealGasStation will not work as expected. See RealGasStation.txt for technical details.";
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
                    if (num4 == 255)
                    {
                        PlayerBuildingUI.refeshOnce = true;
                    }

                    CustomTransferManager.CustomSimulationStepImpl();
                }
            }
        }
    }
}
