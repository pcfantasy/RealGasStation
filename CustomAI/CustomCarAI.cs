using ColossalFramework;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.CustomAI
{
    public class CustomCarAI
    {
        public static void CarAIPathfindFailurePostFix(ushort vehicleID, ref Vehicle data)
        {
            ushort i = vehicleID;
            if (data.m_transferType == 112)
            {
                if (data.Info.m_vehicleAI is CargoTruckAI && (data.m_targetBuilding != 0))
                {
                    CargoTruckAI AI = (CargoTruckAI)data.Info.m_vehicleAI;
#if DEBUG
                    DebugLog.LogToFileOnly("PathFind not success " + i.ToString() + "transferType = " + vehicle.m_transferType.ToString() + "And MainDataStore.TargetGasBuilding[vehicleID] = " + MainDataStore.TargetGasBuilding[i].ToString() + "data.m_targetBuilding = " + vehicle.m_targetBuilding.ToString());
#endif
                    AI.SetTarget((ushort)i, ref data, data.m_targetBuilding);
#if DEBUG
                                DebugLog.LogToFileOnly("Reroute to target " + i.ToString() + "vehicle.m_path = " + vehicle.m_path.ToString() + vehicle.m_flags.ToString());
#endif
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI && data.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                {
                    PassengerCarAI AI = (PassengerCarAI)data.Info.m_vehicleAI;
                    data.m_transferType = MainDataStore.preTranferReason[i];
                    AI.SetTarget((ushort)i, ref data, 0);
                    MainDataStore.TargetGasBuilding[i] = 0;
                    return;
                }
            }
        }
    }
}
