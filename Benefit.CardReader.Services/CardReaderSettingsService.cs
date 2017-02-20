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
    }
}
