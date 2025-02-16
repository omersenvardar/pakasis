using System;

namespace DBGoreWebApp.Models.ViewModels
{
    public class IlanEmlakViewModel
    {
        public int IlanNo { get; set; } // İlan Numarası
        public string IlanAdi { get; set; } = string.Empty; // İlan Başlığı
        public string Il { get; set; } = string.Empty; // İl Bilgisi
        public string Ilce { get; set; } = string.Empty; // İlçe Bilgisi
        public string Mahalle { get; set; } = string.Empty; // Mahalle Bilgisi
        public int? FiyatSatis { get; set; } // Satış Fiyatı
        public string? ResimUrl { get; set; } // İlk Resim URL
        public char Durum { get; set; } // İlan Durumu
        public DateTime CreatedDate { get; set; } // Oluşturma Tarihi
    }
}
