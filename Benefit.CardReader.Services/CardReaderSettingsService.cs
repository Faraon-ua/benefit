using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Benefit.Common.Constants;

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

        public static string BillInfoFilePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "billinfo.xml");
            }
        }
        public static string DefinedNfcReaderPortName
        {
            get
            {
                var portsConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"config", "ports.config");
                if (!File.Exists(portsConfigFilePath)) return null;
                var configValues = GetConfigValues(portsConfigFilePath);
                if(!configValues.ContainsKey(ReaderConstants.ComPortSettingName)) return null;
                return configValues[ReaderConstants.ComPortSettingName];
            }
        }

        private static Dictionary<string, string> GetConfigValues(string configFilePath)
        {
            var text = File.ReadAllText(configFilePath);
            var configRows = text.Split(';');
            return
                configRows.Select(configRow => configRow.Split('='))
                    .ToDictionary(configValues => configValues.First(), configValues => configValues.Last());
        }
    }
}
