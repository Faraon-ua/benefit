﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.DataTransfer.Reader;
using Benefit.CardReader.Services;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int PingOnlinePeriod = 15;//minutes

        public bool IsConnected { get; set; }
        public bool IsDeviceConnected { get; set; }
        public BenefitAuthInfo AuthInfo { get; set; }
        public string Token { get; set; }
        public string LicenseKey { get; set; }

        private DispatcherTimer timer;
        public App()
        {
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, PingOnlinePeriod, 0);
            timer.Start();
            AuthInfo = new BenefitAuthInfo();
            CheckForMultipleLaunches();
//            InstallMeOnStartUp();
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
