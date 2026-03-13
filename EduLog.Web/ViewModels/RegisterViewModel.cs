using System.ComponentModel.DataAnnotations;

namespace EduLog.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        [MaxLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sınıf katılım kodu gereklidir.")]
        [MaxLength(10)]
        [Display(Name = "Sınıf Katılım Kodu")]
        public string JoinCode { get; set; } = string.Empty;
    }
}
