using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Benefit.CardReader.Services;
using System.IO.Ports;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;
using System.Windows.Interop;
using Benefit.CardReader.DataTransfer.Reader;

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
        public ApiService apiService;
        public App app;

        private SerialPort _serialPort;
        private char[] _readSymbols;
        private Dictionary<ViewType, Control> _controls = new Dictionary<ViewType, Control>();
        private ViewType ActiveView { get; set; }
        private ServiceHost communicationServiceHost = null;
        public DefaultWindow()
        {
            InitializeComponent();
            Title = "Benefit NFC Reader";
            Taskbar.IconSource = this.Icon;

            app = (App)Application.Current;

            apiService = new ApiService();
            //add all views
            _controls.Add(ViewType.CashierPartial, CashierPartial);
            _controls.Add(ViewType.UserAuthPartial, UserAuthPartial);
            _controls.Add(ViewType.TransactionPartial, TransactionPartial);
            _controls.Add(ViewType.DeviceNotConnected, DeviceNotConnected);
            _controls.Add(ViewType.DeviceNotAuthorized, DeviceNotAuthorized);

            KeyDown += DefaultWindow_KeyDown;
            Loaded += DefaultWindow_Loaded;
        }

        void DefaultWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        void DefaultWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                TransactionPartial.txtPaymentSum.Focus();
            }
            if (e.Key == Key.F2)
            {
                if (TransactionPartial.txtBillNumber.IsEnabled)
                {
                    TransactionPartial.txtBillNumber.Focus();
                }
            }
            if (e.Key == Key.F3)
            {
                TransactionPartial.BtnPayBonuses_OnClick(null, null);
            }
            if (e.Key == Key.F12)
            {
                TransactionPartial.BtnChargeBonuses_OnClick(null, null);
            }
            if (e.Key == Key.F4)
            {
                TransactionPartial.btnUserInfo.Focus();
                TransactionPartial.BtnUserInfo_OnMouseLeftButtonDown(null, null);
            }
            if (e.Key == Key.Escape)
            {
                if (ErrorMessage.Visibility == Visibility.Visible || UserInfo.Visibility == Visibility.Visible || KeyboardPartial.Visibility == Visibility.Visible)
                {
                    ErrorMessage.Visibility = Visibility.Hidden;
                    UserInfo.Visibility = Visibility.Hidden;
                    KeyboardPartial.Visibility = Visibility.Hidden;
                }
                else
                {
                    Hide();
                }
            }
            if (e.Key == Key.Enter)
            {
                if (KeyboardPartial.Visibility == Visibility.Visible)
                {
                    KeyboardPartial.OkKey_OnClick(null, null);
                }
                if (TransactionPartial.TransactionResult.Visibility == Visibility.Visible)
                {
                    TransactionPartial.TransactionOk_OnClick(null, null);
                }
            }
        }

        public void ShowErrorMessage(string errorMessage)
        {
            ErrorMessage.Visibility = Visibility.Visible;
            ErrorMessage.txtError.Text = errorMessage;
        }

        public void ShowSingleControl(ViewType? viewType = null, bool track = true)
        {
            ErrorMessage.Visibility = Visibility.Hidden;
            var view = viewType.GetValueOrDefault(ActiveView);
            _controls.Select(entry => entry.Value).ToList().ForEach(entry => entry.Visibility = Visibility.Hidden);
            _controls[view].Visibility = Visibility.Visible;
            if (track)
            {
                ActiveView = view;
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void OpenWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void CloseApp_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Taskbar_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }
    }
}
