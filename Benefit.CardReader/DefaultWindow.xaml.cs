using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Benefit.HttpClient;
using System.IO.Ports;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for DefaultWindow.xaml
    /// </summary>
    public partial class DefaultWindow : Window
    {
        private const int CheckConnectionPeriod = 10;
        private DispatcherTimer dispatcherTimer;
        private BenefitHttpClient httpClient;

        private SerialPort _serialPort;
        private char[] _readSymbols;
        private List<Control> _controls = new List<Control>();
        public DefaultWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            httpClient = new BenefitHttpClient();
            Loaded += DefaultWindow_Loaded;
            //add all views
            _controls.Add(CashierPartial);
            _controls.Add(UserAuthPartial);
            _controls.Add(TransactionPartial);
        }

        void DefaultWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Tick += CheckConnection;
            dispatcherTimer.Interval = new TimeSpan(0, 0, CheckConnectionPeriod);
            dispatcherTimer.Start();
        }

        void CheckConnection(object sender, EventArgs e)
        {
            var isconnected = httpClient.CheckForInternetConnection();
            if (isconnected)
            {
                ConnectionEsteblished.Visibility = Visibility.Visible;
                NoConnection.Visibility = Visibility.Collapsed;
            }
            else
            {
                ConnectionEsteblished.Visibility = Visibility.Collapsed;
                NoConnection.Visibility = Visibility.Visible;
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
