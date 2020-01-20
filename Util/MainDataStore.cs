using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.Util
{
    public class MainDataStore
    {
        public static ushort[] TargetGasBuilding = new ushort[65536];
        public static byte[] tempVehicleForFuelCount = new byte[49152];
        public static byte[] finalVehicleForFuelCount = new byte[49152];
        public static byte[] preTranferReason = new byte[65536];
        public static ushort[] petrolBuffer = new ushort[49152];
        public static bool[] alreadyAskForFuel = new bool[65536];
        public static bool[] isBuildingReleased = new bool[49152];
        public static byte[] resourceCategory = new byte[49152];

        public static ushort lastVehicle = 0;
        public static ushort lastBuilding = 0;
        public static byte lastLanguage = 0;

        public static byte[] saveData = new byte[262144];
        public static byte[] saveDataForMoreVehicle = new byte[196608];

        public static void DataInit()
        {
            for (int i = 0; i < MainDataStore.TargetGasBuilding.Length; i++)
            {
                TargetGasBuilding[i] = 0;
                alreadyAskForFuel[i] = false;
                preTranferReason[i] = 0;
            }

            for (int i = 0; i < MainDataStore.petrolBuffer.Length; i++)
            {
                tempVehicleForFuelCount[i] = 0;
                finalVehicleForFuelCount[i] = 0;
                petrolBuffer[i] = 0;
                isBuildingReleased[i] = false;
            }

            for (int i = 0; i < MainDataStore.resourceCategory.Length; i++)
            {
                resourceCategory[i] = 0;
            }
        }

        public static void save()
        {
            int i = 0;
            ushort[] TargetGasBuildingLegacy = new ushort[16384];
            byte[] preTranferReasonLegacy = new byte[16384];
            bool[] alreadyAskForFuelLegacy = new bool[16384];
            //for legacy, other 49152 will save in other place
            for (int j = 0; j < 16384; j++)
            {
                TargetGasBuildingLegacy[j] = TargetGasBuilding[j];
                preTranferReasonLegacy[j] = preTranferReason[j];
                alreadyAskForFuelLegacy[j] = alreadyAskForFuel[j];
            }
            SaveAndRestore.save_ushorts(ref i, TargetGasBuildingLegacy, ref saveData);
            SaveAndRestore.save_bytes(ref i, preTranferReasonLegacy, ref saveData);
            SaveAndRestore.save_ushorts(ref i, petrolBuffer, ref saveData);
            SaveAndRestore.save_bools(ref i, alreadyAskForFuelLegacy, ref saveData);
            SaveAndRestore.save_bytes(ref i, tempVehicleForFuelCount, ref saveData);
            SaveAndRestore.save_bytes(ref i, finalVehicleForFuelCount, ref saveData);
            i = 0;
            ushort[] TargetGasBuildingMoreVehicle = new ushort[49152];
            byte[] preTranferReasonMoreVehicle = new byte[49152];
            bool[] alreadyAskForFuelMoreVehicle = new bool[49152];
            for (int j = 0; j < 49152; j++)
            {
                TargetGasBuildingMoreVehicle[j] = TargetGasBuilding[j + 16384];
                preTranferReasonMoreVehicle[j] = preTranferReason[j + 16384];
                alreadyAskForFuelMoreVehicle[j] = alreadyAskForFuel[j + 16384];
            }
            SaveAndRestore.save_ushorts(ref i, TargetGasBuildingMoreVehicle, ref saveDataForMoreVehicle);
            SaveAndRestore.save_bytes(ref i, preTranferReasonMoreVehicle, ref saveDataForMoreVehicle);
            SaveAndRestore.save_bools(ref i, alreadyAskForFuelMoreVehicle, ref saveDataForMoreVehicle);
        }

        public static void load()
        {
            int i = 0;
            ushort[] TargetGasBuildingLegacy = new ushort[16384];
            byte[] preTranferReasonLegacy = new byte[16384];
            bool[] alreadyAskForFuelLegacy = new bool[16384];
            TargetGasBuildingLegacy = SaveAndRestore.load_ushorts(ref i, saveData, TargetGasBuildingLegacy.Length);
            preTranferReasonLegacy = SaveAndRestore.load_bytes(ref i, saveData, preTranferReasonLegacy.Length);
            petrolBuffer = SaveAndRestore.load_ushorts(ref i, saveData, petrolBuffer.Length);
            alreadyAskForFuelLegacy = SaveAndRestore.load_bools(ref i, saveData, alreadyAskForFuelLegacy.Length);
            tempVehicleForFuelCount = SaveAndRestore.load_bytes(ref i, saveData, tempVehicleForFuelCount.Length);
            finalVehicleForFuelCount = SaveAndRestore.load_bytes(ref i, saveData, finalVehicleForFuelCount.Length);
            //for legacy, other 49152 will be loaded in other place
            for (int j = 0; j < 16384; j++)
            {
                TargetGasBuilding[j] = TargetGasBuildingLegacy[j];
                preTranferReason[j] = preTranferReasonLegacy[j];
                alreadyAskForFuel[j] = alreadyAskForFuelLegacy[j];
            }
        }
        public static void loadForMoreVehicle()
        {
            int i = 0;
            ushort[] TargetGasBuildingMoreVehicle = new ushort[49152];
            byte[] preTranferReasonMoreVehicle = new byte[49152];
            bool[] alreadyAskForFuelMoreVehicle = new bool[49152];
            TargetGasBuildingMoreVehicle = SaveAndRestore.load_ushorts(ref i, saveDataForMoreVehicle, TargetGasBuildingMoreVehicle.Length);
            preTranferReasonMoreVehicle = SaveAndRestore.load_bytes(ref i, saveDataForMoreVehicle, preTranferReasonMoreVehicle.Length);
            alreadyAskForFuelMoreVehicle = SaveAndRestore.load_bools(ref i, saveDataForMoreVehicle, alreadyAskForFuelMoreVehicle.Length);
            for (int j = 0; j < 49152; j++)
            {
                TargetGasBuilding[j + 16384] = TargetGasBuildingMoreVehicle[j];
                preTranferReason[j + 16384] = preTranferReasonMoreVehicle[j];
                alreadyAskForFuel[j + 16384] = alreadyAskForFuelMoreVehicle[j];
            }
            DebugLog.LogToFileOnly("saveDataForMoreVehicle in MainDataStore is " + i.ToString());
        }
    }
}
