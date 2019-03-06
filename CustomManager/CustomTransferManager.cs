using ColossalFramework;
using RealGasStation.CustomAI;
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
                    MainDataStore.TargetGasBuilding[vehicleID] = offer.Building;
                    data.m_transferType = 112;
                    if (offer.Building == data.m_targetBuilding)
                    {
                        DebugLog.LogToFileOnly("Error: Transfer fuel cargotruck do not need fuel");
                    }
                    if (data.m_flags.IsFlagSet(Vehicle.Flags.Created) && !data.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && !data.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && (data.m_cargoParent == 0) && data.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !data.m_flags.IsFlagSet(Vehicle.Flags.GoingBack) && data.m_targetBuilding != 0)
                    {
                        AI.SetTarget(vehicleID, ref data, offer.Building);
                    }
                    else
                    {
                        DebugLog.LogToFileOnly("Warning: Not a valid CargoTruckAI");
                    }
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    PassengerCarAI AI = data.Info.m_vehicleAI as PassengerCarAI;
                    MainDataStore.preTranferReason[vehicleID] = data.m_transferType;
                    if (data.m_targetBuilding == 0)
                    {
                        MainDataStore.TargetGasBuilding[vehicleID] = offer.Building;
                        data.m_transferType = 112;
                        if (data.m_flags.IsFlagSet(Vehicle.Flags.Created) && !data.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && !data.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && (data.m_cargoParent == 0) && data.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !data.m_flags.IsFlagSet(Vehicle.Flags.GoingBack))
                        {
                            AI.SetTarget(vehicleID, ref data, offer.Building);
                        }
                        else
                        {
                            DebugLog.LogToFileOnly("Warning: Not a valid PassengerCarAI");
                        }
                    }
                    else
                    {
                        DebugLog.LogToFileOnly("Error: PassengerCarAI should not have targetBuilding");
                    }
                }
            }
        }

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
            if (frameIndex == 117)
            {
                //fuel demand
                MatchOffers((TransferReason)112);
            }
        }

        // TransferManager
        private static void MatchOffers(TransferReason material)
        {
            if (material != TransferReason.None)
            {
                float distanceMultiplier = 1E-07f;
                float maxDistance = (distanceMultiplier == 0f) ? 0f : (0.01f / distanceMultiplier);
                for (int priority = 7; priority >= 0; priority--)
                {
                    int offerIdex = (int)material * 8 + priority;
                    int incomingCount = _incomingCount[offerIdex];
                    int outgoingCount = _outgoingCount[offerIdex];
                    int incomingIdex = 0;
                    int outgoingIdex = 0;
                    while (incomingIdex < incomingCount || outgoingIdex < outgoingCount)
                    {
                        //use incomingOffer to match outgoingOffer
                        if (incomingIdex < incomingCount)
                        {
                            TransferOffer incomingOffer = _incomingOffers[offerIdex * 256 + incomingIdex];
                            Vector3 incomingPosition = incomingOffer.Position;
                            int incomingOfferAmount = incomingOffer.Amount;
                            do
                            {
                                int incomingPriority = Mathf.Max(0, 2 - priority);
                                int incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validOutgoingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int outgoingIdexInsideIncoming = outgoingIdex;
                                for (int incomingPriorityInside = priority; incomingPriorityInside >= incomingPriority; incomingPriorityInside--)
                                {
                                    int outgoingIdexWithPriority = (int)material * 8 + incomingPriorityInside;
                                    int outgoingCountWithPriority = _outgoingCount[outgoingIdexWithPriority];
                                    //To let incomingPriorityInsideFloat!=0
                                    float incomingPriorityInsideFloat = (float)incomingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    if (distanceOffsetPre >= incomingPriorityInsideFloat)
                                    {
                                        break;
                                    }
                                    //Find the nearest offer to match in every priority.
                                    for (int i = outgoingIdexInsideIncoming; i < outgoingCountWithPriority; i++)
                                    {
                                        TransferOffer outgoingOfferPre = _outgoingOffers[outgoingIdexWithPriority * 256 + i];
                                        if (incomingOffer.m_object != outgoingOfferPre.m_object && (!outgoingOfferPre.Exclude || incomingPriorityInside >= incomingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(outgoingOfferPre.Position - incomingPosition);
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (incomingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (incomingPriorityInsideFloat - incomingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if (distanceOffset > distanceOffsetPre)
                                            {
                                                validPriority = incomingPriorityInside;
                                                validOutgoingIdex = i;
                                                distanceOffsetPre = distanceOffset;
                                                if (incomingOutgoingDistance < maxDistance)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    outgoingIdexInsideIncoming = 0;
                                }
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get outgoingOffer
                                int matchedOutgoingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer outgoingOffer = _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex];
                                int outgoingOfferAmount = outgoingOffer.Amount;
                                int matchedOfferAmount = Mathf.Min(incomingOfferAmount, outgoingOfferAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    StartTransfer(material, outgoingOffer, incomingOffer, matchedOfferAmount);
                                }
                                incomingOfferAmount -= matchedOfferAmount;
                                outgoingOfferAmount -= matchedOfferAmount;
                                //matched outgoingOffer is empty now
                                if (outgoingOfferAmount == 0)
                                {
                                    int outgoingCountPost = _outgoingCount[matchedOutgoingOfferIdex] - 1;
                                    _outgoingCount[matchedOutgoingOfferIdex] = (ushort)outgoingCountPost;
                                    _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = _outgoingOffers[matchedOutgoingOfferIdex * 256 + outgoingCountPost];
                                    if (matchedOutgoingOfferIdex == offerIdex)
                                    {
                                        outgoingCount = outgoingCountPost;
                                    }
                                }
                                else
                                {
                                    outgoingOffer.Amount = outgoingOfferAmount;
                                    _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = outgoingOffer;
                                }
                                incomingOffer.Amount = incomingOfferAmount;
                            }
                            while (incomingOfferAmount != 0);
                            //matched incomingOffer is empty now
                            if (incomingOfferAmount == 0)
                            {
                                incomingCount--;
                                _incomingCount[offerIdex] = (ushort)incomingCount;
                                _incomingOffers[offerIdex * 256 + incomingIdex] = _incomingOffers[offerIdex * 256 + incomingCount];
                            }
                            else
                            {
                                incomingOffer.Amount = incomingOfferAmount;
                                _incomingOffers[offerIdex * 256 + incomingIdex] = incomingOffer;
                                incomingIdex++;
                            }
                        }
                        //use outgoingOffer to match incomingOffer
                        if (outgoingIdex < outgoingCount)
                        {
                            TransferOffer outgoingOffer = _outgoingOffers[offerIdex * 256 + outgoingIdex];
                            Vector3 outgoingOfferPosition = outgoingOffer.Position;
                            int outgoingOfferAmount = outgoingOffer.Amount;
                            do
                            {
                                int outgoingPriority = Mathf.Max(0, 2 - priority);
                                int outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validIncomingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int incomingIdexInsideOutgoing = incomingIdex;
                                for (int outgoingPriorityInside = priority; outgoingPriorityInside >= outgoingPriority; outgoingPriorityInside--)
                                {
                                    int incomingIdexWithPriority = (int)material * 8 + outgoingPriorityInside;
                                    int incomingCountWithPriority = _incomingCount[incomingIdexWithPriority];
                                    //To let outgoingPriorityInsideFloat!=0
                                    float outgoingPriorityInsideFloat = (float)outgoingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    if (distanceOffsetPre >= outgoingPriorityInsideFloat)
                                    {
                                        break;
                                    }
                                    for (int j = incomingIdexInsideOutgoing; j < incomingCountWithPriority; j++)
                                    {
                                        TransferOffer incomingOfferPre = _incomingOffers[incomingIdexWithPriority * 256 + j];
                                        if (outgoingOffer.m_object != incomingOfferPre.m_object && (!incomingOfferPre.Exclude || outgoingPriorityInside >= outgoingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(incomingOfferPre.Position - outgoingOfferPosition);
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (outgoingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (outgoingPriorityInsideFloat - outgoingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if (distanceOffset > distanceOffsetPre)
                                            {
                                                validPriority = outgoingPriorityInside;
                                                validIncomingIdex = j;
                                                distanceOffsetPre = distanceOffset;
                                                if (incomingOutgoingDistance < maxDistance)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    incomingIdexInsideOutgoing = 0;
                                }
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get incomingOffer
                                int matchedIncomingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer incomingOffers = _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex];
                                int incomingOffersAmount = incomingOffers.Amount;
                                int matchedOfferAmount = Mathf.Min(outgoingOfferAmount, incomingOffersAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    StartTransfer(material, outgoingOffer, incomingOffers, matchedOfferAmount);
                                }
                                outgoingOfferAmount -= matchedOfferAmount;
                                incomingOffersAmount -= matchedOfferAmount;
                                //matched incomingOffer is empty now
                                if (incomingOffersAmount == 0)
                                {
                                    int incomingCountPost = _incomingCount[matchedIncomingOfferIdex] - 1;
                                    _incomingCount[matchedIncomingOfferIdex] = (ushort)incomingCountPost;
                                    _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = _incomingOffers[matchedIncomingOfferIdex * 256 + incomingCountPost];
                                    if (matchedIncomingOfferIdex == offerIdex)
                                    {
                                        incomingCount = incomingCountPost;
                                    }
                                }
                                else
                                {
                                    incomingOffers.Amount = incomingOffersAmount;
                                    _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = incomingOffers;
                                }
                                outgoingOffer.Amount = outgoingOfferAmount;
                            }
                            while (outgoingOfferAmount != 0);
                            //matched outgoingOffer is empty now
                            if (outgoingOfferAmount == 0)
                            {
                                outgoingCount--;
                                _outgoingCount[offerIdex] = (ushort)outgoingCount;
                                _outgoingOffers[offerIdex * 256 + outgoingIdex] = _outgoingOffers[offerIdex * 256 + outgoingCount];
                            }
                            else
                            {
                                outgoingOffer.Amount = outgoingOfferAmount;
                                _outgoingOffers[offerIdex * 256 + outgoingIdex] = outgoingOffer;
                                outgoingIdex++;
                            }
                        }
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    int num40 = (int)material * 8 + k;
                    _incomingCount[num40] = 0;
                    _outgoingCount[num40] = 0;
                }
                _incomingAmount[(int)material] = 0;
                _outgoingAmount[(int)material] = 0;
            }
        }

    }
}
