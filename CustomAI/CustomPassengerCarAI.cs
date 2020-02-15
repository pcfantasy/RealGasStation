using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.Util;
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomPassengerCarAI : PassengerCarAI
    {
        public void PassengerCarAIArriveAtTargetForRealGasStationPre(ushort vehicleID, ref Vehicle data)
        {
            if (MainDataStore.petrolBuffer[data.m_targetBuilding] > 400)
            {
                MainDataStore.petrolBuffer[data.m_targetBuilding] -= 400;
            }
            SetTarget(vehicleID, ref data, 0);
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + 1800f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level1);
            }
        }

        private ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            return instance2;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }

        public static void GetVehicleRunningTimingForRealCity(ushort vehicleID, ref Vehicle vehicleData)
        {
            DebugLog.LogToFileOnly("Error: Should be detour by RealCity @ GetVehicleRunningTimingForRealCity");
        }

        private bool CustomArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            //RealCity Mod related
            if (Loader.isRealCityRunning)
            {
                GetVehicleRunningTimingForRealCity(vehicleID, ref data);
            }
            //RealGasStation Mod related
            if (data.m_transferType == 112)
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    PassengerCarAIArriveAtTargetForRealGasStationPre(vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = false;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    data.Unspawn(vehicleID);
                }
                return true;
            }
            // NON-STOCK CODE END
            if ((data.m_flags & Vehicle.Flags.Parking) != (Vehicle.Flags)0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                CitizenManager instance2 = Singleton<CitizenManager>.instance;
                ushort driverInstance = this.GetDriverInstance(vehicleID, ref data);
                if (driverInstance != 0)
                {
                    uint citizen = instance2.m_instances.m_buffer[(int)driverInstance].m_citizen;
                    if (citizen != 0u)
                    {
                        ushort parkedVehicle = instance2.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_parkedVehicle;
                        if (parkedVehicle != 0)
                        {
                            Vehicle.Frame lastFrameData = data.GetLastFrameData();
                            instance.m_parkedVehicles.m_buffer[(int)parkedVehicle].m_travelDistance = lastFrameData.m_travelDistance;
                            VehicleParked[] expr_A1_cp_0 = instance.m_parkedVehicles.m_buffer;
                            ushort expr_A1_cp_1 = parkedVehicle;
                            expr_A1_cp_0[(int)expr_A1_cp_1].m_flags = (ushort)(expr_A1_cp_0[(int)expr_A1_cp_1].m_flags & 65527);
                            InstanceID empty = InstanceID.Empty;
                            empty.Vehicle = vehicleID;
                            InstanceID empty2 = InstanceID.Empty;
                            empty2.ParkedVehicle = parkedVehicle;
                            Singleton<InstanceManager>.instance.ChangeInstance(empty, empty2);
                        }
                    }
                }
            }
            this.UnloadPassengers(vehicleID, ref data);
            if (data.m_targetBuilding == 0)
            {
                return true;
            }
            data.m_targetPos0 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].CalculateSidewalkPosition();
            data.m_targetPos0.w = 2f;
            data.m_targetPos1 = data.m_targetPos0;
            data.m_targetPos2 = data.m_targetPos0;
            data.m_targetPos3 = data.m_targetPos0;
            this.RemoveTarget(vehicleID, ref data);
            return true;
        }

        private void UnloadPassengers(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            CitizenInfo info = instance.m_instances.m_buffer[(int)instance2].Info;
                            info.m_citizenAI.SetCurrentVehicle(instance2, ref instance.m_instances.m_buffer[(int)instance2], 0, 0u, data.m_targetPos0);
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public override void SetTarget(ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if ((data.m_transferType != 113) && (data.m_transferType != 112))
            {
                RemoveTarget(vehicleID, ref data);
            }
            data.m_targetBuilding = targetBuilding;
            if (targetBuilding != 0)
            {
                if ((data.m_transferType != 113) && (data.m_transferType != 112))
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
                }
            }

            if ((data.m_transferType == 112) || (data.m_transferType == 113))
            {             
                if (!HackedStartPathFind(vehicleID, ref data))
                {
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    data.m_targetBuilding = 0;
                    SetTarget(vehicleID, ref data, 0);
                    MainDataStore.TargetGasBuilding[vehicleID] = 0;
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

        protected bool HackedStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.WaitingTarget) != (Vehicle.Flags)0)
            {
                return true;
            }
            if ((vehicleData.m_flags & Vehicle.Flags.GoingBack) != (Vehicle.Flags)0)
            {
                if (vehicleData.m_sourceBuilding != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    BuildingInfo info = instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding].Info;
                    Randomizer randomizer = new Randomizer((int)vehicleID);
                    Vector3 vector;
                    Vector3 endPos;
                    info.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_sourceBuilding, ref instance.m_buildings.m_buffer[(int)vehicleData.m_sourceBuilding], ref randomizer, this.m_info, out vector, out endPos);
                    return HackedStartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, endPos, true, true, false);
                }
            }
            else if (vehicleData.m_targetBuilding != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance2.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding].Info;
                Randomizer randomizer2 = new Randomizer((int)vehicleID);
                Vector3 vector2;
                Vector3 endPos2;
                info2.m_buildingAI.CalculateUnspawnPosition(vehicleData.m_targetBuilding, ref instance2.m_buildings.m_buffer[(int)vehicleData.m_targetBuilding], ref randomizer2, this.m_info, out vector2, out endPos2);
                return HackedStartPathFind(vehicleID, ref vehicleData, vehicleData.m_targetPos3, endPos2, true, true, false);
            }
            return false;
        }

        protected bool HackedStartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)
        {
            if ((vehicleData.m_flags & (Vehicle.Flags.TransferToSource | Vehicle.Flags.GoingBack)) != (Vehicle.Flags)0)
            {
                return base.StartPathFind(vehicleID, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
            }
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != (Vehicle.Flags)0;
            PathUnit.Position startPosA;
            PathUnit.Position startPosB;
            float num;
            float num2;
            bool flag = PathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, allowUnderground, false, 32f, out startPosA, out startPosB, out num, out num2);
            PathUnit.Position position;
            PathUnit.Position position2;
            float num3;
            float num4;
            if (PathManager.FindPathPosition(startPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, allowUnderground, false, 32f, out position, out position2, out num3, out num4))
            {
                if (!flag || (num3 < num && (Mathf.Abs(startPos.x) > 4800f || Mathf.Abs(startPos.z) > 4800f)))
                {
                    startPosA = position;
                    startPosB = position2;
                    num = num3;
                    num2 = num4;
                }
                flag = true;
            }
            PathUnit.Position endPosA;
            PathUnit.Position endPosB;
            float num5;
            float num6;
            bool flag2 = PathManager.FindPathPosition(endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, undergroundTarget, false, 32f, out endPosA, out endPosB, out num5, out num6);
            PathUnit.Position position3;
            PathUnit.Position position4;
            float num7;
            float num8;
            if (PathManager.FindPathPosition(endPos, ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane, undergroundTarget, false, 32f, out position3, out position4, out num7, out num8))
            {
                if (!flag2 || (num7 < num5 && (Mathf.Abs(endPos.x) > 4800f || Mathf.Abs(endPos.z) > 4800f)))
                {
                    endPosA = position3;
                    endPosB = position4;
                    num5 = num7;
                    num6 = num8;
                }
                flag2 = true;
            }
            if (flag && flag2)
            {
                PathManager instance = Singleton<PathManager>.instance;
                if (!startBothWays || num < 10f)
                {
                    startPosB = default(PathUnit.Position);
                }
                if (!endBothWays || num5 < 10f)
                {
                    endPosB = default(PathUnit.Position);
                }
                NetInfo.LaneType laneTypes = NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle;
                VehicleInfo.VehicleType vehicleTypes = VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Train | VehicleInfo.VehicleType.Ship | VehicleInfo.VehicleType.Plane;
                uint path;
                if (instance.CreatePath(out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, default(PathUnit.Position), laneTypes, vehicleTypes, 20000f, this.IsHeavyVehicle(), this.IgnoreBlocked(vehicleID, ref vehicleData), false, false, false, false, this.CombustionEngine()))
                {
                    if (vehicleData.m_path != 0u)
                    {
                        instance.ReleasePath(vehicleData.m_path);
                    }
                    vehicleData.m_path = path;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            }
            return false;
        }

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }
        }

        //Can not do this because TMPE will patch CargoTruckAI.StartPathFind
        /*public void InitDelegate()
        {
            if (CustomStartPathFindDG != null)
            {
                return;
            }

            startPathFind = typeof(CargoTruckAI).GetMethod("StartPathFind", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) }, null);
            DebugLog.LogToFileOnly("InitDelegate in PassengerCarAI");
            var p0 = Expression.Parameter(typeof(CargoTruckAI), "CargoTruckAI");
            var p1 = Expression.Parameter(typeof(ushort), "vehicleID");
            var p2 = Expression.Parameter(typeof(Vehicle).MakeByRefType(), "vehicleData");
            var p3 = Expression.Parameter(typeof(Vector3), "startPos");
            var p4 = Expression.Parameter(typeof(Vector3), "endPos");
            var p5 = Expression.Parameter(typeof(bool), "startBothWays");
            var p6 = Expression.Parameter(typeof(bool), "endBothWays");
            var p7 = Expression.Parameter(typeof(bool), "undergroundTarget");
            var invokeExpression = Expression.Call(p0, startPathFind, new Expression[] { p1, p2, p3, p4, p5, p6, p7 });
            CustomStartPathFindDG = Expression.Lambda<CustomStartPathFind>(invokeExpression, p0, p1, p2, p3, p4, p5, p6, p7).Compile();
        }

        public static MethodInfo startPathFind;
        public delegate bool CustomStartPathFind(CargoTruckAI CargoTruckAI, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget);
        public static CustomStartPathFind CustomStartPathFindDG;*/
    }
}