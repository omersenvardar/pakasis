using System.Collections.Generic;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
    public class CompositeViewModel
    {
        public List<EmlakBahce> EmlakBahceler { get; set; } = new List<EmlakBahce>();
        public List<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
        public List<ArsaResim> ArsaResimler { get; set; } = new List<ArsaResim>();
        public List<Adres> Adresler { get; set; } = new List<Adres>();
        public ArabaViewModel? ArabaViewModels { get; set; }
        public List<Araba>? Arabalar { get; set; }
        public List<ArabaResim>? ArabalarResimler { get; set; }

        public EmlakBahce? EmlakDetay { get; set; }
        public Kullanici? KullaniciDetay { get; set; }
        public ArsaResim? ArsaResimDetay { get; set; }
        public Adres? AdresDetay { get; set; } 
    public int AdresKonumuNavigation { get; set; }
        // Sayfalama bilgileri
    public int CurrentArabaPage { get; set; }
    public int TotalArabaPages { get; set; }
    public int CurrentEmlakPage { get; set; }
    public int TotalEmlakPages { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
    
}
