using System.ComponentModel.DataAnnotations;
using Benefit.Domain.Models.Enums;

namespace Benefit.DataTransfer.ViewModels
{
    public class AnketaViewModel
    {
        [Required(ErrorMessage = "Вкажіть вид діяльності")]
        public string BusinessType { get; set; }
        [Required]
        public bool HasEcommerceWebSite { get; set; }
        public string WebSiteAddress { get; set; }
        [Required]
        public SellerStatus Status { get; set; }
        [Required(ErrorMessage = "Вкажіть ПІБ")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Вкажіть назву компанії або ФОП")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Вкажіть номер телефону")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Вкажіть email адресу")]
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
