using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Benefit.Domain.Models
{
    public class Personnel
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "І'мя обовязково для заповнення")]
        [MaxLength(32)]
        public string Name { get; set; }
        [MaxLength(16)]
        public string Phone { get; set; }
        [MaxLength(10)]
        public string CardNumber { get; set; }
        [MaxLength(10)]
        [Required(ErrorMessage = "Неправильний номер картки")]
        public string NFCCardNumber { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [MaxLength(20)]
        public string RoleName { get; set; }
    }
}
