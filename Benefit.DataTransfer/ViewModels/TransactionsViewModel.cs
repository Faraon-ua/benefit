using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class PartnerTransactionsViewModel
    {
        public PartnerTransactionsViewModel()
        {
            General = new List<Transaction>();
            Personal = new List<Transaction>();
            Referals = new List<Transaction>();
        }
        public ApplicationUser User { get; set; }
        public string DateRange { get; set; }
        public List<Transaction> General { get; set; } 
        public List<Transaction> Personal { get; set; } 
        public List<Transaction> Referals { get; set; } 
    }
}
