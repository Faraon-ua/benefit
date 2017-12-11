using System.Windows;
using System.Windows.Controls;

namespace Benefit.CardReader.Controls
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : UserControl
    {
        public Loading()
        {
            InitializeComponent();
        }

        private void Loading_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadImg.Spin = IsVisible;
        }
    }
}
