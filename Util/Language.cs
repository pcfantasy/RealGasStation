using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealGasStation.Util
{
    public class Language
    {
        public static string[] English =
        {
            "Language",                                                                                      //0
            "Language_Select",                                                                               //1
            "Petrol Stored",
            "Transfer construction resource to",
            "Transfer operation resource to",
            "Going for Fuel to",
            "RealGasStation UI",
        };



        public static string[] Chinese =
            {
            "语言",                                                   //0
            "语言选择",                                               //1
            "储存的石油",
            "运输建筑材料到",
            "运输日常运营材料到",
            "前往加油站",
            "RealGasStation 界面",
        };


        public static string[] Strings = new string[English.Length];

        public static byte currentLanguage = 255;

        public static void LanguageSwitch(byte language)
        {
            if (language == 1)
            {
                for (int i = 0; i < English.Length; i++)
                {
                    Strings[i] = Chinese[i];
                }
                currentLanguage = 1;
            }
            else if (language == 0)
            {
                for (int i = 0; i < English.Length; i++)
                {
                    Strings[i] = English[i];
                }
                currentLanguage = 0;
            }
            else
            {
                DebugLog.LogToFileOnly("unknow language!! use English");
                for (int i = 0; i < English.Length; i++)
                {
                    Strings[i] = English[i];
                }
                currentLanguage = 0;
            }
        }
    }
}
