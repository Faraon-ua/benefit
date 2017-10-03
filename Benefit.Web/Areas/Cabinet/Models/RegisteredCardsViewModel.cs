using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.Web.Areas.Cabinet.Models
{
    public class RegisteredCardsViewModel
    {
        public List<BenefitCard> Registered { get; set; } 
        public List<BenefitCard> Available { get; set; } 
    }
}