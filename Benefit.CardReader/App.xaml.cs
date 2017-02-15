using System;
using System.Windows;
using System.Windows.Threading;
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
        public string UserNfc { get; set; }
        public string CashierNfc { get; set; }
        public string CashierName { get; set; }
        public string SellerName { get; set; }
        public string Token { get; set; }

        private DispatcherTimer timer;
        public App()
        {
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, PingOnlinePeriod, 0);
            timer.Start();
            var apiService = new ApiService();
            apiService.PingOnline("uUQxiCQJyrqpA7lln8RvXB5dasw7-PTZDde0BRRaV-jNPSQyMPetqFFgGXM0mef6nNDfroHrpJWx4kr1AktvI8Rl_PseY8bGY1hpiRaHQImNb9uzp9vktapk4cL30upyDt0gRtSLlAN7KEQOUt--StjdlnQfJXsRIqQhvxIZjeh69y8Z36llK2GKRFv7kCTyYqphyslU6PUMBhUxwjoHn5vgAgJAiHm-Vz_OyeIGrzc");
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if(Token == null || !IsConnected) return;
            var apiService = new ApiService();
            apiService.PingOnline(Token);
        }
    }
}
