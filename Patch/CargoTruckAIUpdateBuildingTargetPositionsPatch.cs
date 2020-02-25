using ColossalFramework;
using ColossalFramework.Math;
using Harmony;
using RealGasStation.CustomAI;
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
    public static class CargoTruckAIUpdateBuildingTargetPositionsPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CargoTruckAI).GetMethod("UpdateBuildingTargetPositions", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(int).MakeByRefType(), typeof(float) }, null);
        }
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos, ushort leaderID, ref Vehicle leaderData, ref int index, float minSqrDistance)
        {
            if ((leaderData.m_flags & Vehicle.Flags.WaitingTarget) == (Vehicle.Flags)0)
            {
                if ((leaderData.m_flags & Vehicle.Flags.GoingBack) == (Vehicle.Flags)0)
                {
                    if (leaderData.m_targetBuilding != 0)
                    {
                        BuildingManager instance2 = Singleton<BuildingManager>.instance;
                        BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]].Info;
                        float distance = Vector3.Distance(instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]].m_position, leaderData.GetLastFramePosition());
                        if (((leaderData.m_transferType == 213) || (leaderData.m_transferType == 212)) && (distance < 50))
                        {
                            if (leaderID == vehicleID)
                            {
                                if (MainDataStore.TargetGasBuilding[leaderID] != 0)
                                {
                                    Randomizer randomizer2 = new Randomizer((int)vehicleID);
                                    Vector3 targetPos2;
                                    Vector3 vector2;
                                    info2.m_buildingAI.CalculateUnspawnPosition(MainDataStore.TargetGasBuilding[leaderID], ref instance2.m_buildings.m_buffer[(int)MainDataStore.TargetGasBuilding[leaderID]], ref randomizer2, __instance.m_info, out targetPos2, out vector2);
                                    vehicleData.SetTargetPos(index++, CalculateTargetPoint(refPos, targetPos2, minSqrDistance, 4f));
                                    return false;
                                }
                            }
                            else
                            {
                                DebugLog.LogToFileOnly("Error: No such case for RealGasStation");
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static Vector4 CalculateTargetPoint(Vector3 refPos, Vector3 targetPos, float maxSqrDistance, float speed)
        {
            Vector3 a = targetPos - refPos;
            float sqrMagnitude = a.sqrMagnitude;
            Vector4 result = (!(sqrMagnitude > maxSqrDistance)) ? ((Vector4)targetPos) : ((Vector4)(refPos + a * Mathf.Sqrt(maxSqrDistance / sqrMagnitude)));
            result.w = speed;
            return result;
        }
    }
}
