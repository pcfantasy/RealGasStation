using System;
using ICities;
using System.IO;

namespace RealGasStation.Util
{
    public class SaveAndRestore : SerializableDataExtensionBase
    {
        private static ISerializableData _serializableData;


        public static void save_bytes(ref int idex, byte[] item, ref byte[] container)
        {
            int j;
            for (j = 0; j < item.Length; j++)
            {
                container[idex + j] = item[j];
            }
            idex = idex + item.Length;
        }

        public static byte[] load_bytes(ref int idex, byte[] container, int length)
        {
            byte[] tmp = new byte[length];
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < length; i++)
                {
                    tmp[i] = container[idex];
                    idex = idex + 1;
                }
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    idex = idex + 1;
                }
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
            }
            return tmp;
        }

        public static void save_ushorts(ref int idex, ushort[] item, ref byte[] container)
        {
            int i; int j;
            byte[] temp_data;
            for (j = 0; j < item.Length; j++)
            {
                temp_data = BitConverter.GetBytes(item[j]);
                for (i = 0; i < temp_data.Length; i++)
                {
                    container[idex + i] = temp_data[i];
                }
                idex = idex + temp_data.Length;
            }
        }

        public static ushort[] load_ushorts(ref int idex, byte[] container, int length)
        {
            ushort[] tmp = new ushort[length];
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < length; i++)
                {
                    tmp[i] = BitConverter.ToUInt16(container, idex);
                    idex = idex + 2;
                }
            }
            else
            {
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
                for (i = 0; i < length; i++)
                {
                    idex = idex + 2;
                }
            }
            return tmp;
        }

        public static void save_bools(ref int idex, bool[] item, ref byte[] container)
        {
            int i; int j;
            byte[] temp_data;
            for (j = 0; j < item.Length; j++)
            {
                temp_data = BitConverter.GetBytes(item[j]);
                for (i = 0; i < temp_data.Length; i++)
                {
                    container[idex + i] = temp_data[i];
                }
                idex = idex + temp_data.Length;
            }
        }


        public static bool[] load_bools(ref int idex, byte[] container, int length)
        {
            bool[] tmp = new bool[length];
            if (idex < container.Length)
            {
                int i;
                for (i = 0; i < length; i++)
                {
                    tmp[i] = BitConverter.ToBoolean(container, idex);
                    idex = idex + 1;
                }
            }
            else
            {
                int i;
                for (i = 0; i < length; i++)
                {
                    idex = idex + 1;
                }
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
            }
            return tmp;
        }

        public static void gather_saveData()
        {
            MainDataStore.save();
        }


        public override void OnCreated(ISerializableData serializableData)
        {
            SaveAndRestore._serializableData = serializableData;
        }

        public override void OnReleased()
        {
        }

        public override void OnSaveData()
        {
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                DebugLog.LogToFileOnly("startsave");
                MainDataStore.saveData = new byte[262144];
                MainDataStore.saveDataForMoreVehicle = new byte[196608];
                gather_saveData();
                SaveAndRestore._serializableData.SaveData("RealGasStation MainDataStore", MainDataStore.saveData);
                SaveAndRestore._serializableData.SaveData("RealGasStation resourceCategory", MainDataStore.resourceCategory);
                SaveAndRestore._serializableData.SaveData("RealGasStation saveDataForMoreVehicle", MainDataStore.saveDataForMoreVehicle);
            }
        }

        public override void OnLoadData()
        {
            MainDataStore.DataInit();
            MainDataStore.saveData = new byte[262144];
            MainDataStore.resourceCategory = new byte[49152];
            MainDataStore.saveDataForMoreVehicle = new byte[196608];
            DebugLog.LogToFileOnly("OnLoadData");
            DebugLog.LogToFileOnly("startload");

            MainDataStore.saveData = SaveAndRestore._serializableData.LoadData("RealGasStation MainDataStore");
            if (MainDataStore.saveData == null)
            {
                DebugLog.LogToFileOnly("no RealGasStation MainDataStore save data, please check");
            }
            else
            {
                MainDataStore.load();
            }

            MainDataStore.resourceCategory = SaveAndRestore._serializableData.LoadData("RealGasStation resourceCategory");
            if (MainDataStore.resourceCategory == null)
            {
                DebugLog.LogToFileOnly("no RealGasStation resourceCategory save data, please check");
                MainDataStore.resourceCategory = new byte[49152];
                for (int i = 0; i < MainDataStore.resourceCategory.Length; i++)
                {
                    MainDataStore.resourceCategory[i] = 0;
                }
            }
            else
            {
                //MainDataStore.load();
            }

            MainDataStore.saveDataForMoreVehicle = SaveAndRestore._serializableData.LoadData("RealGasStation saveDataForMoreVehicle");
            if (MainDataStore.saveDataForMoreVehicle == null)
            {
                DebugLog.LogToFileOnly("no RealGasStation MainDataStore saveDataForMoreVehicle, please check");
            }
            else
            {
                MainDataStore.loadForMoreVehicle();
            }

        }
    }
}
