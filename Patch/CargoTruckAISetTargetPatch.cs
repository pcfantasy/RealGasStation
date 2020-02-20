using ColossalFramework;
using Harmony;
using RealGasStation.CustomAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class CargoTruckAISetTargetPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null);
        }
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if ((data.m_transferType == 113) || (data.m_transferType == 112))
            {
                if (targetBuilding == data.m_targetBuilding)
                {
                    return true;
                }
                else
                {
                    data.m_flags &= ~Vehicle.Flags.WaitingTarget;
                    data.m_waitCounter = 0;
                    ushort tempTargetBuilding = data.m_targetBuilding;
                    data.m_targetBuilding = MainDataStore.TargetGasBuilding[vehicleID];
                    bool success = CustomCargoTruckAI.CustomStartPathFind(vehicleID, ref data);
                    data.m_targetBuilding = tempTargetBuilding;
                    if (!success)
                    {
                        data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                        PathManager instance = Singleton<PathManager>.instance;
                        if (data.m_path != 0u)
                        {
                            instance.ReleasePath(data.m_path);
                            data.m_path = 0;
                        }
                        __instance.SetTarget(vehicleID, ref data, data.m_targetBuilding);
                        MainDataStore.TargetGasBuilding[vehicleID] = 0;
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
