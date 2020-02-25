using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealGasStation.Util
{
    public class MainDataStore
    {
        public static ushort[] TargetGasBuilding = new ushort[65536];
        public static byte[] finalVehicleForFuelCount = new byte[49152];
        public static byte[] preTranferReason = new byte[65536];
        public static ushort[] petrolBuffer = new ushort[49152];
        public static bool[] alreadyAskForFuel = new bool[65536];
        public static bool[] isBuildingReleased = new bool[49152];
        public static byte[] resourceCategory = new byte[49152];
        public static bool[] alreadyPaidForFuel = new bool[65536];

        public static ushort lastVehicle = 0;
        public static ushort lastBuilding = 0;
        public static byte lastLanguage = 0;

        public static void DataInit()
        {
            for (int i = 0; i < MainDataStore.TargetGasBuilding.Length; i++)
            {
                TargetGasBuilding[i] = 0;
                alreadyAskForFuel[i] = false;
                preTranferReason[i] = 0;
                alreadyPaidForFuel[i] = false;
            }

            for (int i = 0; i < MainDataStore.petrolBuffer.Length; i++)
            {
                finalVehicleForFuelCount[i] = 0;
                petrolBuffer[i] = 0;
                isBuildingReleased[i] = false;
            }

            for (int i = 0; i < MainDataStore.resourceCategory.Length; i++)
            {
                resourceCategory[i] = 0;
            }
        }

        public static void Save(ref byte[] saveData)
        {
            //212992
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
            SaveAndRestore.SaveData(ref i, TargetGasBuildingLegacy, ref saveData);
            SaveAndRestore.SaveData(ref i, preTranferReasonLegacy, ref saveData);
            SaveAndRestore.SaveData(ref i, petrolBuffer, ref saveData);
            SaveAndRestore.SaveData(ref i, alreadyAskForFuelLegacy, ref saveData);
            SaveAndRestore.SaveData(ref i, finalVehicleForFuelCount, ref saveData);

            if (i != saveData.Length)
            {
                DebugLog.LogToFileOnly($"MainDataStore Save Error: saveData.Length = {saveData.Length} + i = {i}");
            }
        }

        public static void SaveForMoreVehicle(ref byte[] saveData)
        {
            //196608
            int i = 0;
            ushort[] TargetGasBuildingMoreVehicle = new ushort[49152];
            byte[] preTranferReasonMoreVehicle = new byte[49152];
            bool[] alreadyAskForFuelMoreVehicle = new bool[49152];
            for (int j = 0; j < 49152; j++)
            {
                TargetGasBuildingMoreVehicle[j] = TargetGasBuilding[j + 16384];
                preTranferReasonMoreVehicle[j] = preTranferReason[j + 16384];
                alreadyAskForFuelMoreVehicle[j] = alreadyAskForFuel[j + 16384];
            }
            SaveAndRestore.SaveData(ref i, TargetGasBuildingMoreVehicle, ref saveData);
            SaveAndRestore.SaveData(ref i, preTranferReasonMoreVehicle, ref saveData);
            SaveAndRestore.SaveData(ref i, alreadyAskForFuelMoreVehicle, ref saveData);

            if (i != saveData.Length)
            {
                DebugLog.LogToFileOnly($"MainDataStore SaveForMoreVehicle Error: saveData.Length = {saveData.Length} + i = {i}");
            }
        }

        public static void Load(ref byte[] saveData)
        {
            //212992
            int i = 0;
            ushort[] TargetGasBuildingLegacy = new ushort[16384];
            byte[] preTranferReasonLegacy = new byte[16384];
            bool[] alreadyAskForFuelLegacy = new bool[16384];
            SaveAndRestore.LoadData(ref i, saveData, ref TargetGasBuildingLegacy);
            SaveAndRestore.LoadData(ref i, saveData, ref preTranferReasonLegacy);
            SaveAndRestore.LoadData(ref i, saveData, ref petrolBuffer);
            SaveAndRestore.LoadData(ref i, saveData, ref alreadyAskForFuelLegacy);
            SaveAndRestore.LoadData(ref i, saveData, ref finalVehicleForFuelCount);
            //for legacy, other 49152 will be loaded in other place
            for (int j = 0; j < 16384; j++)
            {
                TargetGasBuilding[j] = TargetGasBuildingLegacy[j];
                preTranferReason[j] = preTranferReasonLegacy[j];
                alreadyAskForFuel[j] = alreadyAskForFuelLegacy[j];
            }

            if (i != saveData.Length)
            {
                DebugLog.LogToFileOnly($"MainDataStore Load Error: saveData.Length = {saveData.Length} + i = {i}");
            }
        }
        public static void LoadForMoreVehicle(ref byte[] saveData)
        {
            //196608
            int i = 0;
            ushort[] TargetGasBuildingMoreVehicle = new ushort[49152];
            byte[] preTranferReasonMoreVehicle = new byte[49152];
            bool[] alreadyAskForFuelMoreVehicle = new bool[49152];
            SaveAndRestore.LoadData(ref i, saveData, ref TargetGasBuildingMoreVehicle);
            SaveAndRestore.LoadData(ref i, saveData, ref preTranferReasonMoreVehicle);
            SaveAndRestore.LoadData(ref i, saveData, ref alreadyAskForFuelMoreVehicle);
            for (int j = 0; j < 49152; j++)
            {
                TargetGasBuilding[j + 16384] = TargetGasBuildingMoreVehicle[j];
                preTranferReason[j + 16384] = preTranferReasonMoreVehicle[j];
                alreadyAskForFuel[j + 16384] = alreadyAskForFuelMoreVehicle[j];
            }

            if (i != saveData.Length)
            {
                DebugLog.LogToFileOnly($"LoadForMoreVehicle Load Error: saveData.Length = {saveData.Length} + i = {i}");
            }
        }
    }
}
