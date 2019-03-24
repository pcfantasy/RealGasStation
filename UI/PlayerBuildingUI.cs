using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using RealGasStation.Util;
using RealGasStation.NewAI;

namespace RealGasStation.UI
{
    public class PlayerBuildingUI : UIPanel
    {
        public static readonly string cacheName = "PlayerBuildingUI";
        private static readonly float SPACING = 15f;
        private static readonly float SPACING22 = 22f;
        private Dictionary<string, UILabel> _valuesControlContainer = new Dictionary<string, UILabel>(16);
        public CityServiceWorldInfoPanel baseBuildingWindow;
        public static bool refeshOnce = false;
        private UILabel Petrol;
        private UILabel inComingVehicleCount;

        public override void Update()
        {
            this.RefreshDisplayData();
            base.Update();
        }

        public override void Awake()
        {
            base.Awake();
            this.DoOnStartup();
        }

        public override void Start()
        {
            base.Start();
            this.canFocus = true;
            this.isInteractive = true;
            base.isVisible = true;
            base.opacity = 1f;
            base.cachedName = cacheName;
            this.RefreshDisplayData();
            base.Hide();
        }

        private void DoOnStartup()
        {
            this.ShowOnGui();
            base.Hide();
        }


        private void ShowOnGui()
        {
            this.Petrol = base.AddUIComponent<UILabel>();
            this.Petrol.text = Localization.Get("PETROL_STORED");
            this.Petrol.relativePosition = new Vector3(SPACING, 50f);
            this.Petrol.autoSize = true;

            this.inComingVehicleCount = base.AddUIComponent<UILabel>();
            this.inComingVehicleCount.text = Localization.Get("INCOMING_VEHICLE_COUNT");
            this.inComingVehicleCount.relativePosition = new Vector3(SPACING, this.Petrol.relativePosition.y + SPACING22);
            this.inComingVehicleCount.autoSize = true;
        }

        private void RefreshDisplayData()
        {
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            uint num2 = currentFrameIndex & 255u;

            if (PlayerBuildingUI.refeshOnce || (MainDataStore.lastBuilding != WorldInfoPanel.GetCurrentInstanceID().Building))
            {
                if (base.isVisible)
                {
                    MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
                    Building buildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.lastBuilding];

                    if (GasStationAI.IsGasBuilding(MainDataStore.lastBuilding) == true)
                    {
                        this.Petrol.text = string.Format(Localization.Get("PETROL_STORED") + " [{0}]", MainDataStore.petrolBuffer[MainDataStore.lastBuilding]);
                    }
                    else
                    {
                        this.Petrol.text = "";
                    }

                    this.inComingVehicleCount.text = string.Format(Localization.Get("INCOMING_VEHICLE_COUNT") + " [{0}]", MainDataStore.finalVehicleForFuelCount[MainDataStore.lastBuilding]);
                    PlayerBuildingUI.refeshOnce = false;
                }
                else
                {
                    this.Hide();
                }
            }

        }

    }
}