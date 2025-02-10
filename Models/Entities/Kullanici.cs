using System;
using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.Entities
{
    public class Kullanici
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Ad { get; set; }

        [Required]
        [StringLength(50)]
        public string? Soyad { get; set; }

        [StringLength(15)]
        public string? Telefon { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        public string? ImgUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string? SifreHash { get; set; }

        [StringLength(255)]
        public string? Rol { get; set; }

        [StringLength(255)]
        public string? FirmaUnvani { get; set; }

        [StringLength(255)]
        public string? Referans { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public char Durum { get; set; } = 'p'; // Durum (Varsayılan: 'p')

        public string? AbonelikTipi { get; set; }

        public DateTime? AbonelikBaslangicTarihi { get; set; }

        public DateTime? AbonelikBitisTarihi { get; set; }

        public string? AbonelikStatusu { get; set; }

        public DateTime? SonKullanimTarihi { get; set; }
        // Kullanıcı ile Emlak İlanları arasındaki ilişki
        public virtual ICollection<EmlakBahce>? EmlakIlanlari { get; set; }
        public virtual ICollection<Araba>? ArabaIlanlari { get; set; }
    }
}
