using System.Collections.Generic;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
    public class ArabaViewModel
    {
        public List<Kullanici>? Kullanicilar { get; set; }
        public List<Adres>? Adresler { get; set; }
        public List<Araba>? Arabalar { get; set; }
        public List<ArabaResim>? ArabaResimler { get; set; }
        public List<AracMarkalari>? AracMarkalaris { get; set; }
        public List<AracModelListesi>? AracModelListesis { get; set; }
        public List<AracModelYillari>? AracModelYillaris { get; set; }
        public List<AracYilVersiyon>? AracYilVersiyons { get; set; }

        public Araba? ArabaDetay { get; set; }
        public Kullanici? KullaniciDetay { get; set; }
        public AracMarkalari? AracMarkalari { get; set; }
        public AracModelListesi? AracModelListesi { get; set; }
        public AracModelYillari? AracModelYillari { get; set; }
        public AracYilVersiyon? AracYilVersiyon { get; set; }
        public int MarkaID { get; set; }
        public int ModelID { get; set; }
        public int YilID { get; set; }
        public int VersiyonID { get; set; }
        public int kullaniciId { get; set; }
        public int AdresKonumu { get; set; }
        public int Fiyat { get; set; }
        public string? MarkaAdi { get; set; }
        public string? ModelAdi { get; set; }
        public string? YilAdi { get; set; }
        public string? VersiyonAdi { get; set; }
        public string? Ozellikleri { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? MahalleKoyAdi { get; set; }
        public Adres? Adres { get; set; }
    }
}
