using ColossalFramework;
using HarmonyLib;
using RealGasStation.NewAI;
using RealGasStation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public class PlayerBuildingAICreateBuildingPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(PlayerBuildingAI).GetMethod("CreateBuilding", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Building).MakeByRefType() }, null);
        }
        public static void Prefix(ushort buildingID, ref Building data)
        {
            if (data.Info.name == "1908304789.gas station(w/o NE2)_Data")
            {
                //special building default value
                MainDataStore.resourceCategory[buildingID] = 1;
            }
        }
    }
}
