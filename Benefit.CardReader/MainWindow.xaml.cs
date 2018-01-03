using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Benefit.CardReader.Services;
using Benefit.CardReader.Services.Factories;
using Benefit.HttpClient;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Benefit.CardReader.Controls;
using Benefit.CardReader.DataTransfer.Reader;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan _minLoadTime = new TimeSpan(0, 0, 5);//sec

        private const int CheckConnectionPeriod = 15; //seconds
        private const int minCheckDevicePeriod = 5; //seconds
        private const int maxCheckDevicePeriod = 60; //seconds
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
            deviceCheckTimer.Interval = new TimeSpan(0, 0, app.IsDeviceConnected ? maxCheckDevicePeriod : minCheckDevicePeriod);
            deviceCheckTimer.Start();
        }

        #region helper methods

        private void DeviceCheck(object sender, EventArgs eventArgs)
        {
            SetView();
        }

        private bool ProcessAuthCashier(string nfcCode)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                defaultWindow.LoadingSpinner.Visibility = Visibility.Visible;
            }));
            var readerManager = ReaderFactory.GetReaderManager(app.IsConnected);
            var cashierSeller = readerManager.AuthCashier(nfcCode);
            if (cashierSeller != null)
            {
                app.AuthInfo.CashierNfc = nfcCode;
                app.AuthInfo.CashierName = cashierSeller.CashierName;
                app.AuthInfo.SellerName = cashierSeller.SellerName;
                app.AuthInfo.ShowBill = cashierSeller.ShowBill;
                app.AuthInfo.ShowChargeBonuses = cashierSeller.ShowBonusesPayment;
                app.AuthInfo.ShowKeyboard = cashierSeller.ShowKeyboard;
                readerManager.LogRequests = app.AuthInfo.LogRequests = cashierSeller.LogRequests;

                Dispatcher.Invoke(new Action(() =>
                {
                    defaultWindow.UserAuthPartial.SellerName.Text = cashierSeller.SellerName;
                    defaultWindow.UserAuthPartial.CashierName.Text = cashierSeller.CashierName;
                    defaultWindow.ShowSingleControl(ViewType.UserAuthPartial);
                    defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                    defaultWindow.TransactionPartial.txtBillNumber.IsEnabled = cashierSeller.ShowBill;
                    defaultWindow.TransactionPartial.txtBillNumber.Background = cashierSeller.ShowBill
                        ? new SolidColorBrush(Color.FromRgb(255, 255, 255))
                        : new SolidColorBrush(Color.FromRgb(153, 153, 153));
                    defaultWindow.TransactionPartial.btnChargeBonuses.IsEnabled = cashierSeller.ShowBonusesPayment;
                    defaultWindow.TransactionPartial.btnChargeBonuses.Background = cashierSeller.ShowBonusesPayment
                        ? new SolidColorBrush(Color.FromRgb(255, 254, 0))
                        : new SolidColorBrush(Color.FromRgb(153, 153, 153));
                }));
                return true;
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                    defaultWindow.ShowErrorMessage("Піднесіть картку касира \n а потім картку клієнта");
                }));
            }
            return false;
        }

        #endregion

        void CheckConnection(object sender, EventArgs e)
        {
            var isconnected = httpClient.CheckForInternetConnection();
            app.IsConnected = isconnected;

            if (isconnected)
            {
                OfflineService.ProcessStoredPayments(app.LicenseKey);
                if (app.AuthInfo.ShowChargeBonuses)
                {
                    defaultWindow.TransactionPartial.btnChargeBonuses.IsEnabled = true;
                    defaultWindow.TransactionPartial.btnChargeBonuses.Background = new SolidColorBrush(Color.FromRgb(255, 254, 0));
                }
                defaultWindow.TransactionPartial.btnUserInfo.Visibility = Visibility.Visible;
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Visible;
                defaultWindow.NoConnection.Visibility = Visibility.Collapsed;
            }
            else
            {
                defaultWindow.TransactionPartial.btnChargeBonuses.IsEnabled = false;
                defaultWindow.TransactionPartial.btnChargeBonuses.Background = new SolidColorBrush(Color.FromRgb(153, 153, 153));
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
                var readerManager = ReaderFactory.GetReaderManager(app.IsConnected);
                app.Token = readerManager.GetAuthToken(handShake.LicenseKey);
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
            deviceCheckTimer.Stop();
            deviceCheckTimer.Interval = new TimeSpan(0, 0, app.IsDeviceConnected ? maxCheckDevicePeriod : minCheckDevicePeriod);
            deviceCheckTimer.Start();
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
            Dispatcher.Invoke(new Action(() =>
            {
                defaultWindow.Show();
                defaultWindow.Activate();
                defaultWindow.Focus();
            }));

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
                            defaultWindow.TransactionPartial.TransactionResult.Visibility = Visibility.Hidden;
                            defaultWindow.TransactionPartial.TransactionPanel.Visibility = Visibility.Visible;
                            defaultWindow.TransactionPartial.txtBillNumber.Text = defaultWindow.TransactionPartial.txtBillNumber.Tag.ToString();
                            defaultWindow.TransactionPartial.txtPaymentSum.Text = defaultWindow.TransactionPartial.txtPaymentSum.Tag.ToString();

                            defaultWindow.TransactionPartial.btnPayBonuses.Focusable = true;
                            defaultWindow.TransactionPartial.btnPayBonuses.Focus();
                            Keyboard.Focus(defaultWindow.TransactionPartial.btnPayBonuses);

                            defaultWindow.TransactionPartial.SellerName.Text = app.AuthInfo.SellerName;
                            defaultWindow.TransactionPartial.CashierName.Text = app.AuthInfo.CashierName;
                            defaultWindow.TransactionPartial.UserName.Text = user.Name;
                            defaultWindow.TransactionPartial.UserCard.Text = user.AvailableBonuses;
                            defaultWindow.ShowSingleControl(ViewType.TransactionPartial);
                            defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;

                            //if bill saved - process
                            var billInfo =
                                FileService.XmlDeserialize<BillInfo>(CardReaderSettingsService.BillInfoFilePath);
                            if (billInfo != null)
                            {
                                defaultWindow.TransactionPartial.txtPaymentSum.Text = billInfo.Sum.ToString();
                                if (app.AuthInfo.ShowBill)
                                {
                                    defaultWindow.TransactionPartial.txtBillNumber.Text = billInfo.Number;
                                }
                                defaultWindow.TransactionPartial.btnPayBonuses.Focus();
                                var _chargeBonuses = billInfo.ChargeBonuses && app.AuthInfo.ShowChargeBonuses;
                                defaultWindow.TransactionPartial.ProcessPayment(_chargeBonuses);
                                FileService.XmlSerialize<BillInfo>(CardReaderSettingsService.BillInfoFilePath, null, true);
                            }
                        }));
                    }
                    else
                    {
                        if (!ProcessAuthCashier(e.NfcCode))
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                                defaultWindow.ShowErrorMessage("Картка клієнта не активована");
                            }));
                        }
                    }
                }
            }
            //auth cashier
            else if (app.Token != null)
            {
                ProcessAuthCashier(e.NfcCode);
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
