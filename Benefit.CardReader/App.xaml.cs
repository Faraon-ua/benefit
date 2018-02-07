using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.DataTransfer.Reader;
using Benefit.CardReader.Services;
using NLog;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Dictionary<string, string> arguments = new Dictionary<string, string>();
        private const int PingOnlinePeriod = 15;//minutes
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public bool IsConnected { get; set; }
        public bool IsDeviceConnected { get; set; }
        public BenefitAuthInfo AuthInfo { get; set; }
        public string Token { get; set; }
        public string LicenseKey { get; set; }

        private DispatcherTimer timer;
        public App()
        {
            ProcessArguments();
            CheckForMultipleLaunches();
            RegisterReaderEnviromentVariable();

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, PingOnlinePeriod, 0);
            timer.Start();
            AuthInfo = new BenefitAuthInfo();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var _logger = LogManager.GetCurrentClassLogger();
            _logger.Fatal(e.Exception.ToString);
            e.Handled = true;
        }

        void ProcessArguments()
        {
            var args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
            {
                var arg = args[index].Replace("--", "").Replace("-", "");
                arguments.Add(arg, args[index + 1]);
            }
            if (arguments.Any())
            {
                double? price = null;
                string bill = null;
                var chargeBonuses = false;
                var flashSum = false;
                if (arguments.ContainsKey("price"))
                {
                    var priceVal = arguments["price"].Replace(',', '.');
                    price = double.Parse(priceVal, NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (arguments.ContainsKey("bill"))
                {
                    bill = arguments["bill"];
                }
                if (arguments.ContainsKey("chargebonuses"))
                {
                    chargeBonuses = int.Parse(arguments["chargebonuses"]) == 1;
                }
                if (arguments.ContainsKey("flashprice"))
                {
                    flashSum = int.Parse(arguments["flashprice"]) == 1;
                }
                if (price.HasValue)
                {
                    var billInfo = new BillInfo()
                    {
                        Sum = price.Value,
                        Number = bill,
                        ChargeBonuses = chargeBonuses
                    };
                    _logger.Info("[PriceImport]" + billInfo);
                    FileService.XmlSerialize(CardReaderSettingsService.BillInfoFilePath, billInfo);
                }
                if (flashSum)
                {
                    _logger.Info("[FlashPrice]");
                    FileService.XmlSerialize<BillInfo>(CardReaderSettingsService.BillInfoFilePath, null, true);
                }
                Current.Shutdown();
            }
        }

        void RegisterReaderEnviromentVariable()
        {
            try
            {
                var appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Benefit.CardReader.exe");
                Environment.SetEnvironmentVariable(CardReaderSettingsService.EnviromentVariableName, appPath,
                    EnvironmentVariableTarget.Machine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        void CheckForMultipleLaunches()
        {
            var thisprocessname = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
                Current.Shutdown();
        }
        void InstallMeOnStartUp()
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                var curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch
            {
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (Token == null || !IsConnected || !IsDeviceConnected)
            {
                if (AuthInfo.LogRequests)
                {
                    var message = string.Format("[PingFail] Token: {0}\nIsConnected: {1}\n IsDeviceConnected: {2}\n\n", Token, IsConnected, IsDeviceConnected);
                    _logger.Info(message);
                }
                return;
            }
            var apiService = new ApiService()
            {
                LogRequests = AuthInfo.LogRequests
            };
            apiService.PingOnline(Token, LicenseKey);
        }

        public void ClearUserAndSellerInfo()
        {
            AuthInfo.UserNfc = null;
            AuthInfo.UserName = null;
            AuthInfo.UserCard = null;
        }
    }
}
