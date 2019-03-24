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
        public void PassengerCarAIArriveAtTargetForRealGasStationPre(ushort vehicleID, ref Vehicle data)
        {
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.petrolBuffer[data.m_targetBuilding] > 400)
            {
                MainDataStore.petrolBuffer[data.m_targetBuilding] -= 400;
            }
            SetTarget(vehicleID, ref data, 0);
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, 3000, ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level1);
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

        private bool CustomArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            //RealGasStation Mod related
            if (data.m_transferType == 112)
            {
                PassengerCarAIArriveAtTargetForRealGasStationPre(vehicleID, ref data);
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
            if (data.m_transferType != 112)
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

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }
        }
    }
}