using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.Communication;
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
                var CommunicationService = new CommunicationService();
                var proc = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(proc.ProcessName);
                IntPtr price = IntPtr.Zero, bill = IntPtr.Zero, chargeBonuses = IntPtr.Zero;
                if (arguments.ContainsKey("price"))
                {
                    price = new IntPtr(int.Parse(arguments["price"]));
                }
                if (arguments.ContainsKey("bill"))
                {
                    bill = new IntPtr(int.Parse(arguments["bill"]));
                }
                if (arguments.ContainsKey("chargebonuses"))
                {
                    chargeBonuses = new IntPtr(int.Parse(arguments["chargebonuses"]));
                }
                if (processes.Length > 1)
                {
                    foreach (var p in processes)
                    {
                        if (p.Id != proc.Id)
                        {
                            if (bill != IntPtr.Zero)
                            {
                                CommunicationService.SendMessage(p.MainWindowHandle,
                                    CardReaderSettingsService.SetBillWindowsMessageId, bill, IntPtr.Zero);
                            }
                            CommunicationService.SendMessage(p.MainWindowHandle, CardReaderSettingsService.SetChargeBonusesWindowsMessageId, chargeBonuses, IntPtr.Zero);
                            CommunicationService.SendMessage(p.MainWindowHandle, CardReaderSettingsService.SetPriceWindowsMessageId, price, IntPtr.Zero);
                        }
                    }
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
            if (Token == null || !IsConnected || !IsDeviceConnected) return;
            var apiService = new ApiService();
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
