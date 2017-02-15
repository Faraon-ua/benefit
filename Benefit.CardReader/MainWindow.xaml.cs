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
        private ApiService apiService;
        private ReaderService readerService;
        private App app;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            defaultWindow = new DefaultWindow();
            httpClient = new BenefitHttpClient();
            apiService = new ApiService();
            readerService = new ReaderService();
            app = (App) Application.Current;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var isconnected = httpClient.CheckForInternetConnection();
            if (isconnected)
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Visible;
                defaultWindow.NoConnection.Visibility = Visibility.Collapsed;

                var handShake = readerService.HandShake();
                if (handShake == null)
                {
                    //show no device connected
                }
                else
                {
                    //get license key from device;
                    app.Token = apiService.GetAuthAToken(handShake.LicenseKey);
                    if (app.Token == null)
                    {
                        //show device not authorized
                    }
                    else
                    {
                        defaultWindow.Show();
                        Close();
                    }
                }
            }
            else
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Collapsed;
                defaultWindow.NoConnection.Visibility = Visibility.Visible;
            }
            ((App) Application.Current).IsConnected = isconnected;
        }
    }
}
