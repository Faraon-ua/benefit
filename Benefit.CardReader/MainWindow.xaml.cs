using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.Services;
using Benefit.CardReader.Services.Factories;
using Benefit.HttpClient;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan _minLoadTime = new TimeSpan(0, 0, 5);//sec

        private const int CheckConnectionPeriod = 15; //seconds
        private const int CheckDevicePeriod = 60; //seconds
        private DispatcherTimer onlineCheckTimer;
        private DispatcherTimer deviceCheckTimer;

        private DefaultWindow defaultWindow;
        private BenefitHttpClient httpClient;
        public ReaderService readerService;
        public OfflineService OfflineService;
        private App app;

        private DateTime startTime;
        private DateTime endTime;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            defaultWindow = new DefaultWindow();
            //            ((DefaultWindow)defaultWindow).SiteHyperlink.RequestNavigate += Hyperlink_OnRequestNavigate;
            httpClient = new BenefitHttpClient();
            OfflineService = new OfflineService();
            readerService = new ReaderService();
            readerService.CardReaded += readerService_CardReaded;
            app = (App)Application.Current;
            
            onlineCheckTimer = new DispatcherTimer();
            onlineCheckTimer.Tick += CheckConnection;
            onlineCheckTimer.Interval = new TimeSpan(0, 0, CheckConnectionPeriod);
            onlineCheckTimer.Start();

            deviceCheckTimer = new DispatcherTimer();
            deviceCheckTimer.Tick += DeviceCheck;
            deviceCheckTimer.Interval = new TimeSpan(0, 0, CheckDevicePeriod);
            deviceCheckTimer.Start();
        }

        private void DeviceCheck(object sender, EventArgs eventArgs)
        {
            SetView();
        }

        void CheckConnection(object sender, EventArgs e)
        {
            var isconnected = httpClient.CheckForInternetConnection();
            ((App)Application.Current).IsConnected = isconnected;

            if (isconnected)
            {
                OfflineService.ProcessStoredPayments(app.LicenseKey);
                defaultWindow.TransactionPartial.btnChargeBonuses.Visibility = Visibility.Visible;
                defaultWindow.TransactionPartial.btnUserInfo.Visibility = Visibility.Visible;
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Visible;
                defaultWindow.NoConnection.Visibility = Visibility.Collapsed;
            }
            else
            {
                defaultWindow.TransactionPartial.btnChargeBonuses.Visibility = Visibility.Hidden;
                defaultWindow.TransactionPartial.btnUserInfo.Visibility = Visibility.Hidden;
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Collapsed;
                defaultWindow.NoConnection.Visibility = Visibility.Visible;
            }
        }

        /*        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
                {
                    System.Diagnostics.Process.Start(e.Uri.ToString());
                }*/

        private void SetView()
        {
            //view bar
            var handShake = readerService.HandShake();
            if (handShake == null)
            {
                ((App)Application.Current).IsDeviceConnected = false;
                //show no device connected
                defaultWindow.ShowSingleControl(ViewType.DeviceNotConnected, false);
            }
            else
            {
                ((App)Application.Current).IsDeviceConnected = true;
                //get license key from device;
                app.Token = ReaderFactory.GetReaderManager(app.IsConnected).GetAuthToken(handShake.LicenseKey);
                app.LicenseKey = handShake.LicenseKey;
                if (app.Token == null)
                {
                    //show device not authorized
                    defaultWindow.ShowSingleControl(ViewType.DeviceNotAuthorized);
                }
                else
                {
                    defaultWindow.ShowSingleControl();
                }
            }
        }

        public void CloseComPort()
        {
            readerService.CloseComPort();
        }
        private void readerService_CardReaded(object sender, Common.CustomEventArgs.NfcEventArgs e)
        {
            //reset timer
            deviceCheckTimer.Stop();
            deviceCheckTimer.Start();
            Dispatcher.Invoke(new Action(() => defaultWindow.Show()));

            //auth user
            if (app.AuthInfo.CashierNfc != null)
            {
                //if returning cashier - log off
                if (app.AuthInfo.CashierNfc == e.NfcCode)
                {
                    app.AuthInfo.CashierNfc = null;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        defaultWindow.ShowSingleControl(ViewType.CashierPartial);
                    }));
                }
                //if new card - user auth
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        defaultWindow.LoadingSpinner.Visibility = Visibility.Visible;
                    }));
                    var user = ReaderFactory.GetReaderManager(app.IsConnected).AuthUser(e.NfcCode);
                    if (user != null)
                    {
                        app.AuthInfo.UserNfc = e.NfcCode;
                        app.AuthInfo.UserName = user.Name;
                        app.AuthInfo.UserCard = user.AvailableBonuses;

                        Dispatcher.Invoke(new Action(() =>
                        {
                            defaultWindow.TransactionPartial.SellerName.Text = app.AuthInfo.SellerName;
                            defaultWindow.TransactionPartial.CashierName.Text = app.AuthInfo.CashierName;
                            defaultWindow.TransactionPartial.UserName.Text = user.Name;
                            defaultWindow.TransactionPartial.UserCard.Text = user.AvailableBonuses;
                            defaultWindow.ShowSingleControl(ViewType.TransactionPartial);
                            defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                            defaultWindow.ShowErrorMessage("Картка клієнта не активована");
                        }));
                    }
                }
            }
            //auth cashier
            else if (app.Token != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    defaultWindow.LoadingSpinner.Visibility = Visibility.Visible;
                }));
                var cashierSeller = ReaderFactory.GetReaderManager(app.IsConnected).AuthCashier(e.NfcCode);
                if (cashierSeller != null)
                {
                    app.AuthInfo.CashierNfc = e.NfcCode;
                    app.AuthInfo.CashierName = cashierSeller.CashierName;
                    app.AuthInfo.SellerName = cashierSeller.SellerName;

                    Dispatcher.Invoke(new Action(() =>
                    {
                        defaultWindow.UserAuthPartial.SellerName.Text = cashierSeller.SellerName;
                        defaultWindow.UserAuthPartial.CashierName.Text = cashierSeller.CashierName;
                        defaultWindow.ShowSingleControl(ViewType.UserAuthPartial);
                        defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                        defaultWindow.TransactionPartial.txtBillNumber.Visibility = cashierSeller.ShowBill
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                        defaultWindow.ShowErrorMessage("Картка касира не активована");
                    }));
                }
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            startTime = DateTime.Now;
            //connection bar
            CheckConnection(null, null);

            SetView();

            endTime = DateTime.Now;
            var actualLoadTime = endTime - startTime;
            if (actualLoadTime < _minLoadTime)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_minLoadTime - actualLoadTime);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        defaultWindow.Show();
                        Close();
                    }));
                });
            }
            else
            {
                defaultWindow.Show();
                Close();
            }
        }
    }
}
