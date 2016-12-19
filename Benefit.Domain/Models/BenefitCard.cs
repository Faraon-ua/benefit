using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public class BenefitCard
    {
        //written number
        [Required]
        [MaxLength(10)]
        public string Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string NfcCode { get; set; }
    }
}
