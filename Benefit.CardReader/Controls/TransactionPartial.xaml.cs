using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.CardReader.Services;
using Benefit.CardReader.Services.Factories;
using Benefit.Common.Extensions;

namespace Benefit.CardReader.Controls
{
    /// <summary>
    /// Interaction logic for TransactionPartial.xaml
    /// </summary>
    public partial class TransactionPartial : UserControl
    {
        private App app;
        private ApiService apiService;
        private DefaultWindow defaultWindow;
        public TransactionPartial()
        {
            InitializeComponent();
            app = (App)App.Current;
            apiService = new ApiService();
            Loaded += TransactionPartial_Loaded;
        }

        void TransactionPartial_Loaded(object sender, RoutedEventArgs e)
        {
            defaultWindow = Window.GetWindow(this) as DefaultWindow;
        }

        private void ProcessPayment(bool chargeBonuses = false)
        {
            var billNumber = txtBillNumber.Text == txtBillNumber.Tag.ToString() ? null : txtBillNumber.Text;
            double paymentSum = 0;
            try
            {
                paymentSum = double.Parse(txtPaymentSum.Text, CultureInfo.InvariantCulture);
            }
            catch
            {
                defaultWindow.ShowErrorMessage("Введіть суму чека");
                return;
            }
            defaultWindow.LoadingSpinner.Visibility = Visibility.Visible;
            var paymentIngest = new PaymentIngest
            {
                CashierNfc = app.AuthInfo.CashierNfc,
                UserNfc = app.AuthInfo.UserNfc,
                Sum = paymentSum,
                BillNumber = billNumber,
                ChargeBonuses = chargeBonuses
            };
            apiService.AuthToken = app.Token;

            var task = Task.Factory.StartNew(() => ReaderFactory.GetReaderManager(app.IsConnected).ProcessPayment(paymentIngest));
            task.ContinueWith(x =>
            {
                var result = x.Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    defaultWindow.LoadingSpinner.Visibility = Visibility.Hidden;
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        defaultWindow.ShowErrorMessage(result.ErrorMessage ?? "Нарахування не було проведено");
                    }
                    else
                    {
                        if (chargeBonuses)
                        {
                            PaymentStatus.Text = "Списання пройшло успішно!";
                            BonusesCharged.Text = result.Data.BonusesCharged.ToString("F");
                            BonusesChargedPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            PaymentStatus.Text = app.IsConnected
                                ? "Нарахування проведено!"
                                : "Нарахування буде проведено в онлайн режимі";
                            BonusesChargedPanel.Visibility = Visibility.Collapsed;
                        }
                        if (!app.IsConnected)
                        {
                            BonusesAquired.Visibility = Visibility.Hidden;
                            BonusesAccount.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            BonusesAquired.Visibility = Visibility.Visible;
                            BonusesAccount.Visibility = Visibility.Visible;
                            BonusesAquired.Text = result.Data.BonusesAcquired.GetValueOrDefault(0).ToString("F");
                            BonusesAccount.Text = result.Data.BonusesAccount.GetValueOrDefault(0).ToString("F");
                        }
                        TransactionPanel.Visibility = Visibility.Hidden;
                        TransactionResult.Visibility = Visibility.Visible;
                    }
                }
                    ));
            });
        }

        public void BtnUserInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            apiService.AuthToken = app.Token;
            var user = apiService.GetUserInfo(app.AuthInfo.UserNfc);
            var parent = Window.GetWindow(this) as DefaultWindow;
            var userInfoWindow = parent.UserInfo;
            userInfoWindow.UserName.Text = user.Name;
            userInfoWindow.UserCard.Text = user.CardNumber;
            userInfoWindow.BonusesAccount.Text = user.BonusesAccount.ToString("F");
            userInfoWindow.TransactionTime.Text = user.LastCardOrderInfo.Time.ToLocalDateTimeWithFormat();
            userInfoWindow.TransactionSeller.Text = user.LastCardOrderInfo.SellerName;
            userInfoWindow.TransactionSum.Text = user.LastCardOrderInfo.Sum + " грн";
            userInfoWindow.Visibility = Visibility.Visible;
        }

        private void PaymentSum_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var newChar = e.Text[e.Text.Length - 1];
            if (!(char.IsDigit(newChar)))
            {
                e.Handled = true;
            }
            else
            {
                if ((sender as TextBox).Text.Length >= 2)
                {
                    var oldText = (sender as TextBox).Text.Replace(".", "");
                    var newText = oldText + newChar;
                    (sender as TextBox).Text = newText.Insert(newText.Length - 2, ".");
                    (sender as TextBox).CaretIndex = (sender as TextBox).Text.Length;
                    e.Handled = true;
                }
            }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == textBox.Tag.ToString())
            {
                textBox.Text = string.Empty;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == string.Empty)
            {
                textBox.Text = textBox.Tag.ToString();
            }
        }

        public void BtnPayBonuses_OnClick(object sender, RoutedEventArgs e)
        {
            btnPayBonuses.Focus();
            ProcessPayment();
        }

        public void BtnChargeBonuses_OnClick(object sender, RoutedEventArgs e)
        {
            btnChargeBonuses.Focus();
            ProcessPayment(true);
        }

        public void TransactionOk_OnClick(object sender, RoutedEventArgs e)
        {
            TransactionResult.Visibility = Visibility.Hidden;
            TransactionPanel.Visibility = Visibility.Visible;
            txtBillNumber.Text = txtBillNumber.Tag.ToString();
            txtPaymentSum.Text = txtPaymentSum.Tag.ToString();

            var parent = Window.GetWindow(this) as DefaultWindow;
            parent.ShowSingleControl(ViewType.UserAuthPartial);
            app.ClearUserAndSellerInfo();
        }
    }
}
