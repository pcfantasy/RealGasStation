using ColossalFramework;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealGasStation.CustomManager
{
    public class CustomTransferManager : TransferManager
    {
        public static void StartGasTransfer(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == (TransferManager.TransferReason)112)
            {
                if (data.Info.m_vehicleAI is CargoTruckAI)
                {
                    CargoTruckAI AI = data.Info.m_vehicleAI as CargoTruckAI;
                    MainDataStore.preTranferReason[vehicleID] = data.m_transferType;
                    MainDataStore.preTargetBuilding[vehicleID] = data.m_targetBuilding;
                    data.m_transferType = 112;
                    if (offer.Building == data.m_targetBuilding)
                    {
                        DebugLog.LogToFileOnly("Error: Transfer fuel cargotruck do not need fuel");
                    }
                    AI.SetTarget(vehicleID, ref data, offer.Building);
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    PassengerCarAI AI = data.Info.m_vehicleAI as PassengerCarAI;
                    MainDataStore.preTranferReason[vehicleID] = data.m_transferType;
                    //ushort driverInstance = GetDriverInstance(vehicleID, ref data);
                    if (data.m_targetBuilding == 0)
                    {
                        MainDataStore.preTargetBuilding[vehicleID] = data.m_targetBuilding;
                        data.m_transferType = 112;
                        AI.SetTarget(vehicleID, ref data, offer.Building);
                    }
                    else
                    {
                        DebugLog.LogToFileOnly("Error: PassengerCarAI should not have targetBuilding");
                    }
                }
            }
        }


        /*public static ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                    if (citizen != 0)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
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
        }*/


        public static void StartTransfer(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            bool active = offerIn.Active;
            bool active2 = offerOut.Active;
            if (active && offerIn.Vehicle != 0)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Vehicle");
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                Array16<Vehicle> vehicles2 = Singleton<VehicleManager>.instance.m_vehicles;
                ushort vehicle2 = offerOut.Vehicle;
                VehicleInfo info2 = vehicles2.m_buffer[(int)vehicle2].Info;
                offerIn.Amount = delta;
                if (RealGasStationThreading.IsGasBuilding(offerIn.Building))
                {
                    Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
                    ushort building = offerIn.Building;
                    BuildingInfo info3 = buildings.m_buffer[(int)building].Info;
                    StartGasTransfer(vehicle2, ref vehicles2.m_buffer[(int)vehicle2], material, offerIn);
                }
                else
                {
                    DebugLog.LogToFileOnly("Error: active2 && offerOut.Vehicle");
                }
            }
            else if (active && offerIn.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Citizen");
            }
            else if (active2 && offerOut.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: active2 && offerOut.Citizen");
            }
            else if (active2 && offerOut.Building != 0)
            {
                DebugLog.LogToFileOnly("Error: active2 && offerOut.Building");
            }
            else if (active && offerIn.Building != 0)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Building");
            }
        }

        public static void Init()
        {
            DebugLog.LogToFileOnly("Init fake transfer manager");
            try
            {
                var inst = Singleton<TransferManager>.instance;
                var incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
                var incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
                var incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                if (inst == null)
                {
                    DebugLog.LogToFileOnly("No instance of TransferManager found!");
                    return;
                }
                _incomingCount = incomingCount.GetValue(inst) as ushort[];
                _incomingOffers = incomingOffers.GetValue(inst) as TransferManager.TransferOffer[];
                _incomingAmount = incomingAmount.GetValue(inst) as int[];
                _outgoingCount = outgoingCount.GetValue(inst) as ushort[];
                _outgoingOffers = outgoingOffers.GetValue(inst) as TransferManager.TransferOffer[];
                _outgoingAmount = outgoingAmount.GetValue(inst) as int[];
                if (_incomingCount == null || _incomingOffers == null || _incomingAmount == null || _outgoingCount == null || _outgoingOffers == null || _outgoingAmount == null)
                {
                    DebugLog.LogToFileOnly("Arrays are null");
                }
            }
            catch (Exception ex)
            {
                DebugLog.LogToFileOnly("Exception: " + ex.Message);
            }
        }

        private static TransferManager.TransferOffer[] _incomingOffers;
        private static ushort[] _incomingCount;
        private static int[] _incomingAmount;
        private static TransferManager.TransferOffer[] _outgoingOffers;
        private static ushort[] _outgoingCount;
        private static int[] _outgoingAmount;
        private static bool _init;


        public static void CustomSimulationStepImpl()
        {
            if (!_init)
            {
                _init = true;
                Init();
            }

            int frameIndex = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 255u);
            if ((frameIndex == 6) || (frameIndex == 58) || (frameIndex == 117) || (frameIndex == 210))
            {
                //fuel demand
                MatchOffers((TransferReason)112);
            }
        }

        // TransferManager
        public static void MatchOffers(TransferManager.TransferReason material)
        {
            if (material == TransferManager.TransferReason.None)
            {
                return;
            }
            float distanceMultiplier = 1E-07f;
            float num;
            if (distanceMultiplier != 0f)
            {
                num = 0.01f / distanceMultiplier;
            }
            else
            {
                num = 0f;
            }
            for (int i = 7; i >= 0; i--)
            {
                int num2 = (int)((int)material * 8 + i);
                int num3 = (int)_incomingCount[num2];
                int num4 = (int)_outgoingCount[num2];
                int num5 = 0;
                int num6 = 0;
                while (num5 < num3 || num6 < num4)
                {
                    if (num5 < num3)
                    {
                        TransferManager.TransferOffer transferOffer = _incomingOffers[num2 * 256 + num5];
                        Vector3 position = transferOffer.Position;
                        int num7 = transferOffer.Amount;
                        do
                        {
                            int num8 = Mathf.Max(0, 2 - i);
                            int num9 = (!transferOffer.Exclude) ? num8 : Mathf.Max(0, 3 - i);
                            int num10 = -1;
                            int num11 = -1;
                            float num12 = -1f;
                            int num13 = num6;
                            for (int j = i; j >= num8; j--)
                            {
                                int num14 = (int)((int)material * 8 + j);
                                int num15 = (int)_outgoingCount[num14];
                                float num16 = (float)j + 0.1f;
                                if (num12 >= num16)
                                {
                                    break;
                                }
                                for (int k = num13; k < num15; k++)
                                {
                                    TransferManager.TransferOffer transferOffer2 = _outgoingOffers[num14 * 256 + k];
                                    if (transferOffer.m_object != transferOffer2.m_object && (!transferOffer2.Exclude || j >= num9))
                                    {
                                        float num17 = Vector3.SqrMagnitude(transferOffer2.Position - position);
                                        float num18;
                                        if (distanceMultiplier < 0f)
                                        {
                                            num18 = num16 - num16 / (1f - num17 * distanceMultiplier);
                                        }
                                        else
                                        {
                                            num18 = num16 / (1f + num17 * distanceMultiplier);
                                        }
                                        if (num18 > num12)
                                        {
                                            num10 = j;
                                            num11 = k;
                                            num12 = num18;
                                            if (num17 < num)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                num13 = 0;
                            }
                            if (num10 == -1)
                            {
                                break;
                            }
                            int num19 = (int)((int)material * 8 + num10);
                            TransferManager.TransferOffer transferOffer3 = _outgoingOffers[num19 * 256 + num11];
                            int num20 = transferOffer3.Amount;
                            int num21 = Mathf.Min(num7, num20);
                            if (num21 != 0)
                            {
                                StartTransfer(material, transferOffer3, transferOffer, num21);
                            }
                            num7 -= num21;
                            num20 -= num21;
                            if (num20 == 0)
                            {
                                int num22 = (int)(_outgoingCount[num19] - 1);
                                _outgoingCount[num19] = (ushort)num22;
                                _outgoingOffers[num19 * 256 + num11] = _outgoingOffers[num19 * 256 + num22];
                                if (num19 == num2)
                                {
                                    num4 = num22;
                                }
                            }
                            else
                            {
                                transferOffer3.Amount = num20;
                                _outgoingOffers[num19 * 256 + num11] = transferOffer3;
                            }
                            transferOffer.Amount = num7;
                        }
                        while (num7 != 0);
                        IL_2E8:
                        if (num7 == 0)
                        {
                            num3--;
                            _incomingCount[num2] = (ushort)num3;
                            _incomingOffers[num2 * 256 + num5] = _incomingOffers[num2 * 256 + num3];
                            goto IL_364;
                        }
                        transferOffer.Amount = num7;
                        _incomingOffers[num2 * 256 + num5] = transferOffer;
                        num5++;
                        goto IL_364;
                        goto IL_2E8;
                    }
                    IL_364:
                    if (num6 < num4)
                    {
                        TransferManager.TransferOffer transferOffer4 = _outgoingOffers[num2 * 256 + num6];
                        Vector3 position2 = transferOffer4.Position;
                        int num23 = transferOffer4.Amount;
                        do
                        {
                            int num24 = Mathf.Max(0, 2 - i);
                            int num25 = (!transferOffer4.Exclude) ? num24 : Mathf.Max(0, 3 - i);
                            int num26 = -1;
                            int num27 = -1;
                            float num28 = -1f;
                            int num29 = num5;
                            for (int l = i; l >= num24; l--)
                            {
                                int num30 = (int)((int)material * 8 + l);
                                int num31 = (int)_incomingCount[num30];
                                float num32 = (float)l + 0.1f;
                                if (num28 >= num32)
                                {
                                    break;
                                }
                                for (int m = num29; m < num31; m++)
                                {
                                    TransferManager.TransferOffer transferOffer5 = _incomingOffers[num30 * 256 + m];
                                    if (transferOffer4.m_object != transferOffer5.m_object && (!transferOffer5.Exclude || l >= num25))
                                    {
                                        float num33 = Vector3.SqrMagnitude(transferOffer5.Position - position2);
                                        float num34;
                                        if (distanceMultiplier < 0f)
                                        {
                                            num34 = num32 - num32 / (1f - num33 * distanceMultiplier);
                                        }
                                        else
                                        {
                                            num34 = num32 / (1f + num33 * distanceMultiplier);
                                        }
                                        if (num34 > num28)
                                        {
                                            num26 = l;
                                            num27 = m;
                                            num28 = num34;
                                            if (num33 < num)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                num29 = 0;
                            }
                            if (num26 == -1)
                            {
                                break;
                            }
                            int num35 = (int)((int)material * 8 + num26);
                            TransferManager.TransferOffer transferOffer6 = _incomingOffers[num35 * 256 + num27];
                            int num36 = transferOffer6.Amount;
                            int num37 = Mathf.Min(num23, num36);
                            if (num37 != 0)
                            {
                                StartTransfer(material, transferOffer4, transferOffer6, num37);
                            }
                            num23 -= num37;
                            num36 -= num37;
                            if (num36 == 0)
                            {
                                int num38 = (int)(_incomingCount[num35] - 1);
                                _incomingCount[num35] = (ushort)num38;
                                _incomingOffers[num35 * 256 + num27] = _incomingOffers[num35 * 256 + num38];
                                if (num35 == num2)
                                {
                                    num3 = num38;
                                }
                            }
                            else
                            {
                                transferOffer6.Amount = num36;
                                _incomingOffers[num35 * 256 + num27] = transferOffer6;
                            }
                            transferOffer4.Amount = num23;
                        }
                        while (num23 != 0);
                        IL_5EF:
                        if (num23 == 0)
                        {
                            num4--;
                            _outgoingCount[num2] = (ushort)num4;
                            _outgoingOffers[num2 * 256 + num6] = _outgoingOffers[num2 * 256 + num4];
                            continue;
                        }
                        transferOffer4.Amount = num23;
                        _outgoingOffers[num2 * 256 + num6] = transferOffer4;
                        num6++;
                        continue;
                        goto IL_5EF;
                    }
                }
            }
            for (int n = 0; n < 8; n++)
            {
                int num39 = (int)((int)material * 8 + n);
                _incomingCount[num39] = 0;
                _outgoingCount[num39] = 0;
            }
            _incomingAmount[(int)material] = 0;
            _outgoingAmount[(int)material] = 0;
        }

    }
}
