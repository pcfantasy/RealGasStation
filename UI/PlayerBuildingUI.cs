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
        public static UICheckBox both;
        public static UICheckBox heavy;
        public static UICheckBox small;
        private UILabel bothText;
        private UILabel heavyText;
        private UILabel smallText;

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

            both = base.AddUIComponent<UICheckBox>();
            both.relativePosition = new Vector3(15f, inComingVehicleCount.relativePosition.y + 20f);
            this.bothText = base.AddUIComponent<UILabel>();
            this.bothText.relativePosition = new Vector3(both.relativePosition.x + both.width + 20f, both.relativePosition.y + 5f);
            both.height = 16f;
            both.width = 16f;
            both.label = this.bothText;
            both.text = Localization.Get("ACCEPT_ALL_VEHICLE");
            UISprite uISprite0 = both.AddUIComponent<UISprite>();
            uISprite0.height = 20f;
            uISprite0.width = 20f;
            uISprite0.relativePosition = new Vector3(0f, 0f);
            uISprite0.spriteName = "check-unchecked";
            uISprite0.isVisible = true;
            UISprite uISprite1 = both.AddUIComponent<UISprite>();
            uISprite1.height = 20f;
            uISprite1.width = 20f;
            uISprite1.relativePosition = new Vector3(0f, 0f);
            uISprite1.spriteName = "check-checked";
            both.checkedBoxObject = uISprite1;
            both.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 0) ? true : false;
            both.isEnabled = true;
            both.isVisible = true;
            both.canFocus = true;
            both.isInteractive = true;
            both.eventCheckChanged += delegate (UIComponent component, bool eventParam)
            {
                Both_OnCheckChanged(component, eventParam);
            };

            heavy = base.AddUIComponent<UICheckBox>();
            heavy.relativePosition = new Vector3(15f, both.relativePosition.y + 20f);
            this.heavyText = base.AddUIComponent<UILabel>();
            this.heavyText.relativePosition = new Vector3(heavy.relativePosition.x + heavy.width + 20f, heavy.relativePosition.y + 5f);
            heavy.height = 16f;
            heavy.width = 16f;
            heavy.label = this.heavyText;
            heavy.text = Localization.Get("ACCEPT_HEAVY_VEHICLE");
            UISprite uISprite2 = heavy.AddUIComponent<UISprite>();
            uISprite2.height = 20f;
            uISprite2.width = 20f;
            uISprite2.relativePosition = new Vector3(0f, 0f);
            uISprite2.spriteName = "check-unchecked";
            uISprite2.isVisible = true;
            UISprite uISprite3 = heavy.AddUIComponent<UISprite>();
            uISprite3.height = 20f;
            uISprite3.width = 20f;
            uISprite3.relativePosition = new Vector3(0f, 0f);
            uISprite3.spriteName = "check-checked";
            heavy.checkedBoxObject = uISprite3;
            heavy.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 1) ? true : false;
            heavy.isEnabled = true;
            heavy.isVisible = true;
            heavy.canFocus = true;
            heavy.isInteractive = true;
            heavy.eventCheckChanged += delegate (UIComponent component, bool eventParam)
            {
                heavy_OnCheckChanged(component, eventParam);
            };

            small = base.AddUIComponent<UICheckBox>();
            small.relativePosition = new Vector3(15f, heavy.relativePosition.y + 20f);
            this.smallText = base.AddUIComponent<UILabel>();
            this.smallText.relativePosition = new Vector3(small.relativePosition.x + small.width + 20f, small.relativePosition.y + 5f);
            small.height = 16f;
            small.width = 16f;
            small.label = this.smallText;
            small.text = Localization.Get("ACCEPT_SMALL_VEHICLE");
            UISprite uISprite4 = small.AddUIComponent<UISprite>();
            uISprite4.height = 20f;
            uISprite4.width = 20f;
            uISprite4.relativePosition = new Vector3(0f, 0f);
            uISprite4.spriteName = "check-unchecked";
            uISprite4.isVisible = true;
            UISprite uISprite5 = small.AddUIComponent<UISprite>();
            uISprite5.height = 20f;
            uISprite5.width = 20f;
            uISprite5.relativePosition = new Vector3(0f, 0f);
            uISprite5.spriteName = "check-checked";
            small.checkedBoxObject = uISprite5;
            small.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 2) ? true : false;
            small.isEnabled = true;
            small.isVisible = true;
            small.canFocus = true;
            small.isInteractive = true;
            small.eventCheckChanged += delegate (UIComponent component, bool eventParam)
            {
                small_OnCheckChanged(component, eventParam);
            };
        }

        public static void Both_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (bValue)
            {
                MainDataStore.resourceCategory[MainDataStore.lastBuilding] = 0;
                small.isChecked = false;
                heavy.isChecked = false;
                both.isChecked = true;
            }
            else
            {
                if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 0)
                {
                    both.isChecked = true;
                    small.isChecked = false;
                    heavy.isChecked = false;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 1)
                {
                    both.isChecked = false;
                    small.isChecked = false;
                    heavy.isChecked = true;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 2)
                {
                    both.isChecked = false;
                    small.isChecked = true;
                    heavy.isChecked = false;
                }
            }
        }

        public static void heavy_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (bValue)
            {
                MainDataStore.resourceCategory[MainDataStore.lastBuilding] = 1;
                small.isChecked = false;
                heavy.isChecked = true;
                both.isChecked = false;
            }
            else
            {
                if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 0)
                {
                    both.isChecked = true;
                    small.isChecked = false;
                    heavy.isChecked = false;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 1)
                {
                    both.isChecked = false;
                    small.isChecked = false;
                    heavy.isChecked = true;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 2)
                {
                    both.isChecked = false;
                    small.isChecked = true;
                    heavy.isChecked = false;
                }
            }
        }

        public static void small_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (bValue)
            {
                MainDataStore.resourceCategory[MainDataStore.lastBuilding] = 2;
                small.isChecked = true;
                heavy.isChecked = false;
                both.isChecked = false;
            }
            else
            {
                if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 0)
                {
                    both.isChecked = true;
                    small.isChecked = false;
                    heavy.isChecked = false;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 1)
                {
                    both.isChecked = false;
                    small.isChecked = false;
                    heavy.isChecked = true;
                }
                else if (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 2)
                {
                    both.isChecked = false;
                    small.isChecked = true;
                    heavy.isChecked = false;
                }
            }
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
                        this.inComingVehicleCount.text = string.Format(Localization.Get("INCOMING_VEHICLE_COUNT") + " [{0}]", MainDataStore.finalVehicleForFuelCount[MainDataStore.lastBuilding]);
                        both.isVisible = true;
                        heavy.isVisible = true;
                        small.isVisible = true;
                        both.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 0) ? true : false;
                        heavy.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 1) ? true : false;
                        small.isChecked = (MainDataStore.resourceCategory[MainDataStore.lastBuilding] == 2) ? true : false;
                        both.text = Localization.Get("ACCEPT_ALL_VEHICLE");
                        heavy.text = Localization.Get("ACCEPT_HEAVY_VEHICLE");
                        small.text = Localization.Get("ACCEPT_SMALL_VEHICLE");
                    }
                    else
                    {
                        this.Petrol.text = "";
                        this.inComingVehicleCount.text = "";
                        both.isVisible = false;
                        heavy.isVisible = false;
                        small.isVisible = false;
                        Hide();
                    }
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