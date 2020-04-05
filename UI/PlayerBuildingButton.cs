using ColossalFramework.UI;
using RealGasStation.NewAI;
using RealGasStation.Util;
using UnityEngine;

namespace RealGasStation.UI
{
    public class PlayerBuildingButton : UIButton
    {
        private UIPanel playerBuildingInfo;
        private PlayerBuildingUI playerBuildingUI;
        private InstanceID BuildingID = InstanceID.Empty;
        public void PlayerBuildingUIToggle()
        {
            if ((!playerBuildingUI.isVisible) && (BuildingID != InstanceID.Empty))
            {
                playerBuildingUI.position = new Vector3(playerBuildingInfo.size.x, playerBuildingInfo.size.y);
                playerBuildingUI.size = new Vector3(playerBuildingInfo.size.x, playerBuildingInfo.size.y);
                PlayerBuildingUI.refeshOnce = true;
                playerBuildingUI.Show();
            }
            else
            {
                playerBuildingUI.Hide();
            }
        }

        public override void Start()
        {
            base.normalBgSprite = "ToolbarIconGroup1Nomarl";
            base.hoveredBgSprite = "ToolbarIconGroup1Hovered";
            base.focusedBgSprite = "ToolbarIconGroup1Focused";
            base.pressedBgSprite = "ToolbarIconGroup1Pressed";
            base.playAudioEvents = true;
            base.name = "PBButton";
            UISprite internalSprite = base.AddUIComponent<UISprite>();
            internalSprite.atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName);
            internalSprite.spriteName = "Pic";
            internalSprite.relativePosition = new Vector3(0, 0);
            internalSprite.width = 40f;
            internalSprite.height = 40f;
            base.width = 40f;
            base.height = 40f;
            base.size = new Vector2(40f, 40f);
            //Setup PlayerBuildingUI
            var buildingWindowGameObject = new GameObject("buildingWindowObject");
            playerBuildingUI = (PlayerBuildingUI)buildingWindowGameObject.AddComponent(typeof(PlayerBuildingUI));
            playerBuildingInfo = UIView.Find<UIPanel>("(Library) CityServiceWorldInfoPanel");
            if (playerBuildingInfo == null)
            {
                DebugLog.LogToFileOnly("UIPanel not found (update broke the mod!): (Library) CityServiceWorldInfoPanel\nAvailable panels are:\n");
            }
            playerBuildingUI.transform.parent = playerBuildingInfo.transform;
            playerBuildingUI.baseBuildingWindow = playerBuildingInfo.gameObject.transform.GetComponentInChildren<CityServiceWorldInfoPanel>();
            base.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                PlayerBuildingUIToggle();
            };
        }

        public override void Update()
        {
            var Building = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (WorldInfoPanel.GetCurrentInstanceID() != InstanceID.Empty)
            {
                BuildingID = WorldInfoPanel.GetCurrentInstanceID();
            }

            if (GasStationAI.IsGasBuilding(Building) && Loader.isGuiRunning)
            {
                relativePosition = new Vector3(playerBuildingInfo.size.x - width - 90, playerBuildingInfo.size.y - height);
                base.Show();
            }

            base.Update();
        }
    }
}