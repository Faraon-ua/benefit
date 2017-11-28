using System.Windows;
using System.Windows.Controls;
using Benefit.Common.CustomEventArgs;

namespace Benefit.CardReader.Controls
{
    /// <summary>
    /// Interaction logic for KeyboardPartial.xaml
    /// </summary>
    public partial class KeyboardPartial : UserControl
    {
        public delegate void OkKeyPressedEventHandler(object sender, ButtonEventArgs e);

        public event OkKeyPressedEventHandler OkKeyPressed;
        public string ControlName { get; set; }

        public void SendOkKeyPressed()
        {
            if (this.OkKeyPressed != null)
            {
                this.OkKeyPressed(this, new ButtonEventArgs(ControlName, txtValue.Text));
            }
        }
        public KeyboardPartial()
        {
            InitializeComponent();
        }

        private void CloseWindow_OnClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void KeyButton_OnClick(object sender, RoutedEventArgs e)
        {
            var key = (sender as Button).Tag.ToString();
            if (key == "." && txtValue.Text.Contains("."))
                return;
            if (key == "Bsp")
            {
                if (txtValue.Text.Length == 0)
                {
                    return;
                }
                txtValue.Text = txtValue.Text.Substring(0, txtValue.Text.Length - 1);
                return;
            }
            if (txtValue.Text.Contains("."))
            {
                var index = txtValue.Text.LastIndexOf(".");
                if (index != -1 && (txtValue.Text.Length - index) > 2)
                {
                    return;
                }
            }
            txtValue.Text += key;
        }

        public void OkKey_OnClick(object sender, RoutedEventArgs e)
        {
            SendOkKeyPressed();
            Visibility = Visibility.Hidden;
        }
    }
}
