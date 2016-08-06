using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Benefit.Services
{
    public class SettingsService
    {
        public static List<string> SupportedLocalizations
        {
            get
            {
                return ConfigurationManager.AppSettings["SupportedLocalizations"].Split(',').ToList();
            }
        } 
    }
}
