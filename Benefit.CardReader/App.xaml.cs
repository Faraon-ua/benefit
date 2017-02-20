using System;
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
        public BenefitAuthInfo AuthInfo { get; set; }
        public string Token { get; set; }

        private DispatcherTimer timer;
        public App()
        {
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, PingOnlinePeriod, 0);
            timer.Start();
            AuthInfo = new BenefitAuthInfo();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (Token == null || !IsConnected) return;
            var apiService = new ApiService();
            apiService.PingOnline(Token);
        }

        public void ClearUserAndSellerInfo()
        {
            AuthInfo.UserNfc = null;
            AuthInfo.UserName = null;
            AuthInfo.UserCard = null;
            AuthInfo.SellerName = null;
            AuthInfo.SellerName = null;
        }
    }
}
