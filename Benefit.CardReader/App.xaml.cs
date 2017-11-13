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
        }

        void ProcessArguments()
        {
            var args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
            {
                var arg = args[index].Replace("--", "").Replace("-", "");
                arguments.Add(arg, args[index + 1]);
            }
            // check for price and bill number
            if (arguments.Any())
            {
                var price = new IntPtr(int.Parse(arguments["price"]));
                var CommunicationService = new CommunicationService();
                var proc = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(proc.ProcessName);

                if (processes.Length > 1)
                {
                    foreach (var p in processes)
                    {
                        if (p.Id != proc.Id)
                        {
                            CommunicationService.SendMessage(p.MainWindowHandle, 0x000C, price, IntPtr.Zero);
                        }
                    }
                }
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
            apiService.PingOnline(Token);
        }

        public void ClearUserAndSellerInfo()
        {
            AuthInfo.UserNfc = null;
            AuthInfo.UserName = null;
            AuthInfo.UserCard = null;
        }
    }
}
