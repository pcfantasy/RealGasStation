using ColossalFramework;
using ColossalFramework.UI;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation.UI
{
    public class PlayerBuildingButton : UIButton
    {
        public static void PlayerBuildingUIToggle()
        {
            if (!Loader.playerBuildingUI.isVisible)
            {
                PlayerBuildingUI.refeshOnce = true;
                MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
                Loader.playerBuildingUI.Show();
            }
            else
            {
                Loader.playerBuildingUI.Hide();
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
            base.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                PlayerBuildingButton.PlayerBuildingUIToggle();
            };
        }

        public override void Update()
        {
            MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (GasStationAI.IsGasBuilding(MainDataStore.lastBuilding) && Loader.isGuiRunning)
            {
                base.Show();
            }
            else
            {
                base.Hide();
            }
            base.Update();
        }
    }
}