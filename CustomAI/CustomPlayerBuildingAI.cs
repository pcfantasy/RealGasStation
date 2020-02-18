using ColossalFramework;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealGasStation.CustomAI
{
    public class CustomPlayerBuildingAI
    {
        public static void PlayerBuildingAISimulationStepPostFix(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if (GasStationAI.IsGasBuilding(buildingID))
            {
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    GasStationAI.ProcessGasBuildingIncoming(buildingID, ref buildingData);
                }
            }
        }
    }
}
