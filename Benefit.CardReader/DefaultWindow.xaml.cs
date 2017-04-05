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
        public ApiService apiService;
        public App app;

        private SerialPort _serialPort;
        private char[] _readSymbols;
        private Dictionary<ViewType, Control> _controls = new Dictionary<ViewType, Control>();
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
