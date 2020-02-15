using ColossalFramework;
using Harmony;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class VehicleAICalculateTargetSpeedPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(VehicleAI).GetMethod("CalculateTargetSpeed", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(float), typeof(float) }, null);
        }
        public static void Postfix(ref Vehicle data, ref float __result)
        {
            ushort num = FindToll(data.GetLastFramePosition(), 8f);
            if (num != 0)
            {
                __result = 1f;
            }
        }

        public static ushort FindToll(Vector3 pos, float maxDistance)
        {
            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            ushort result = 0;
            BuildingManager building = Singleton<BuildingManager>.instance;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = building.m_buildingGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        BuildingInfo info = building.m_buildings.m_buffer[(int)num6].Info;
                        if (GasStationAI.IsGasBuilding((ushort)num6))
                        {
                            if (!building.m_buildings.m_buffer[(int)num6].m_flags.IsFlagSet(Building.Flags.Deleted))
                            {
                                float num8 = Vector3.SqrMagnitude(pos - building.m_buildings.m_buffer[(int)num6].m_position);
                                if (num8 < num5)
                                {
                                    result = num6;
                                    num5 = num8;
                                    break;
                                }
                            }
                        }
                        num6 = building.m_buildings.m_buffer[(int)num6].m_nextGridBuilding;
                        if (++num7 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
