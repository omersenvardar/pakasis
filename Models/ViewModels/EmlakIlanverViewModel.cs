using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
    public class EmlakIlanverViewModel
    {
        public List<Adres>? Adresler { get; set; } // Adres bilgilerini çekmek için
        public List<ArsaResim>? ArsaResimleri { get; set; } // Bu alan boş geçilebilir olmalı
        public int? Id { get; set; } // Güncelleme için gerekli
        public int? IlanNo { get; set; } // Otomatik atanacak
        public int? KullaniciId { get; set; } // ilanı ekleyen kullanıcın ID'si

        [Required]
        [MaxLength(100)]
        public string IlanAdi { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Il { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Ilce { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string MahalleKoyAdi { get; set; } = string.Empty;

        public int? AdresKonumu { get; set; } // Bu alan kontrolörde doldurulacak

        public int? Ada { get; set; }
        public short? Parsel { get; set; }
        public int? YuzOlcum { get; set; }
        public int? Hisse { get; set; }
        public string Turu { get; set; } = string.Empty;
        public short? BagceSayisi { get; set; }
        public short? BahceSayisiSatilan { get; set; }
        public DateTime? TarihBas { get; set; }
        public DateTime? TarihBitis { get; set; }
        public DateTime? TarihTeslim { get; set; }
        public int? FiyatSatis { get; set; }
        public int? FiyatPesin { get; set; }
        public int? Kapora { get; set; }
        public string? Taksitlimi { get; set; }
        public string? TaksitBedeli { get; set; }
        public string? TaksitSayisi { get; set; }

        [Required]
        public string IlanAciklamasi { get; set; } = string.Empty;

        public List<IFormFile>? Resimler { get; set; } = new List<IFormFile>(); // Bu alan boş geçilebilir olmalı
        public string? ParselSorgu { get; set; }
        public char Durum { get; set; } 
    }
}