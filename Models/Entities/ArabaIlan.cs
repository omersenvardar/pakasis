using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    
    public class ArabaIlan
    {
        [Key]
        public int Id { get; set; }

        public int KullaniciId { get; set; }

        [StringLength(100)]
        public string? ArabaMarka { get; set; } // Araç Markası

        [StringLength(50)]
        public string? ArabaTur { get; set; } // Araç Türü (SUV, Sedan, vb.)

        [StringLength(55)]
        public string? ArabaModel { get; set; } // Araç Modeli
        [StringLength(55)]
        public string? ArabaSeri { get; set; } // Araç Serisi

        public int ArabaModelYili { get; set; } // Araç Model Yılı

        public int ArabaKm { get; set; } // Araç Kilometresi

        [StringLength(50)]
        public string? ArabaYakit { get; set; } // Yakıt Türü (Benzin, Dizel, vb.)

        [StringLength(50)]
        public string? ArabaRenk { get; set; } // Araç Rengi

        [StringLength(255)]
        public string? ArabaResim { get; set; } // Araç Resim URL'si

        [StringLength(50)]
        public string? ArabaVites { get; set; } // Vites Türü (Manuel, Otomatik, vb.)

        public double ArabaFiyat { get; set; } // Araç Fiyatı

        [StringLength(500)]
        public string? ArabaAciklama { get; set; } // Araç Açıklaması

        [StringLength(50)]
        public string? ArabaKasaTipi { get; set; } // Kasa Tipi (Sedan, Hatchback, vb.)

        [StringLength(10)]
        public string? ArabaKoltukSayisi { get; set; } // Koltuk Sayısı

        [StringLength(50)]
        public string? ArabaMotorGucu { get; set; } // Motor Gücü (BHP)

        [StringLength(50)]
        public string? ArabaMotorHacmi { get; set; } // Motor Hacmi (CC)

        [StringLength(50)]
        public string? ArabaCekis { get; set; } // Çekiş Tipi (Önden Çekişli, Arkadan İtişli, vb.)

        [StringLength(500)]
        public string? ArabaOzellikler { get; set; } // Araç Özellikleri (Klima, ABS, vb.)

        public DateTime IlanTarihi { get; set; } // İlan Tarihi

        public bool SatildiMi { get; set; } // Satış Durumu
    }
}
