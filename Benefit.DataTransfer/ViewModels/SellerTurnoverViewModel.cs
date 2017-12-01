using System.Collections.Generic;

namespace Benefit.DataTransfer.ViewModels
{
    public class SellerTurnover
{
        public string SellerName { get; set; }
        public double SellerTotalDiscount { get; set; }
        public double SiteTurnover { get; set; }
        public double SiteTurnoverWithoutBonuses { get; set; }
        public double CardsTurnover { get; set; }
        public double BonusesOnlineTurnover { get; set; }
        public double BonusesOffineTurnover { get; set; }
        public double BCcomission { get; set; }
}
    public class SellerTurnoverViewModel
    {
        public SellerTurnoverViewModel()
        {
            SellerTurnovers = new List<SellerTurnover>();
        }
        public List<SellerTurnover> SellerTurnovers { get; set; }
        public string DateRange { get; set; }
        public string RegionId { get; set; }
    }
}
