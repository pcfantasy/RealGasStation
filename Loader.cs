using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using RealGasStation.CustomAI;
using RealGasStation.CustomManager;
using RealGasStation.UI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation
{
    public class Loader : LoadingExtensionBase
    {
        public static LoadMode CurrentLoadMode;

        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }

        public static List<Detour> Detours { get; set; }
        public static bool DetourInited = false;
        public static bool HarmonyDetourInited = false;
        public static bool HarmonyDetourFailed = true;
        public static bool isGuiRunning = false;
        public static bool isRealCityRunning = false;
        public static bool isRealConstructionRunning = false;
        public static bool isAdvancedJunctionRuleRunning = false;
        public static PlayerBuildingButton PBButton;
        public static string m_atlasName = "RealGasStation";
        public static bool m_atlasLoaded;

        public override void OnCreated(ILoading loading)
        {
            Detours = new List<Detour>();
            base.OnCreated(loading);
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Loader.CurrentLoadMode = mode;
            if (RealGasStation.IsEnabled)
            {
                if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                {
                    DebugLog.LogToFileOnly("OnLevelLoaded");
                    InitDetour();
                    HarmonyInitDetour();
                    SetupGui();
                    for (int i = 0; i < 16384; i++)
                    {
                        CustomCarAI.watingPathTime[i] = 0;
                        CustomCarAI.stuckTime[i] = 0;
                    }
                    if (mode == LoadMode.NewGame)
                    {
                        DebugLog.LogToFileOnly("New Game");
                    }
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (RealGasStation.IsEnabled)
                {
                    RevertDetour();
                    HarmonyRevertDetour();
                    RealGasStationThreading.isFirstTime = true;
                    if (Loader.isGuiRunning)
                    {
                        RemoveGui();
                    }
                }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }

        private static void LoadSprites()
        {
            if (SpriteUtilities.GetAtlas(m_atlasName) != null) return;
            var modPath = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath;
            m_atlasLoaded = SpriteUtilities.InitialiseAtlas(Path.Combine(modPath, "Icon/RealGasStation.png"), m_atlasName);
            if (m_atlasLoaded)
            {
                var spriteSuccess = true;
                spriteSuccess = SpriteUtilities.AddSpriteToAtlas(new Rect(new Vector2(1, 1), new Vector2(191, 191)), "Pic", m_atlasName)
                             && spriteSuccess;
                if (!spriteSuccess) DebugLog.LogToFileOnly("Some sprites haven't been loaded. This is abnormal; you should probably report this to the mod creator.");
            }
            else DebugLog.LogToFileOnly("The texture atlas (provides custom icons) has not loaded. All icons have reverted to text prompts.");
        }

        public static void SetupGui()
        {
            LoadSprites();
            if (m_atlasLoaded)
            {
                SetupPlayerBuildingButton();
                Loader.isGuiRunning = true;
            }
        }

        public static void RemoveGui()
        {
            Loader.isGuiRunning = false;
            if (PBButton != null)
            {
                UnityEngine.Object.Destroy(PBButton);
                Loader.PBButton = null;
            }
        }

        public static void SetupPlayerBuildingButton()
        {
            var playerbuildingInfo = UIView.Find<UIPanel>("(Library) CityServiceWorldInfoPanel");
            if (PBButton == null)
            {
                PBButton = (playerbuildingInfo.AddUIComponent(typeof(PlayerBuildingButton)) as PlayerBuildingButton);
            }
            PBButton.width = 30f;
            PBButton.height = 40f;
            PBButton.relativePosition = new Vector3(playerbuildingInfo.size.x - PBButton.width - 90, playerbuildingInfo.size.y - PBButton.height);
            PBButton.Show();
        }

        public void InitDetour()
        {
            isRealCityRunning = CheckRealCityIsLoaded();
            isRealConstructionRunning = CheckRealConstructionIsLoaded();
            isAdvancedJunctionRuleRunning = CheckAdvancedJunctionRuleIsLoaded();

            if (!DetourInited)
            {
                DebugLog.LogToFileOnly("Init detours");
                bool detourFailed = false;

                //1
                DebugLog.LogToFileOnly("Detour CargoTruckAI::SetTarget calls");
                try
                {
                    Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CargoTruckAI::SetTarget");
                    detourFailed = true;
                }

                //2
                DebugLog.LogToFileOnly("Detour PassengerCarAI::SetTarget calls");
                try
                {
                    Detours.Add(new Detour(typeof(PassengerCarAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null),
                                           typeof(CustomPassengerCarAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour PassengerCarAI::SetTarget");
                    detourFailed = true;
                }

                //3
                /*DebugLog.LogToFileOnly("Detour VehicleAI::CalculateTargetSpeed calls");
                try
                {
                    Detours.Add(new Detour(typeof(VehicleAI).GetMethod("CalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null),
                                           typeof(CustomVehicleAI).GetMethod("CustomCalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour VehicleAI::CalculateTargetSpeed");
                    detourFailed = true;
                }*/

                //4
                DebugLog.LogToFileOnly("Detour CargoTruckAI::UpdateBuildingTargetPositions calls");
                try
                {
                    Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("UpdateBuildingTargetPositions", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() , typeof(Vector3), typeof(ushort), typeof(Vehicle).MakeByRefType() , typeof(int).MakeByRefType(), typeof(float) }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("UpdateBuildingTargetPositions", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(int).MakeByRefType(), typeof(float) }, null)));

                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CargoTruckAI::UpdateBuildingTargetPositions");
                    detourFailed = true;
                }

                //5
                DebugLog.LogToFileOnly("Detour CargoTruckAI::ArriveAtSource calls");
                try
                {
                    Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("ArriveAtSource", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("ArriveAtSource", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CargoTruckAI::ArriveAtSource");
                    detourFailed = true;
                }

                if (true)
                //if (!isRealConstructionRunning || !isRealCityRunning)
                {
                    //if (!isRealConstructionRunning && !isRealCityRunning)
                    if (true)
                    {
                        //6
                        DebugLog.LogToFileOnly("Detour CargoTruckAI::ArriveAtTarget calls");
                        try
                        {
                            Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                                   typeof(CustomCargoTruckAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                        }
                        catch (Exception)
                        {
                            DebugLog.LogToFileOnly("Could not detour CargoTruckAI::ArriveAtTarget");
                            detourFailed = true;
                        }
                    }

                    if (!isRealConstructionRunning)
                    {
                        //7
                        DebugLog.LogToFileOnly("Detour CargoTruckAI::GetLocalizedStatus calls");
                        try
                        {
                            Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("GetLocalizedStatus", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InstanceID).MakeByRefType() }, null),
                                                   typeof(CustomCargoTruckAI).GetMethod("GetLocalizedStatus", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InstanceID).MakeByRefType() }, null)));
                        }
                        catch (Exception)
                        {
                            DebugLog.LogToFileOnly("Could not detour CargoTruckAI::GetLocalizedStatus");
                            detourFailed = true;
                        }
                    }

                    //if (!isRealCityRunning)
                    if (true)
                    {
                        //8
                        DebugLog.LogToFileOnly("Detour PassengerCarAI::ArriveAtTarget calls");
                        try
                        {
                            Detours.Add(new Detour(typeof(PassengerCarAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                                   typeof(CustomPassengerCarAI).GetMethod("CustomArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                        }
                        catch (Exception)
                        {
                            DebugLog.LogToFileOnly("Could not detour PassengerCarAI::ArriveAtTarget");
                            detourFailed = true;
                        }
                    }
                }
                else
                {
                    //DebugLog.LogToFileOnly("Both RealCity and RealConstruction are Running");
                }

                if (detourFailed)
                {
                    DebugLog.LogToFileOnly("Detours failed");
                }
                else
                {
                    DebugLog.LogToFileOnly("Detours successful");
                }
                DetourInited = true;
            }
        }

        public void RevertDetour()
        {
            if (DetourInited)
            {
                DebugLog.LogToFileOnly("Revert detours");
                Detours.Reverse();
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                DetourInited = false;
                Detours.Clear();
                DebugLog.LogToFileOnly("Reverting detours finished.");
            }
        }

        public void HarmonyInitDetour()
        {
            if (!HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Init harmony detours");
                HarmonyDetours.Apply();
                HarmonyDetourInited = true;
            }
        }

        public void HarmonyRevertDetour()
        {
            if (HarmonyDetourInited)
            {
                DebugLog.LogToFileOnly("Revert harmony detours");
                HarmonyDetours.DeApply();
                HarmonyDetourInited = false;
                HarmonyDetourFailed = true;
            }
        }

        private bool Check3rdPartyModLoaded(string namespaceStr, bool printAll = false)
        {
            bool thirdPartyModLoaded = false;

            var loadingWrapperLoadingExtensionsField = typeof(LoadingWrapper).GetField("m_LoadingExtensions", BindingFlags.NonPublic | BindingFlags.Instance);
            List<ILoadingExtension> loadingExtensions = (List<ILoadingExtension>)loadingWrapperLoadingExtensionsField.GetValue(Singleton<LoadingManager>.instance.m_LoadingWrapper);

            if (loadingExtensions != null)
            {
                foreach (ILoadingExtension extension in loadingExtensions)
                {
                    if (printAll)
                        DebugLog.LogToFileOnly($"Detected extension: {extension.GetType().Name} in namespace {extension.GetType().Namespace}");
                    if (extension.GetType().Namespace == null)
                        continue;

                    var nsStr = extension.GetType().Namespace.ToString();
                    if (namespaceStr.Equals(nsStr))
                    {
                        DebugLog.LogToFileOnly($"The mod '{namespaceStr}' has been detected.");
                        thirdPartyModLoaded = true;
                        break;
                    }
                }
            }
            else
            {
                DebugLog.LogToFileOnly("Could not get loading extensions");
            }

            return thirdPartyModLoaded;
        }

        private bool CheckRealCityIsLoaded()
        {
            return this.Check3rdPartyModLoaded("RealCity", true);
        }

        private bool CheckRealConstructionIsLoaded()
        {
            return this.Check3rdPartyModLoaded("RealConstruction", true);
        }

        private bool CheckAdvancedJunctionRuleIsLoaded()
        {
            return this.Check3rdPartyModLoaded("AdvancedJunctionRule", true);
        }
    }
}
