using ColossalFramework;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.CustomAI
{
    public class CustomCommonBuildingAI : BuildingAI
    {
        public static void CustomReleaseBuilding(ushort buildingID)
        {
            MainDataStore.petrolBuffer[buildingID] = 0;
            MainDataStore.tempVehicleForFuelCount[buildingID] = 0;
            MainDataStore.finalVehicleForFuelCount[buildingID] = 0;

            if (!Loader.realCityRunning && !Loader.realConstructionRunning)
            {
                //RealCity and RealConstruction Will also do this
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Building = buildingID;
                Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)110, offer);
                Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)111, offer);
                Singleton<TransferManager>.instance.RemoveOutgoingOffer((TransferManager.TransferReason)112, offer);
            }
        }

    }
}