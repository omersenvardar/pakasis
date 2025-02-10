using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.ViewModels
{
    public class LoginViewModel
    {
    [Required(ErrorMessage = "E-posta veya telefon numarası gereklidir.")]
    [StringLength(115)]
    public string EmailOrPhone { get; set; } = string.Empty; // Kullanıcı hem e-posta hem telefon girebilir.

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [DataType(DataType.Password)]
    [StringLength(255)]
    public string Sifre { get; set; }= string.Empty;
    }
}
