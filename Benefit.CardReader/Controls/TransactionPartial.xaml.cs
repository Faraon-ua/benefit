﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.CardReader.Services;
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
        public TransactionPartial()
        {
            InitializeComponent();
            app = (App)App.Current;
            apiService = new ApiService();
        }

        private void BtnUserInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void BtnPayBonuses_OnClick(object sender, RoutedEventArgs e)
        {
            var billNumber = txtBillNumber.Text == txtBillNumber.Tag.ToString() ? null : txtBillNumber.Text;
            double paymentSum = 0;
            try
            {
                paymentSum = double.Parse(txtPaymentSum.Text);
            }
            catch
            {
                var parent = Window.GetWindow(this) as DefaultWindow;
                parent.ShowErrorMessage("Невірний формат суми");
                return;
            }
            var paymentIngest = new PaymentIngest
            {
                CashierNfc = app.AuthInfo.CashierNfc,
                UserNfc = app.AuthInfo.UserNfc,
                Sum = paymentSum,
                BillNumber = billNumber
            };
            apiService.AuthToken = app.Token;
            var result = apiService.ProcessPayment(paymentIngest);
            if (result == null)
            {
                var parent = Window.GetWindow(this) as DefaultWindow;
                parent.ShowErrorMessage("Нарахування не було проведено");
            }
            else
            {
                BonusesAquired.Text = result.BonusesAcquired.ToString("F");
                BonusesAccount.Text = result.BonusesAccount.ToString("F");
                TransactionPanel.Visibility = Visibility.Hidden;
                TransactionResult.Visibility = Visibility.Visible;
            }
        }

        private void TransactionOk_OnClick(object sender, RoutedEventArgs e)
        {
            TransactionResult.Visibility = Visibility.Hidden;
            TransactionPanel.Visibility = Visibility.Visible;
            txtBillNumber.Text = txtBillNumber.Tag.ToString();
            txtPaymentSum.Text = txtPaymentSum.Tag.ToString();

            var parent = Window.GetWindow(this) as DefaultWindow;
            parent.ShowSingleControl(ViewType.UserAuthPartial);
            app.ClearUserAndSellerInfo();
        }

        private void BtnChargeBonuses_OnClick(object sender, RoutedEventArgs e)
        {
                
        }
    }
}