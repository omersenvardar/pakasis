using System;

namespace DBGoreWebApp.Models.Entities
{
    public class Arsa
    {
        
        public int Id { get; set; } // Arsa kimlik numarası
        public int KullaniciId { get; set; }
        public int AdresId { get; set; }
        public string? Il { get; set; } // Arsa'nın bulunduğu il
        public string? Ilce { get; set; } // Arsa'nın bulunduğu ilçe
        public string? Mahalle { get; set; } // Arsa'nın bulunduğu mahalle
        public string? KonutTuru { get; set; } // Konut Türü (Arsa, Konut, Hobi Bahçeleri, İşyeri)vs.
        public string? Tip { get; set; } // Konut tipi (Satılık, Kiralık)
        public decimal Fiyat { get; set; } // Arsa'nın fiyatı
        public double Alan { get; set; } // Arsa'nın alanı (m² cinsinden)
        public string? Aciklama { get; set; } // Arsa hakkında açıklama
        public DateTime IlanTarihi { get; set; } // Arsanın ilan tarihi
        public bool SatildiMi { get; set; } // Arsa'nın satılıp satılmadığı durumu
        
        // İlişkilendirilmiş diğer özellikler
        public int SahibId { get; set; } // Arsa sahibinin kimlik numarası
        public Sahib? Sahib { get; set; } // Arsa sahibinin bilgileri
    }
    
    public class Sahib
    {
        public int Id { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
    }
}
