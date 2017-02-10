using System.Windows;
using Benefit.CardReader.DataTransfer;

namespace Benefit.CardReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public bool IsConnected { get; set; }
        public string CashierName { get; set; }
        public string SellerName { get; set; }
        public string Token { get; set; }
    }
}
