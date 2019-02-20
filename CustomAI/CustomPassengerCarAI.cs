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
            data.m_targetBuilding = 0;
            SetTarget(vehicleID, ref data, MainDataStore.preTargetBuilding[vehicleID]);
            MainDataStore.preTargetBuilding[vehicleID] = 0;
        }

        public bool CustomArriveAtDestination(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transferType == 112)
            {
                PassengerCarAIArriveAtTargetForRealGasStationPre(vehicleID, ref vehicleData);
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
            if (data.m_transferType != 112)
            {
                RemoveTarget(vehicleID, ref data);
            }

            data.m_targetBuilding = targetBuilding;
            if (data.m_transferType != 112 &&  targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].AddGuestVehicle(vehicleID, ref data);
            }

            if (data.m_transferType == 112)
            {                
                if (!CustomStartPathFind(vehicleID, ref data))
                {
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    data.m_targetBuilding = 0;
                    SetTarget(vehicleID, ref data, MainDataStore.preTargetBuilding[vehicleID]);
                    MainDataStore.preTargetBuilding[vehicleID] = 0;
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
                //Fuel demand vehicle will not add into GuestVehicle, so do not need to remove them
                if (data.m_transferType != 112)
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                }
                data.m_targetBuilding = 0;
            }
        }
    }
}