using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class PartnerTransactionsViewModel
    {
        public PartnerTransactionsViewModel()
        {
            Transactions = new List<Transaction>();
        }
        public ApplicationUser User { get; set; }
        public string DateRange { get; set; }
        public List<Transaction> Transactions { get; set; } 
    }
}
