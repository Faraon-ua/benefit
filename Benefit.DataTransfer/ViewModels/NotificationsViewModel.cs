using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class NotificationsViewModel
    {
        public int Total { get; set; }
        public int Reviews { get; set; }
        public List<Seller> NewSellerContent { get; set; }
    }
}
