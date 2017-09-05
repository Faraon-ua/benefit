using System.Collections.Generic;
using Benefit.Domain.Models.Service;

namespace Benefit.DataTransfer.ViewModels
{
    public class ProcessBonusesViewModel
    {
        public List<PartnerReckon> Partners { get; set; }
        public List<PartnerReckon> VipPartners { get; set; }
        public int ActiveBuyersCount { get; set; }
        public double PersonalBonusesAccrued { get; set; }
    }
}
