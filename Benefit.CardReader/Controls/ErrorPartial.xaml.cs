using System.Windows;
using System.Windows.Controls;

namespace Benefit.CardReader.Controls
{
    /// <summary>
    /// Interaction logic for ErrorPartial.xaml
    /// </summary>
    public partial class ErrorPartial : UserControl
    {
        public ErrorPartial()
        {
            InitializeComponent();
        }

        private void CloseErrorMessage_OnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
