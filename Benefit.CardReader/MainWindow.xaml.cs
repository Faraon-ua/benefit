using System;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.Services;
using Benefit.HttpClient;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DefaultWindow defaultWindow;
        private BenefitHttpClient httpClient;
        private DispatcherTimer dispatcherTimer;
        private ApiService apiService;
        private App app;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            defaultWindow = new DefaultWindow();
            httpClient = new BenefitHttpClient();
            dispatcherTimer = new DispatcherTimer();
            apiService = new ApiService();
            app = (App) Application.Current;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += HideLoadScreen;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();

            var isconnected = httpClient.CheckForInternetConnection();
            if (isconnected)
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Visible;
                defaultWindow.NoConnection.Visibility = Visibility.Collapsed;

                //get license key from device;
                app.Token = apiService.GetAuthAToken("801596");
            }
            else
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Collapsed;
                defaultWindow.NoConnection.Visibility = Visibility.Visible;
            }
            ((App) Application.Current).IsConnected = isconnected;
        }

        void HideLoadScreen(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            defaultWindow.Show();
            Close();
        }
    }
}
