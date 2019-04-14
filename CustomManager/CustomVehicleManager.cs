using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealGasStation.CustomManager
{
    public class CustomVehicleManager
    {
        public static void VehicleManagerReleaseVehicleImplementationPostFix(ushort vehicle, ref Vehicle data)
        {
            MainDataStore.TargetGasBuilding[vehicle] = 0;
            MainDataStore.alreadyAskForFuel[vehicle] = false;
        }
    }
}
