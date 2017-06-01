using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ViewModels
{
    public class PromotionAccomplisher
    {
        public string UserFullName { get; set; }
        public int AccomplishmentNumber { get; set; }
    }
     public class PromotionAccomplishementsViewModel
    {
         public string PromotionName { get; set; }
         public string PromotionId{ get; set; }
         public List<PromotionAccomplisher> Accomplishers { get; set; }
    }
}
