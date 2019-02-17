using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.Util;
using System;
using System.Reflection;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomPassengerCarAI : PassengerCarAI
    {
        public bool CustomArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transferType == 112)
            {
                //DebugLog.LogToFileOnly("vehicle arrive at to gas station for petrol now PassengerCarAI");
                vehicleData.m_transferType = MainDataStore.preTranferReason[vehicleID];
                if (MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] > 400)
                {
                    MainDataStore.petrolBuffer[vehicleData.m_targetBuilding] -= 400;
                }
                SetTarget(vehicleID, ref vehicleData, MainDataStore.preTargetBuilding[vehicleID]);
                return true;
            }

            BuildingManager instance = Singleton<BuildingManager>.instance;
            Building building = instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding];
            Building building1 = instance.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding];
            BuildingInfo info = instance.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding].Info;
            var inst = Singleton<PassengerCarAI>.instance;
            var Method = typeof(PassengerCarAI).GetMethod("ArriveAtTarget", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
            //if(Method == null)
            //{
            //    DebugLog.LogToFileOnly("call PassengerCarAI.ArriveAtTarget failed, please check");
            //    return false;
            //}
            Vehicle A = vehicleData;
            ushort B = vehicleID;
            object[] parameters = new object[] { B, A };
            bool return_value = (bool)Method.Invoke(inst, parameters);
            vehicleData = (Vehicle)parameters[1];
            return return_value;
            //return false;
        }


        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if (data.m_transferType == 112)
            {
                //DebugLog.LogToFileOnly("try to add fuel, do not RemoveTarget passengerCarAI");
            }
            else
            {
                RemoveTarget(vehicleID, ref data);
            }

            data.m_targetBuilding = targetBuilding;
            if (targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }

            if (data.m_transferType == 112)
            {
                
                if (!CustomStartPathFind(vehicleID, ref data))
                {
                    if (data.m_targetBuilding != 0)
                    {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                        data.m_targetBuilding = 0;
                    }
                    //DebugLog.LogToFileOnly("fail to find gasstation, try go to orginal place");
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    SetTarget(vehicleID, ref data, MainDataStore.preTargetBuilding[vehicleID]);
                    data.Unspawn(vehicleID);
                }
            }
            else
            {
                if (!StartPathFind(vehicleID, ref data))
                {
                    data.Unspawn(vehicleID);
                }
            }
        }

        protected bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != 0)
            {
                return true;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding].Info;
                    Randomizer randomizer = new Randomizer(vehicleID);
                    Vector3 a, target;
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[vehicleData.m_sourceBuilding], ref randomizer, m_info, out a, out target);


                    var inst = Singleton<CargoTruckAI>.instance;
                    var Method = typeof(CargoTruckAI).GetMethod("StartPathFind", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) }, null);
                    Vehicle A = vehicleData;
                    ushort B = vehicleID;
                    Vector3 C = vehicleData.m_targetPos3;
                    Vector3 D = target;
                    bool E = true;
                    bool F = true;
                    bool G = false;
                    object[] parameters = new object[] { B, A,C,D,E,F,G };
                    bool return_value = (bool)Method.Invoke(inst, parameters);
                    vehicleData = (Vehicle)parameters[1];


                    return return_value;
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer(vehicleID);
                Vector3 b, target2;
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[vehicleData.m_targetBuilding], ref randomizer2, m_info, out b, out target2);

                var inst = Singleton<CargoTruckAI>.instance;
                var Method = typeof(CargoTruckAI).GetMethod("StartPathFind", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) }, null);
                Vehicle A = vehicleData;
                ushort B = vehicleID;
                Vector3 C = vehicleData.m_targetPos3;
                Vector3 D = target2;
                bool E = true;
                bool F = true;
                bool G = false;
                object[] parameters = new object[] { B, A, C, D, E, F, G };
                bool return_value = (bool)Method.Invoke(inst, parameters);
                vehicleData = (Vehicle)parameters[1];


                return return_value;
            }
            return false;
        }


        /*protected bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            if ((vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) != 0)
            {
                return base.StartPathFind(vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
            }
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != (Vehicle.Flags)0;
            PathUnit.Position pathPosA;
            PathUnit.Position pathPosB;
            float distanceSqrA;
            float distanceSqrB;
            PathUnit.Position pathPosA2;
            PathUnit.Position pathPosB2;
            float distanceSqrA2;
            float distanceSqrB2;
            bool flag = PathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, allowUnderground, false, 32f, out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
            if (PathManager.FindPathPosition(startPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, allowUnderground, false, 32f, out pathPosA2, out pathPosB2, out distanceSqrA2, out distanceSqrB2))
            {
                if (!flag || (distanceSqrA2 < distanceSqrA && (Mathf.Abs(startPos.x) > 4800f || Mathf.Abs(startPos.z) > 4800f)))
                {
                    pathPosA = pathPosA2;
                    pathPosB = pathPosB2;
                    distanceSqrA = distanceSqrA2;
                    distanceSqrB = distanceSqrB2;
                }
                flag = true;
            }
            PathUnit.Position pathPosA3;
            PathUnit.Position pathPosB3;
            float distanceSqrA3;
            float distanceSqrB3;
            PathUnit.Position pathPosA4;
            PathUnit.Position pathPosB4;
            float distanceSqrA4;
            float distanceSqrB4;
            bool flag2 = PathManager.FindPathPosition(endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, undergroundTarget, false, 32f, out pathPosA3, out pathPosB3, out distanceSqrA3, out distanceSqrB3);
            if (PathManager.FindPathPosition(endPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, undergroundTarget, false, 32f, out pathPosA4, out pathPosB4, out distanceSqrA4, out distanceSqrB4))
            {
                if (!flag2 || (distanceSqrA4 < distanceSqrA3 && (Mathf.Abs(endPos.x) > 4800f || Mathf.Abs(endPos.z) > 4800f)))
                {
                    pathPosA3 = pathPosA4;
                    pathPosB3 = pathPosB4;
                    distanceSqrA3 = distanceSqrA4;
                    distanceSqrB3 = distanceSqrB4;
                }
                flag2 = true;
            }
            if (flag && flag2)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (!startBothWays || distanceSqrA < 10f)
                {
                    pathPosB = default(PathUnit.Position);
                }
                if (!endBothWays || distanceSqrA3 < 10f)
                {
                    pathPosB3 = default(PathUnit.Position);
                }
                NetInfo.LaneType laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle;
                VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane;
                uint unit = default(uint);
                if (instance.CreatePath(out unit, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, pathPosA, pathPosB, pathPosA3, pathPosB3, default(PathUnit.Position), laneTypes, vehicleTypes, 20000f, IsHeavyVehicle(), IgnoreBlocked(vehicleID, ref vehicleData), false, false, false, false, CombustionEngine()))
                {
                    if (vehicleData.m_path != 0)
                    {
                        instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = unit;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            }
            return false;
        }*/


        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }

            if (MainDataStore.preTargetBuilding[vehicleID] != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[MainDataStore.preTargetBuilding[vehicleID]].RemoveGuestVehicle(vehicleID, ref data);
                MainDataStore.preTargetBuilding[vehicleID] = 0;
            }
        }
    }
}