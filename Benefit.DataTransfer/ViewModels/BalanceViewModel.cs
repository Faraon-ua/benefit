using Benefit.Domain.Models;
using System.Collections.Generic;

namespace Benefit.DataTransfer.ViewModels
{
    public class BalanceViewModel
    {
        public Seller Seller { get; set; }
        public PaginatedList<SellerTransaction> SellerTransactions { get; set; }
        public string OrderNumber { get; set; }
        public string ProductSKU { get; set; }
        public SellerTransactionType? TransactionType { get; set; }
        public string DateRange { get; set; }
     }
}
