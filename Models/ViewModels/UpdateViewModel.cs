using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.ViewModels
{
    public class UpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Ad { get; set; }

        [Required]
        [StringLength(50)]
        public string? Soyad { get; set; }

        [StringLength(15)]
        public string? Telefon { get; set; }

        [StringLength(255)]
        public string? FirmaUnvani { get; set; }

        [StringLength(255)]
        public string? Referans { get; set; }
    }
}
