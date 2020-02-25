using ColossalFramework;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RealGasStation.NewAI
{
    public class GasStationAI
    {
        public static void ProcessGasBuildingIncoming(ushort buildingID, ref Building buildingData)
        {
            int num27 = 0;
            int num28 = 0;
            int num29 = 0;
            int value = 0;
            int num34 = 0;
            TransferManager.TransferReason incomingTransferReason = default(TransferManager.TransferReason);

            //Petrol
            incomingTransferReason = TransferManager.TransferReason.Petrol;
            num27 = 0;
            num28 = 0;
            num29 = 0;
            value = 0;
            num34 = 0;
            if (incomingTransferReason != TransferManager.TransferReason.None && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num27, ref num28, ref num29, ref value);
                buildingData.m_tempImport = (byte)Mathf.Clamp(value, (int)buildingData.m_tempImport, 255);
            }

            num34 = 50000 - MainDataStore.petrolBuffer[buildingID] - num29;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (num34 >= 0)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 7;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = (int)(num34 / 8000);
                    offer.Active = false;

                    if (offer.Amount > 0)
                    {
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                    }
                }
            }

            if ((MainDataStore.resourceCategory[buildingID] == 2) || (MainDataStore.resourceCategory[buildingID] == 0))
            {
                //Fuel
                incomingTransferReason = (TransferManager.TransferReason)126;
                num34 = MainDataStore.petrolBuffer[buildingID] - MainDataStore.finalVehicleForFuelCount[buildingID] * 400;
                if ((MainDataStore.resourceCategory[buildingID] == 0))
                    num34 >>= 1;
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    if (num34 >= 0)
                    {
                        System.Random rand = new System.Random();
                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                        offer.Priority = rand.Next(8);
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = (int)((num34 - 0) / 400);
                        offer.Active = false;

                        if ((int)(num34 / 400) > 0)
                        {
                            Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                        }
                    }
                }
            }

            if ((MainDataStore.resourceCategory[buildingID] == 1) || (MainDataStore.resourceCategory[buildingID] == 0))
            {
                //Fuel for Heavy
                incomingTransferReason = (TransferManager.TransferReason)127;
                num34 = MainDataStore.petrolBuffer[buildingID] - MainDataStore.finalVehicleForFuelCount[buildingID] * 400;
                if ((MainDataStore.resourceCategory[buildingID] == 0))
                    num34 >>= 1;
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    if (num34 >= 0)
                    {
                        System.Random rand = new System.Random();
                        TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                        offer.Priority = rand.Next(8);
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = (int)((num34 - 0) / 400);
                        offer.Active = false;

                        if ((int)(num34 / 400) > 0)
                        {
                            Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                        }
                    }
                }
            }
        }

        protected static void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[(int)num].m_transferType == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[(int)num].Info;
                    int a;
                    int num3;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[(int)num], out a, out num3);
                    cargo += Mathf.Min(a, num3);
                    capacity += num3;
                    count++;
                    if ((instance.m_vehicles.m_buffer[(int)num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != (Vehicle.Flags)0)
                    {
                        outside++;
                    }
                }
                num = instance.m_vehicles.m_buffer[(int)num].m_nextGuestVehicle;
                if (++num2 > Singleton<VehicleManager>.instance.m_vehicles.m_size)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static bool IsGasBuilding(ushort id)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            int num = instance.m_buildings.m_buffer[id].Info.m_buildingAI.GetConstructionCost();
            if (num == 508600)
            {
                return true;
            }

            return false;
        }
    }
}
