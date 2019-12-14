using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.Util
{
    public class MainDataStore
    {
        public static ushort[] TargetGasBuilding = new ushort[16384];
        public static byte[] tempVehicleForFuelCount = new byte[49152];
        public static byte[] finalVehicleForFuelCount = new byte[49152];
        public static byte[] preTranferReason = new byte[16384];
        public static ushort[] petrolBuffer = new ushort[49152];
        public static bool[] alreadyAskForFuel = new bool[16384];
        public static bool[] isBuildingReleased = new bool[49152];
        public static byte[] resourceCategory = new byte[49152];

        public static ushort lastVehicle = 0;
        public static ushort lastBuilding = 0;
        public static byte lastLanguage = 0;

        public static byte[] saveData = new byte[262144];

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
            SaveAndRestore.save_ushorts(ref i, TargetGasBuilding, ref saveData);
            SaveAndRestore.save_bytes(ref i, preTranferReason, ref saveData);
            SaveAndRestore.save_ushorts(ref i, petrolBuffer, ref saveData);
            SaveAndRestore.save_bools(ref i, alreadyAskForFuel, ref saveData);
            SaveAndRestore.save_bytes(ref i, tempVehicleForFuelCount, ref saveData);
            SaveAndRestore.save_bytes(ref i, finalVehicleForFuelCount, ref saveData);
        }

        public static void load()
        {
            int i = 0;
            TargetGasBuilding = SaveAndRestore.load_ushorts(ref i, saveData, TargetGasBuilding.Length);
            //for (int j = 0; j < MainDataStore.preTargetBuilding.Length; j++)
            //{
            //    DebugLog.LogToFileOnly("preTargetBuilding[j] = " + preTargetBuilding[j].ToString());
            //}
            preTranferReason = SaveAndRestore.load_bytes(ref i, saveData, preTranferReason.Length);
            petrolBuffer = SaveAndRestore.load_ushorts(ref i, saveData, petrolBuffer.Length);
            alreadyAskForFuel = SaveAndRestore.load_bools(ref i, saveData, TargetGasBuilding.Length);
            tempVehicleForFuelCount = SaveAndRestore.load_bytes(ref i, saveData, tempVehicleForFuelCount.Length);
            finalVehicleForFuelCount = SaveAndRestore.load_bytes(ref i, saveData, finalVehicleForFuelCount.Length);
        }

    }
}
