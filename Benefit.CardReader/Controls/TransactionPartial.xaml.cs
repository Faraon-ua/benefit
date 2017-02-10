using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Benefit.CardReader.Controls
{
    /// <summary>
    /// Interaction logic for TransactionPartial.xaml
    /// </summary>
    public partial class TransactionPartial : UserControl
    {
        public TransactionPartial()
        {
            InitializeComponent();
        }

        private void BtnUserInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void PaymentSum_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var newChar = e.Text[e.Text.Length - 1];
            var text = (sender as TextBox).Text;
            if (newChar == '.' && text == string.Empty)
                e.Handled = true;
            if (!(char.IsDigit(newChar) || (newChar =='.' && !text.Contains("."))))
                e.Handled = true;
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
        }
    }
}
