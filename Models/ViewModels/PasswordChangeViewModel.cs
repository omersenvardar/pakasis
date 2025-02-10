using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

    using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.ViewModels
{
    public class PasswordChangeViewModel
    {
        public int Id { get; set; } // Kullanıcı ID'si

        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre tekrar gereklidir.")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}