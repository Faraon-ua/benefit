using System.Collections.Generic;

namespace Benefit.DataTransfer.ViewModels
{
    public class PromotionAccomplisher
    {
        public string UserFullName { get; set; }
        public int AccomplishmentNumber { get; set; }
        public int Level { get; set; }
    }
    public class PromotionAccomplishementsViewModel
    {
        public string PromotionName { get; set; }
        public string PromotionId { get; set; }
        public List<PromotionAccomplisher> Accomplishers { get; set; }
    }
}
