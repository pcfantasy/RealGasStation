using System;
using ICities;
using System.IO;

namespace RealGasStation.Util
{
    public class SaveAndRestore : SerializableDataExtensionBase
    {
        private static ISerializableData _serializableData;

        public static void SaveData(ref int idex, byte[] item, ref byte[] container)
        {
            int j;
            for (j = 0; j < item.Length; j++)
            {
                container[idex + j] = item[j];
            }
            idex += item.Length;
        }

        public static void LoadData(ref int idex, byte[] container, ref byte[] item)
        {
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < item.Length; i++)
                {
                    item[i] = container[idex];
                    idex ++;
                }
            }
            else
            {
                for (i = 0; i < item.Length; i++)
                {
                    idex ++;
                }
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
            }
        }

        public static void SaveData(ref int idex, ushort[] item, ref byte[] container)
        {
            int i; int j;
            byte[] bytes;
            for (j = 0; j < item.Length; j++)
            {
                bytes = BitConverter.GetBytes(item[j]);
                for (i = 0; i < bytes.Length; i++)
                {
                    container[idex + i] = bytes[i];
                }
                idex += bytes.Length;
            }
        }

        public static void LoadData(ref int idex, byte[] container, ref ushort[] item)
        {
            int i;
            if (idex < container.Length)
            {
                for (i = 0; i < item.Length; i++)
                {
                    item[i] = BitConverter.ToUInt16(container, idex);
                    idex += 2;
                }
            }
            else
            {
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
                for (i = 0; i < item.Length; i++)
                {
                    idex += 2;
                }
            }
        }

        public static void SaveData(ref int idex, bool[] item, ref byte[] container)
        {
            int i; int j;
            byte[] bytes;
            for (j = 0; j < item.Length; j++)
            {
                bytes = BitConverter.GetBytes(item[j]);
                for (i = 0; i < bytes.Length; i++)
                {
                    container[idex + i] = bytes[i];
                }
                idex += bytes.Length;
            }
        }

        public static void LoadData(ref int idex, byte[] container, ref bool[] item)
        {
            if (idex < container.Length)
            {
                int i;
                for (i = 0; i < item.Length; i++)
                {
                    item[i] = BitConverter.ToBoolean(container, idex);
                    idex++;
                }
            }
            else
            {
                int i;
                for (i = 0; i < item.Length; i++)
                {
                    idex++;
                }
                DebugLog.LogToFileOnly("load data is too short, please check" + container.Length.ToString());
            }
        }

        public override void OnCreated(ISerializableData serializableData)
        {
            SaveAndRestore._serializableData = serializableData;
        }

        public override void OnSaveData()
        {
            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                DebugLog.LogToFileOnly("StartSave");
                var saveData = new byte[212992];
                MainDataStore.Save(ref saveData);
                SaveAndRestore._serializableData.SaveData("RealGasStation MainDataStore", saveData);
                saveData = new byte[196608];
                MainDataStore.SaveForMoreVehicle(ref saveData);
                SaveAndRestore._serializableData.SaveData("RealGasStation saveDataForMoreVehicle", saveData);
                SaveAndRestore._serializableData.SaveData("RealGasStation resourceCategory", MainDataStore.resourceCategory);
                
            }
        }

        public override void OnLoadData()
        {
            MainDataStore.DataInit();
            DebugLog.LogToFileOnly("StartLoad");

            var saveData = SaveAndRestore._serializableData.LoadData("RealGasStation MainDataStore");
            if (saveData == null)
            {
                DebugLog.LogToFileOnly("no RealGasStation MainDataStore save data, please check");
            }
            else
            {
                MainDataStore.Load(ref saveData);
            }

            saveData = SaveAndRestore._serializableData.LoadData("RealGasStation saveDataForMoreVehicle");
            if (saveData == null)
            {
                DebugLog.LogToFileOnly("no RealGasStation MainDataStore saveDataForMoreVehicle, please check");
            }
            else
            {
                MainDataStore.LoadForMoreVehicle(ref saveData);
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
        }
    }
}
