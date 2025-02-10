using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DBGoreWebApp.Models.ViewModels
{
    public class RegisterViewModel
{
    public int Id { get; set; } // Id alanını ekleyin
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string Soyad { get; set; } = string.Empty;

    [Phone]
    public string? Telefon { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Sifre { get; set; } = string.Empty;

    [Required]
    [Compare("Sifre", ErrorMessage = "Şifreler uyuşmuyor.")]
    public string ConfirmSifre { get; set; } = string.Empty;

    public string? FirmaUnvani { get; set; }
    public string? Referans { get; set; }
    public IFormFile? ImgUrl { get; set; }
    public string? ImgUrlStr { get; set; }

    public char Durum { get; set; } = 'p';
    public string Rol { get; set; } = "üye";
    public string AbonelikTipi { get; set; } = "yok";
}

}
