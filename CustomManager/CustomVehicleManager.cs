using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealGasStation.CustomManager
{
    public class CustomVehicleManager
    {
        public static void VehicleManagerReleaseVehicleImplementationPreFix(ushort vehicle, ref Vehicle data)
        {
            MainDataStore.TargetGasBuilding[vehicle] = 0;
            MainDataStore.alreadyAskForFuel[vehicle] = false;
            if (data.m_transferType == 112)
            {
                if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    //do not need to remove targetBuilding
                    data.m_targetBuilding = 0;
                }
            }
        }
    }
}
