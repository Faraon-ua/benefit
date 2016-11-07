using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Benefit.Domain.Models
{
    public class CurrencyComparer : IEqualityComparer<Currency>
    {
        public bool Equals(Currency x, Currency y)
        {
            if (x.Name == y.Name && x.Provider == y.Provider)
            {
                return true;
            }
            return false;
        }
        public int GetHashCode(Currency codeh)
        {
            return (codeh.Id).GetHashCode();
        }
    }
    public class Currency
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string Name { get; set; }
        [Required]
        [MaxLength(16)]
        public string Provider { get; set; }
        [Required]
        public double? Rate { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }

        [NotMapped]
        public string ExpandedName
        {
            get
            {
                return Name == "UAH" ? Name : string.Format("{0} ({1})", Name, Provider);
            }
        }
    }
}
