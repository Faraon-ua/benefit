using System.Configuration;

namespace Benefit.CardReader.Services
{
    public class CardReaderSettingsService
    {
        public static string ApiHost
        {
            get
            {
                return ConfigurationManager.AppSettings["ApiHost"];
            }
        }
        public static string ApiPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["ApiPrefix"];
            }
        }
        public static string ApiTokenPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["ApiTokenPrefix"];
            }
        }
        public static string ReaderHandShakePrefix
        {
            get
            {
                return ConfigurationManager.AppSettings["ReaderHandShakePrefix"];
            }
        }
        public static string OfflineFileSalt
        {
            get
            {
                return ConfigurationManager.AppSettings["OfflineFileSalt"];
            }
        }
        public static string EnviromentVariableName
        {
            get
            {
                return ConfigurationManager.AppSettings["EnviromentVariableName"];
            }
        }

        public static int SetPriceWindowsMessageId
        {
            get
            {
                return 0x000C;
            }
        }

        public static int SetBillWindowsMessageId
        {
            get
            {
                return 0x000F;
            }
        }
        public static int SetChargeBonusesWindowsMessageId
        {
            get
            {
                return 0x001A;
            }
        }
    }
}
