using ColossalFramework.UI;
using UnityEngine;
using RealGasStation.Util;
using RealGasStation.NewAI;

namespace RealGasStation.UI
{
    public class PlayerBuildingUI : UIPanel
    {
        public static readonly string cacheName = "PlayerBuildingUI";
        private static readonly float SPACING = 15f;
        private static readonly float SPACING22 = 22f;
        public CityServiceWorldInfoPanel baseBuildingWindow;
        public static bool refeshOnce = false;
        private UILabel Petrol;
        private UILabel inComingVehicleCount;
        private UILabel buildingType;
        private UIDropDown buildingTypeDD;

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

            buildingType = AddUIComponent<UILabel>();
            buildingType.text = Localization.Get("BUILDING_TYPE");
            buildingType.relativePosition = new Vector3(15f, inComingVehicleCount.relativePosition.y + 20f);
            buildingType.autoSize = true;

            buildingTypeDD = UIUtil.CreateDropDown(this);
            buildingTypeDD.items = new string[] { Localization.Get("NORMAL_BUILDING"), Localization.Get("ACCEPT_ALL_VEHICLE"), Localization.Get("ACCEPT_HEAVY_VEHICLE"), Localization.Get("ACCEPT_SMALL_VEHICLE") };
            buildingTypeDD.selectedIndex = MainDataStore.resourceCategory[MainDataStore.lastBuildingID];
            buildingTypeDD.size = new Vector2(250f, 25f);
            buildingTypeDD.relativePosition = new Vector3(15f, buildingType.relativePosition.y + 20f);
            buildingTypeDD.eventSelectedIndexChanged += delegate (UIComponent c, int sel)
            {
                MainDataStore.resourceCategory[MainDataStore.lastBuildingID] = (byte)sel;
            };
        }

        private void RefreshDisplayData()
        {
            if (refeshOnce || (MainDataStore.lastBuildingID != WorldInfoPanel.GetCurrentInstanceID().Building))
            {
                if (base.isVisible)
                {
                    MainDataStore.lastBuildingID = WorldInfoPanel.GetCurrentInstanceID().Building;

                    if (GasStationAI.IsGasBuilding(MainDataStore.lastBuildingID, true) == true)
                    {
                        this.Petrol.text = string.Format(Localization.Get("PETROL_STORED") + " [{0}]", MainDataStore.petrolBuffer[MainDataStore.lastBuildingID]);
                        this.inComingVehicleCount.text = string.Format(Localization.Get("INCOMING_VEHICLE_COUNT") + " [{0}]", MainDataStore.finalVehicleForFuelCount[MainDataStore.lastBuildingID]);
                    }
                    else
                    {
                        this.Petrol.text = "";
                        this.inComingVehicleCount.text = "";
                    }

                    if (buildingType.text != Localization.Get("BUILDING_TYPE"))
                        buildingTypeDD.items = new string[] { Localization.Get("NORMAL_BUILDING"), Localization.Get("ACCEPT_ALL_VEHICLE"), Localization.Get("ACCEPT_HEAVY_VEHICLE"), Localization.Get("ACCEPT_SMALL_VEHICLE") };
                    if (buildingTypeDD.selectedIndex != MainDataStore.resourceCategory[MainDataStore.lastBuildingID])
                        buildingTypeDD.selectedIndex = MainDataStore.resourceCategory[MainDataStore.lastBuildingID];
                    buildingType.text = Localization.Get("BUILDING_TYPE");
                    refeshOnce = false;
                }
                else
                {
                    this.Hide();
                }
            }
        }
    }
}