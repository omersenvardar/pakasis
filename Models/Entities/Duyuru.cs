using System;
using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.Entities
{
    public class Duyuru
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Baslik { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? Icerik { get; set; }
        
        public int Durumu { get; set; }

        // DuyuruImg, longtext türünde, bu yüzden string olarak bırakıldı
        public string? DuyuruImg { get; set; }

        // OlusturmaTarihi ve GuncellenmeTarihi datetime(6) olarak ayarlanmış, 
        // DateTime türü, hassasiyet ve tarih/saat verisini doğru şekilde tutar.
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public DateTime? GuncellenmeTarihi { get; set; }

        [Required]
        public string? OlusturanKullanici { get; set; }
    }
}
