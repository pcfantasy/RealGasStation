using ColossalFramework;
using ColossalFramework.Plugins;
using RealGasStation.CustomAI;
using RealGasStation.NewAI;
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
        public static readonly ushort CanNotStartGasTransferDistance = 800;
        public static bool _init = false;
        public static void StartGasTransfer(ushort vehicleID, ref Vehicle data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == (TransferManager.TransferReason)112)
            {
                if (data.Info.m_vehicleAI is CargoTruckAI)
                {
                    CargoTruckAI AI = (CargoTruckAI)data.Info.m_vehicleAI;
                    MainDataStore.preTranferReason[vehicleID] = data.m_transferType;
                    MainDataStore.TargetGasBuilding[vehicleID] = offer.Building;
                    data.m_transferType = 112;
                    if (offer.Building == data.m_targetBuilding)
                    {
                        DebugLog.LogToFileOnly("Error: Transfer fuel cargotruck do not need fuel");
                    }
                    if (data.m_flags.IsFlagSet(Vehicle.Flags.Created) && !data.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && !data.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && (data.m_cargoParent == 0) && data.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !data.m_flags.IsFlagSet(Vehicle.Flags.GoingBack) && data.m_targetBuilding != 0 && !data.m_flags.IsFlagSet(Vehicle.Flags.Parking))
                    {
                        if ((Vector3.Distance(data.GetLastFramePosition(), Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].m_position) > CanNotStartGasTransferDistance) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip) == 0) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane) == 0) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain) == 0))
                        {
                            AI.SetTarget(vehicleID, ref data, offer.Building);
                        }
                        //else
                        //{
                            //DebugLog.LogToFileOnly("Info: Cargo is near target position, do not go to gas station now.");
                        //}
                    }
                    //else
                    //{
                    //    DebugLog.LogToFileOnly("Warning: Not a valid CargoTruckAI");
                    //}
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI)
                {
                    PassengerCarAI AI = (PassengerCarAI)data.Info.m_vehicleAI;
                    MainDataStore.preTranferReason[vehicleID] = data.m_transferType;
                    if (data.m_targetBuilding == 0)
                    {
                        MainDataStore.TargetGasBuilding[vehicleID] = offer.Building;
                        data.m_transferType = 112;
                        if (data.m_flags.IsFlagSet(Vehicle.Flags.Created) && !data.m_flags.IsFlagSet(Vehicle.Flags.Deleted) && !data.m_flags.IsFlagSet(Vehicle.Flags.Arriving) && (data.m_cargoParent == 0) && data.m_flags.IsFlagSet(Vehicle.Flags.Spawned) && !data.m_flags.IsFlagSet(Vehicle.Flags.GoingBack) && !data.m_flags.IsFlagSet(Vehicle.Flags.Parking))
                        {
                            ushort citizen = CustomCarAI.GetDriverInstance(vehicleID, ref data);
                            if (Singleton<CitizenManager>.instance.m_instances.m_buffer[citizen].m_targetBuilding != 0)
                            {
                                if ((Vector3.Distance(data.GetLastFramePosition(), Singleton<BuildingManager>.instance.m_buildings.m_buffer[Singleton<CitizenManager>.instance.m_instances.m_buffer[citizen].m_targetBuilding].m_position) > CanNotStartGasTransferDistance) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip) == 0) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane) == 0) && (FindCargoStation(data.GetLastFramePosition(), ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain) == 0))
                                {
                                    AI.SetTarget(vehicleID, ref data, offer.Building);
                                }
                                //else
                                //{
                                    //DebugLog.LogToFileOnly("Info: PassengerCar is near target position, do not go to gas station now.");
                                //}
                            }
                            //else
                            //{
                            //    DebugLog.LogToFileOnly("Warning: No targetBuilding for citizen");
                            //}
                        }
                        //else
                        //{
                        //    DebugLog.LogToFileOnly("Warning: Not a valid PassengerCarAI");
                        //}
                    }
                    //else
                    //{
                    //    DebugLog.LogToFileOnly("Error: PassengerCarAI should not have targetBuilding");
                    //}
                }
            }
        }

        private static ushort FindCargoStation(Vector3 position, ItemClass.Service service, ItemClass.SubService subservice = ItemClass.SubService.None)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (subservice != ItemClass.SubService.PublicTransportPlane)
            {
                subservice = ItemClass.SubService.None;
            }
            ushort num = instance.FindBuilding(position, CanNotStartGasTransferDistance, service, subservice, Building.Flags.None, Building.Flags.None);
            int num2 = 0;
            while (num != 0)
            {
                ushort parentBuilding = instance.m_buildings.m_buffer[(int)num].m_parentBuilding;
                BuildingInfo info = instance.m_buildings.m_buffer[(int)num].Info;
                if (info.m_buildingAI is CargoStationAI || info.m_buildingAI is OutsideConnectionAI || parentBuilding == 0)
                {
                    return num;
                }
                num = parentBuilding;
                if (++num2 > 49152)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
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
                if (GasStationAI.IsGasBuilding(offerIn.Building))
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

        public static void CustomSimulationStepImpl()
        {
            int frameIndex = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 255u);
            if (frameIndex == 117)
            {
                //fuel demand
                MatchOffers((TransferReason)112);
            }
        }

        // TransferManager
        private static void Init()
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
                CODebugBase<LogChannel>.Error(LogChannel.Core, "No instance of TransferManager found!");
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "No instance of TransferManager found!");
                return;
            }
            m_incomingCount = incomingCount.GetValue(inst) as ushort[];
            m_incomingOffers = incomingOffers.GetValue(inst) as TransferManager.TransferOffer[];
            m_incomingAmount = incomingAmount.GetValue(inst) as int[];
            m_outgoingCount = outgoingCount.GetValue(inst) as ushort[];
            m_outgoingOffers = outgoingOffers.GetValue(inst) as TransferManager.TransferOffer[];
            m_outgoingAmount = outgoingAmount.GetValue(inst) as int[];
        }

        private static TransferManager.TransferOffer[] m_outgoingOffers;
        private static TransferManager.TransferOffer[] m_incomingOffers;
        private static ushort[] m_outgoingCount;
        private static ushort[] m_incomingCount;
        private static int[] m_outgoingAmount;
        private static int[] m_incomingAmount;

        private static void MatchOffers(TransferReason material)
        {
            if (!_init)
            {
                Init();
                _init = true;
            }

            if (material != TransferReason.None)
            {
                float distanceMultiplier = 1E-07f;
                float maxDistance = (distanceMultiplier == 0f) ? 0f : (0.01f / distanceMultiplier);
                for (int priority = 7; priority >= 0; priority--)
                {
                    int offerIdex = (int)material * 8 + priority;
                    int incomingCount = m_incomingCount[offerIdex];
                    int outgoingCount = m_outgoingCount[offerIdex];
                    int incomingIdex = 0;
                    int outgoingIdex = 0;
                    int oldPriority = priority;
                    // NON-STOCK CODE START
                    //In Real Gas Station Mod, we use outgoing first mode.
                    byte matchOffersMode = 1;
                    bool isLoopValid = false;
                    if (matchOffersMode == 2)
                    {
                        isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                    }
                    else if (matchOffersMode == 1)
                    {
                        isLoopValid = (outgoingIdex < outgoingCount);
                    }
                    else if (matchOffersMode == 0)
                    {
                        isLoopValid = (incomingIdex < incomingCount);
                    }

                    // NON-STOCK CODE END
                    while (isLoopValid)
                    {
                        //use incomingOffer to match outgoingOffer
                        if (incomingIdex < incomingCount && (matchOffersMode != 1))
                        {
                            TransferOffer incomingOffer = m_incomingOffers[offerIdex * 256 + incomingIdex];
                            // NON-STOCK CODE START
                            Vector3 incomingPositionNew = Vector3.zero;
                            bool canUseNewMatchOffers = true;
                            if (canUseNewMatchOffers)
                            {
                                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOffer.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
                                {
                                    incomingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOffer.Building].m_position;
                                }
                                else
                                {
                                    incomingPositionNew = incomingOffer.Position;
                                }
                            }
                            // NON-STOCK CODE END
                            Vector3 incomingPosition = incomingOffer.Position;
                            int incomingOfferAmount = incomingOffer.Amount;
                            do
                            {
                                int incomingPriority = Mathf.Max(0, 2 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                if (canUseNewMatchOffers)
                                {
                                    priority = 7;
                                    incomingPriority = 0;
                                }
                                else
                                {
                                    priority = oldPriority;
                                    incomingPriority = Mathf.Max(0, 2 - priority);
                                }
                                // NON-STOCK CODE END
                                int incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validOutgoingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int outgoingIdexInsideIncoming = outgoingIdex;
                                for (int incomingPriorityInside = priority; incomingPriorityInside >= incomingPriority; incomingPriorityInside--)
                                {
                                    int outgoingIdexWithPriority = (int)material * 8 + incomingPriorityInside;
                                    int outgoingCountWithPriority = m_outgoingCount[outgoingIdexWithPriority];
                                    //To let incomingPriorityInsideFloat!=0
                                    float incomingPriorityInsideFloat = (float)incomingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    //UseNewMatchOffers to find the shortest transfer building
                                    if ((distanceOffsetPre >= incomingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    //Find the nearest offer to match in every priority.
                                    for (int i = outgoingIdexInsideIncoming; i < outgoingCountWithPriority; i++)
                                    {
                                        TransferOffer outgoingOfferPre = m_outgoingOffers[outgoingIdexWithPriority * 256 + i];
                                        if (incomingOffer.m_object != outgoingOfferPre.m_object && (!outgoingOfferPre.Exclude || incomingPriorityInside >= incomingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(outgoingOfferPre.Position - incomingPosition);
                                            // NON-STOCK CODE START
                                            Vector3 outgoingPositionNew = Vector3.zero;
                                            float incomingOutgoingDistanceNew = 0;
                                            if (canUseNewMatchOffers)
                                            {
                                                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOfferPre.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
                                                {
                                                    outgoingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOfferPre.Building].m_position;
                                                }
                                                else
                                                {
                                                    outgoingPositionNew = outgoingOfferPre.Position;
                                                }
                                                incomingOutgoingDistanceNew = Vector3.SqrMagnitude(outgoingPositionNew - incomingPositionNew);
                                                if ((incomingOutgoingDistanceNew < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    validPriority = incomingPriorityInside;
                                                    validOutgoingIdex = i;
                                                    currentShortestDistance = incomingOutgoingDistanceNew;
                                                }
                                            }
                                            // NON-STOCK CODE END
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (incomingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (incomingPriorityInsideFloat - incomingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
                                            {
                                                validPriority = incomingPriorityInside;
                                                validOutgoingIdex = i;
                                                distanceOffsetPre = distanceOffset;
                                                if ((incomingOutgoingDistance < maxDistance))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    outgoingIdexInsideIncoming = 0;
                                }
                                // NON-STOCK CODE START
                                if (canUseNewMatchOffers)
                                {
                                    priority = oldPriority;
                                }
                                // NON-STOCK CODE END
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get outgoingOffer
                                int matchedOutgoingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer outgoingOffer = m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex];
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
                                    int outgoingCountPost = m_outgoingCount[matchedOutgoingOfferIdex] - 1;
                                    m_outgoingCount[matchedOutgoingOfferIdex] = (ushort)outgoingCountPost;
                                    m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = m_outgoingOffers[matchedOutgoingOfferIdex * 256 + outgoingCountPost];
                                    if (matchedOutgoingOfferIdex == offerIdex)
                                    {
                                        outgoingCount = outgoingCountPost;
                                    }
                                }
                                else
                                {
                                    outgoingOffer.Amount = outgoingOfferAmount;
                                    m_outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = outgoingOffer;
                                }
                                incomingOffer.Amount = incomingOfferAmount;
                            }
                            while (incomingOfferAmount != 0);
                            //matched incomingOffer is empty now
                            if (incomingOfferAmount == 0)
                            {
                                incomingCount--;
                                m_incomingCount[offerIdex] = (ushort)incomingCount;
                                m_incomingOffers[offerIdex * 256 + incomingIdex] = m_incomingOffers[offerIdex * 256 + incomingCount];
                            }
                            else
                            {
                                incomingOffer.Amount = incomingOfferAmount;
                                m_incomingOffers[offerIdex * 256 + incomingIdex] = incomingOffer;
                                incomingIdex++;
                            }
                        }
                        //For RealConstruction, We only satisify incoming building
                        //use outgoingOffer to match incomingOffer
                        if (outgoingIdex < outgoingCount && (matchOffersMode != 0))
                        {
                            TransferOffer outgoingOffer = m_outgoingOffers[offerIdex * 256 + outgoingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = true;
                            Vector3 outgoingPositionNew = Vector3.zero;
                            if (canUseNewMatchOffers)
                            {
                                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOffer.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
                                {
                                    outgoingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOffer.Building].m_position;
                                }
                                else
                                {
                                    outgoingPositionNew = outgoingOffer.Position;
                                }
                            }
                            // NON-STOCK CODE END
                            Vector3 outgoingPosition = outgoingOffer.Position;
                            int outgoingOfferAmount = outgoingOffer.Amount;
                            do
                            {
                                int outgoingPriority = Mathf.Max(0, 2 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                if (canUseNewMatchOffers)
                                {
                                    priority = 7;
                                    outgoingPriority = 0;
                                }
                                else
                                {
                                    priority = oldPriority;
                                    outgoingPriority = Mathf.Max(0, 2 - priority);
                                }
                                // NON-STOCK CODE END
                                int outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validIncomingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int incomingIdexInsideOutgoing = incomingIdex;
                                for (int outgoingPriorityInside = priority; outgoingPriorityInside >= outgoingPriority; outgoingPriorityInside--)
                                {
                                    int incomingIdexWithPriority = (int)material * 8 + outgoingPriorityInside;
                                    int incomingCountWithPriority = m_incomingCount[incomingIdexWithPriority];
                                    //To let outgoingPriorityInsideFloat!=0
                                    float outgoingPriorityInsideFloat = (float)outgoingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    if ((distanceOffsetPre >= outgoingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    for (int j = incomingIdexInsideOutgoing; j < incomingCountWithPriority; j++)
                                    {
                                        TransferOffer incomingOfferPre = m_incomingOffers[incomingIdexWithPriority * 256 + j];
                                        if (outgoingOffer.m_object != incomingOfferPre.m_object && (!incomingOfferPre.Exclude || outgoingPriorityInside >= outgoingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(incomingOfferPre.Position - outgoingPosition);
                                            // NON-STOCK CODE START
                                            Vector3 incomingPositionNew = Vector3.zero;
                                            float incomingOutgoingDistanceNew = 0;
                                            if (canUseNewMatchOffers)
                                            {
                                                if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOfferPre.Building].m_flags.IsFlagSet(Building.Flags.Untouchable))
                                                {
                                                    incomingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOfferPre.Building].m_position;
                                                }
                                                else
                                                {
                                                    incomingPositionNew = incomingOfferPre.Position;
                                                }
                                                incomingOutgoingDistanceNew = Vector3.SqrMagnitude(outgoingPositionNew - incomingPositionNew);
                                                if ((incomingOutgoingDistanceNew < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    validPriority = outgoingPriorityInside;
                                                    validIncomingIdex = j;
                                                    currentShortestDistance = incomingOutgoingDistanceNew;
                                                }
                                            }
                                            // NON-STOCK CODE END
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (outgoingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (outgoingPriorityInsideFloat - outgoingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
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
                                // NON-STOCK CODE START
                                if (canUseNewMatchOffers)
                                {
                                    priority = oldPriority;
                                }
                                // NON-STOCK CODE END
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get incomingOffer
                                int matchedIncomingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer incomingOffers = m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex];
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
                                    int incomingCountPost = m_incomingCount[matchedIncomingOfferIdex] - 1;
                                    m_incomingCount[matchedIncomingOfferIdex] = (ushort)incomingCountPost;
                                    m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = m_incomingOffers[matchedIncomingOfferIdex * 256 + incomingCountPost];
                                    if (matchedIncomingOfferIdex == offerIdex)
                                    {
                                        incomingCount = incomingCountPost;
                                    }
                                }
                                else
                                {
                                    incomingOffers.Amount = incomingOffersAmount;
                                    m_incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = incomingOffers;
                                }
                                outgoingOffer.Amount = outgoingOfferAmount;
                            }
                            while (outgoingOfferAmount != 0);
                            //matched outgoingOffer is empty now
                            if (outgoingOfferAmount == 0)
                            {
                                outgoingCount--;
                                m_outgoingCount[offerIdex] = (ushort)outgoingCount;
                                m_outgoingOffers[offerIdex * 256 + outgoingIdex] = m_outgoingOffers[offerIdex * 256 + outgoingCount];
                            }
                            else
                            {
                                outgoingOffer.Amount = outgoingOfferAmount;
                                m_outgoingOffers[offerIdex * 256 + outgoingIdex] = outgoingOffer;
                                outgoingIdex++;
                            }
                        }

                        // NON-STOCK CODE START
                        if (matchOffersMode == 2)
                        {
                            isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                        }
                        else if (matchOffersMode == 1)
                        {
                            isLoopValid = (outgoingIdex < outgoingCount);
                        }
                        else if (matchOffersMode == 0)
                        {
                            isLoopValid = (incomingIdex < incomingCount);
                        }
                        // NON-STOCK CODE END
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    int num40 = (int)material * 8 + k;
                    m_incomingCount[num40] = 0;
                    m_outgoingCount[num40] = 0;
                }
                m_incomingAmount[(int)material] = 0;
                m_outgoingAmount[(int)material] = 0;
            }
        }
    }
}
