using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomCargoTruckAI : CargoTruckAI
    {
        public static bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData)
        {
            var m_info = vehicleData.Info;
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

                    InitDelegate();
                    return CargoTruckAIStartPathFindDG(inst, vehicleID, ref vehicleData, vehicleData.m_targetPos3, target, true, true, false);
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

                InitDelegate();
                return CargoTruckAIStartPathFindDG(inst, vehicleID, ref vehicleData, vehicleData.m_targetPos3, target2, true, true, false);
            }
            return false;
        }

        public static void InitDelegate()
        {
            if (CargoTruckAIStartPathFindDG != null)
            {
                return;
            }

            CargoTruckAIStartPathFindDG = FastDelegateFactory.Create<CargoTruckAIStartPathFind>(typeof(CargoTruckAI), "StartPathFind", instanceMethod: true);
        }

        public delegate bool CargoTruckAIStartPathFind(CargoTruckAI CargoTruckAI, ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget);
        public static CargoTruckAIStartPathFind CargoTruckAIStartPathFindDG;
    }
}
