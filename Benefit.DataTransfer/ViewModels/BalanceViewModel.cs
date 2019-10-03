using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ViewModels
{
    public class BalanceViewModel
    {
        public Seller Seller { get; set; }
        public List<SellerTransaction> SellerTransactions { get; set; }
        public string OrderNumber { get; set; }
        public string ProductSKU { get; set; }
        public SellerTransactionType? TransactionType { get; set; }
        public string DateRange { get; set; }

     }
}
