using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.Util
{
    public class MainDataStore
    {
        public static ushort[] preTargetBuilding = new ushort[16384];
        public static byte[] preTranferReason = new byte[16384];
        public static ushort[] petrolBuffer = new ushort[49152];
        public static bool[] alreadyAskForFuel = new bool[16384];

        public static ushort lastVehicle = 0;
        public static ushort lastBuilding = 0;
        public static byte lastLanguage = 0;

        public static byte[] saveData = new byte[163840];

        public static void DataInit()
        {
            for (int i = 0; i < MainDataStore.preTargetBuilding.Length; i++)
            {
                preTargetBuilding[i] = 0;
                alreadyAskForFuel[i] = false;
                preTranferReason[i] = 0;
            }

            for (int i = 0; i < MainDataStore.petrolBuffer.Length; i++)
            {
                petrolBuffer[i] = 0;
            }
        }

        public static void save()
        {
            int i = 0;
            SaveAndRestore.save_ushorts(ref i, preTargetBuilding, ref saveData);
            SaveAndRestore.save_bytes(ref i, preTranferReason, ref saveData);
            SaveAndRestore.save_ushorts(ref i, petrolBuffer, ref saveData);
            SaveAndRestore.save_bools(ref i, alreadyAskForFuel, ref saveData);
        }

        public static void load()
        {
            int i = 0;
            preTargetBuilding = SaveAndRestore.load_ushorts(ref i, saveData, preTargetBuilding.Length);
            preTranferReason = SaveAndRestore.load_bytes(ref i, saveData, preTranferReason.Length);
            petrolBuffer = SaveAndRestore.load_ushorts(ref i, saveData, petrolBuffer.Length);
            alreadyAskForFuel = SaveAndRestore.load_bools(ref i, saveData, preTargetBuilding.Length);
        }

    }
}
