using ColossalFramework;
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

        public static bool isGuiRunning = false;

        public static bool realCityRunning = false;
        public static bool realConstructionRunning = false;

        public static UIPanel playerbuildingInfo;

        public static PlayerBuildingUI guiPanel4;

        public static PlayerBuildingButton PBMenuPanel;

        public static GameObject PlayerbuildingWindowGameObject;

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
                    SetupGui();
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


        public static void SetupGui()
        {
            SetupPlayerBuidingGui();
            SetupPlayerBuildingButton();
            Loader.isGuiRunning = true;
        }

        public static void RemoveGui()
        {
            Loader.isGuiRunning = false;
            if (!realCityRunning)
            {
                if (playerbuildingInfo != null)
                {
                    UnityEngine.Object.Destroy(PBMenuPanel);
                    Loader.PBMenuPanel = null;
                }
            }

            //remove PlayerbuildingUI
            if (guiPanel4 != null)
            {
                if (guiPanel4.parent != null)
                {
                    guiPanel4.parent.eventVisibilityChanged -= playerbuildingInfo_eventVisibilityChanged;
                }
            }

            if (PlayerbuildingWindowGameObject != null)
            {
                UnityEngine.Object.Destroy(PlayerbuildingWindowGameObject);
            }
        }

        public static void SetupPlayerBuidingGui()
        {
            PlayerbuildingWindowGameObject = new GameObject("PlayerbuildingWindowGameObject");
            guiPanel4 = (PlayerBuildingUI)PlayerbuildingWindowGameObject.AddComponent(typeof(PlayerBuildingUI));


            playerbuildingInfo = UIView.Find<UIPanel>("(Library) CityServiceWorldInfoPanel");
            if (playerbuildingInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) CityServiceWorldInfoPanel\nAvailable panels are:\n");
            }
            guiPanel4.transform.parent = playerbuildingInfo.transform;
            guiPanel4.size = new Vector3(playerbuildingInfo.size.x, playerbuildingInfo.size.y);
            guiPanel4.baseBuildingWindow = playerbuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel>();
            guiPanel4.position = new Vector3(playerbuildingInfo.size.x, playerbuildingInfo.size.y);
            playerbuildingInfo.eventVisibilityChanged += playerbuildingInfo_eventVisibilityChanged;
        }


        public static void SetupPlayerBuildingButton()
        {
            if (PBMenuPanel == null)
            {
                PBMenuPanel = (playerbuildingInfo.AddUIComponent(typeof(PlayerBuildingButton)) as PlayerBuildingButton);
            }
            PBMenuPanel.RefPanel = playerbuildingInfo;
            PBMenuPanel.Alignment = UIAlignAnchor.BottomRight;
            PBMenuPanel.Show();
        }

        public static void playerbuildingInfo_eventVisibilityChanged(UIComponent component, bool value)
        {
            guiPanel4.isEnabled = value;
            if (value)
            {
                Loader.guiPanel4.transform.parent = Loader.playerbuildingInfo.transform;
                Loader.guiPanel4.size = new Vector3(Loader.playerbuildingInfo.size.x, Loader.playerbuildingInfo.size.y);
                Loader.guiPanel4.baseBuildingWindow = Loader.playerbuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel>();
                Loader.guiPanel4.position = new Vector3(Loader.playerbuildingInfo.size.x, Loader.playerbuildingInfo.size.y);
                //DebugLog.LogToFileOnly("select building found!!!!!:\n");
                //comm_data.current_buildingid = 0;
                //PlayerBuildingUI.refesh_once = true;
                //guiPanel4.Show();
            }
            else
            {
                //comm_data.current_buildingid = 0;
                guiPanel4.Hide();
            }
        }


        public void InitDetour()
        {
            realCityRunning = CheckRealCityIsLoaded();
            realConstructionRunning = CheckRealConstructionIsLoaded();


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
                /*DebugLog.LogToFileOnly("Detour CargoTruckAI::RemoveTarget calls");
                try
                {
                    Detours.Add(new Detour(typeof(CargoTruckAI).GetMethod("RemoveTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null),
                                           typeof(CustomCargoTruckAI).GetMethod("RemoveTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour CargoTruckAI::RemoveTarget");
                    detourFailed = true;
                }*/

                //3
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

                //4
                /*DebugLog.LogToFileOnly("Detour PassengerCarAI::RemoveTarget calls");
                try
                {
                    Detours.Add(new Detour(typeof(PassengerCarAI).GetMethod("RemoveTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null),
                                           typeof(CustomPassengerCarAI).GetMethod("RemoveTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType()}, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour PassengerCarAI::RemoveTarget");
                    detourFailed = true;
                }*/

                //5
                DebugLog.LogToFileOnly("Detour VehicleAI::CalculateTargetSpeed calls");
                try
                {
                    Detours.Add(new Detour(typeof(VehicleAI).GetMethod("CalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null),
                                           typeof(CustomVehicleAI).GetMethod("CustomCalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null)));
                }
                catch (Exception)
                {
                    DebugLog.LogToFileOnly("Could not detour VehicleAI::CalculateTargetSpeed");
                    detourFailed = true;
                }

                //public override void UpdateBuildingTargetPositions(ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
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

                if (!realConstructionRunning || !realCityRunning)
                {
                    if (!realConstructionRunning && !realCityRunning)
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
                    if (!realConstructionRunning)
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
                    if (!realCityRunning)
                    {
                        //8
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

                        //9
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
                    DebugLog.LogToFileOnly("Both RealCity and RealConstruction are Running");
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

    }
}
