using System.ComponentModel.DataAnnotations;

namespace Benefit.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        public int? ReferalNumber { get; set; }
        [Required]
        [MaxLength(64)]
        public string FullName { get; set; }
        public int? RegionId { get; set; }
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required]
        [MaxLength(64)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
    
    public class RegisterViewModel
    {
        public int? ReferalNumber { get; set; }
        [MaxLength(60)]
        [Required(ErrorMessage = "Ім'я та прізвище є обов'язковими для заповнення")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Місто є обов'язковим для заповнення")]
        public string RegionName { get; set; }
        public int? RegionId { get; set; }
        public string ShippingAddress { get; set; }
        [Required(ErrorMessage = "Номер телефону є обов'язковим для заповнення")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Email є обов'язковим для заповнення")]
        [MaxLength(64)]
        [EmailAddress(ErrorMessage = "Невірно вказана Email адреса")]
        public string Email { get; set; }
        [MaxLength(10, ErrorMessage = "Задовгий номер картки")]        
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим для заповнення")]
        [StringLength(100, ErrorMessage = "Пароль має бути не менший за 4 символи", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обов’язковий для заповнення")]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Пароль обов’язковий для заповнення")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запам’ятати мене?")]
        public bool RememberMe { get; set; }
    }

   

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
