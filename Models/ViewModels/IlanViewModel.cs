using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
    public class IlanViewModel
{
    public int Id { get; set; }
    public string? IlanAdi { get; set; }
    public string? Adres { get; set; }
    public int? Fiyat { get; set; }
    public string? ResimUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? IlanTuru { get; set; } // "Emlak" veya "Araba" gibi değerler alacak
    public List<EmlakBahce>? EmlakBahceler { get; set; } // Emlak ilanları
    public List<Araba>? Arabalar { get; set; } // Araba ilanları
    public List<AracMarkalari>? AracMarkalaris { get; set; }
    public List<AracModelListesi>? AracModelListesis { get; set; }
    public List<AracModelYillari>? aracModelYillaris { get; set; }
    public int? MarkaID { get; set; }
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public string? Yil { get; set; }
    public int? ModelID { get; set; }
    public int? YuzOlcum { get; set; }

    public string? Turu { get; set; }
    public string? TaksitBedeli { get; set; }
    public string? TaksitSayisi { get; set; }
    public string? Kapora { get; set; }
    public string TaksitMesaji
    {
        get
        {
            if (int.TryParse(TaksitBedeli, out int bedel) && int.TryParse(TaksitSayisi, out int sayisi))
            {
                int toplamFiyat = bedel * sayisi;
                return $"Bu bahçeyi {toplamFiyat:N0} ₺ fiyata, {Kapora} ₺ peşinat ve {bedel:N0} ₺'den başlayan taksitlerle alabilirsiniz.";
            }
            return string.Empty;
        }
    }
}

}