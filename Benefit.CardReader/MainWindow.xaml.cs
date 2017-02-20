﻿using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Benefit.CardReader.Services;
using Benefit.HttpClient;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan _minLoadTime = new TimeSpan(0, 0, 5);//sec

        private DefaultWindow defaultWindow;
        private BenefitHttpClient httpClient;
        private ApiService apiService;
        public ReaderService readerService;
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
            apiService = new ApiService();
            readerService = new ReaderService();
            readerService.CardReaded += readerService_CardReaded;
            app = (App)Application.Current;
        }

/*        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }*/

        public void CloseComPort()
        {
            readerService.CloseComPort();
        }
        private void readerService_CardReaded(object sender, Common.CustomEventArgs.NfcEventArgs e)
        {
            SystemSounds.Beep.Play();

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
                    var user = apiService.AuthUser(e.NfcCode);
                    if (user != null)
                    {
                        app.AuthInfo.UserNfc = e.NfcCode;
                        app.AuthInfo.UserName = user.Name;
                        app.AuthInfo.UserCard = user.CardNumber;

                        Dispatcher.Invoke(new Action(() =>
                        {
                            defaultWindow.TransactionPartial.SellerName.Text = app.AuthInfo.SellerName;
                            defaultWindow.TransactionPartial.CashierName.Text = app.AuthInfo.CashierName;
                            defaultWindow.TransactionPartial.UserName.Text = user.Name;
                            defaultWindow.TransactionPartial.UserCard.Text = user.CardNumber;
                            defaultWindow.ShowSingleControl(ViewType.TransactionPartial);
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() => defaultWindow.ShowErrorMessage("Картка клієнта не активована")));
                    }
                }
            }
                //auth cashier
            else if (app.Token != null)
            {
                var cashierSeller = apiService.AuthCashier(e.NfcCode);
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
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() => defaultWindow.ShowErrorMessage("Картка касира не активована")));
                }
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            startTime = DateTime.Now;
            //connection bar
            var isconnected = httpClient.CheckForInternetConnection();
            if (isconnected)
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Visible;
                defaultWindow.NoConnection.Visibility = Visibility.Collapsed;
            }
            else
            {
                defaultWindow.ConnectionEsteblished.Visibility = Visibility.Collapsed;
                defaultWindow.NoConnection.Visibility = Visibility.Visible;
            }

            //view bar
            var handShake = readerService.HandShake();
            if (handShake == null)
            {
                //show no device connected
                defaultWindow.ShowSingleControl(ViewType.DeviceNotConnected);
            }
            else
            {
                //get license key from device;
                app.Token = apiService.GetAuthToken(handShake.LicenseKey);
                apiService.AuthToken = app.Token;
                if (app.Token == null)
                {
                    //show device not authorized
                    defaultWindow.ShowSingleControl(ViewType.DeviceNotAuthorized);
                }
                else
                {
                    defaultWindow.ShowSingleControl(ViewType.CashierPartial);
                }
            }

            ((App)Application.Current).IsConnected = isconnected;
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