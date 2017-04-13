using System;

namespace Benefit.Domain.Models
{
    public class CompanyRevenue
    {
        public string Id { get; set; }
        public DateTime Stamp { get; set; }
        public double TotalBonuses { get; set; }
        public double TotalHangingBonuses { get; set; }
        public double TotalEarnedBonuses { get; set; }
    }
}
