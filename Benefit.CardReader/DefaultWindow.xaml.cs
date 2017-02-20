using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Windows.Threading;
using Benefit.CardReader.Controls;
using Benefit.CardReader.Services;
using Benefit.HttpClient;
using System.IO.Ports;
using System.Linq;
using System.Collections.Generic;

namespace Benefit.CardReader
{
    public enum ViewType
    {
        CashierPartial,
        UserAuthPartial,
        TransactionPartial,
        DeviceNotConnected,
        DeviceNotAuthorized
    }

    /// <summary>
    /// Interaction logic for DefaultWindow.xaml
    /// </summary>
    public partial class DefaultWindow : Window
    {
        private const int CheckConnectionPeriod = 10;
        private DispatcherTimer dispatcherTimer;
        private BenefitHttpClient httpClient;
        public ApiService apiService;
        public App app;

        private SerialPort _serialPort;
        private char[] _readSymbols;
        private Dictionary<ViewType, Control> _controls = new Dictionary<ViewType, Control>();
        public DefaultWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            httpClient = new BenefitHttpClient();
            app = (App)Application.Current;

            Loaded += DefaultWindow_Loaded;
            apiService = new ApiService();
            //add all views
            _controls.Add(ViewType.CashierPartial, CashierPartial);
            _controls.Add(ViewType.UserAuthPartial, UserAuthPartial);
            _controls.Add(ViewType.TransactionPartial, TransactionPartial);
            _controls.Add(ViewType.DeviceNotConnected, DeviceNotConnected);
            _controls.Add(ViewType.DeviceNotAuthorized, DeviceNotAuthorized);
        }

        public void ShowErrorMessage(string errorMessage)
        {
            ErrorMessage.Visibility = Visibility.Visible;
            ErrorMessage.txtError.Text = errorMessage;
        }

        public void ShowSingleControl(ViewType viewType)
        {
            _controls.Select(entry => entry.Value).ToList().ForEach(entry => entry.Visibility = Visibility.Hidden);
            _controls[viewType].Visibility = Visibility.Visible;
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

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
