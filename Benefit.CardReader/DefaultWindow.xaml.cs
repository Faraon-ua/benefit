using System;
using System.Windows;
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
        public DefaultWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            httpClient = new BenefitHttpClient();
            Loaded += DefaultWindow_Loaded;

            _readSymbols = new char[256];
            var comPortList = SerialPort.GetPortNames();

            _serialPort = new SerialPort {BaudRate = 38400, ReadTimeout = 500, WriteTimeout = 500, };
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
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
