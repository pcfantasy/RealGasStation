using CitiesHarmony.API;
using ICities;
using System.IO;

namespace RealGasStation
{
    public class RealGasStation: IUserMod
    {
        public static bool IsEnabled = false;
        public static int language_idex = 0;

        public string Name
        {
            get { return "Real Gas Station"; }
        }

        public string Description
        {
            get { return "Cars will need Fuel Now!."; }
        }

        public void OnEnabled()
        {
            IsEnabled = true;
            FileStream fs = File.Create("RealGasStation.txt");
            fs.Close();
            HarmonyHelper.EnsureHarmonyInstalled();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
        }

    }
}
