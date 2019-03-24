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
    public class PlayerBuildingButton : UIPanel
    {
        public static UIButton PBButton;
        private ItemClass.Availability CurrentMode;
        public static PlayerBuildingButton instance;

        public static void PlayerBuildingUIToggle()
        {
            if (!Loader.guiPanel4.isVisible)
            {
                PlayerBuildingUI.refeshOnce = true;
                MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
                Loader.guiPanel4.Show();
            }
            else
            {
                Loader.guiPanel4.Hide();
            }
        }

        public override void Start()
        {
            UIView aView = UIView.GetAView();
            base.name = "PlayerBuildingUIPanel";
            base.opacity = 1f;
            this.CurrentMode = Singleton<ToolManager>.instance.m_properties.m_mode;
            PBButton = base.AddUIComponent<UIButton>();
            PBButton.normalBgSprite = "PBButton";
            PBButton.hoveredBgSprite = "PBButtonHovered";
            PBButton.focusedBgSprite = "PBButtonFocused";
            PBButton.pressedBgSprite = "PBButtonPressed";
            PBButton.playAudioEvents = true;
            PBButton.name = "PBButton";
            PBButton.tooltipBox = aView.defaultTooltipBox;
            if (Loader.m_atlasLoaded)
            {
                UISprite internalSprite = PBButton.AddUIComponent<UISprite>();
                internalSprite.atlas = SpriteUtilities.GetAtlas(Loader.m_atlasName);
                internalSprite.spriteName = "Pic";
                internalSprite.relativePosition = new Vector3(0, 0);
                internalSprite.width = 30f;
                internalSprite.height = 40f;
                base.width = 30f;
                base.height = 40f;
                PBButton.size = new Vector2(30f, 40f);
            }
            else
            {
                PBButton.text = Localization.Get("REALGASSTATION_UI");
                PBButton.textScale = 0.9f;
                base.width = 180f;
                base.height = 15f;
                PBButton.size = new Vector2(150f, 15f);
            }
            PBButton.relativePosition = new Vector3(0f, 0f);
            PBButton.eventClick += delegate (UIComponent component, UIMouseEventParameter eventParam)
            {
                PlayerBuildingButton.PlayerBuildingUIToggle();
            };
        }

        public override void Update()
        {
            MainDataStore.lastBuilding = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (GasStationAI.IsGasBuilding(MainDataStore.lastBuilding) && Loader.isGuiRunning)
            {
                if (!Loader.m_atlasLoaded)
                {
                    PlayerBuildingButton.PBButton.text = Localization.Get("REALGASSTATION_UI");
                }
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